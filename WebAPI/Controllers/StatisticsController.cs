using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Cors;
using WebAPI.Models.App.JSONFormat;
using WebAPI.Models;
using WebAPI.Models.App;
using WebAPI.Models.App.Statistics;

namespace WebAPI.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class StatisticsController : ApiController
    {
        internal const int NUM_OF_QUESTIONS_IN_LESSON = 10;
        internal const int NUM_OF_LESSONS_IN_CLASS = 10;

        [HttpGet]
        [Route("api/Statistics/LessonStats/{ClassGroupId}/{LessonNum}")]
        public LessonStats LessonStats(int classGroupId, int lessonNum)
        {
            try
            {
                LoginController.checkOnAccess(this.Request.Headers);
            }
            catch (Exception ex)
            {
                throw ex;
            };

            var db = new DBModel();
            LessonStats lStats = new LessonStats
            {
                QLDist = new List<QResLessonDistribution>(),
                Students = new List<PersonDTO>()
            };

            try
            {
                //Get all the records of the results in the lesson
                var results = from lc in db.LessonsToClasses
                              where lc.ClassId == classGroupId
                              join l in db.Lessons on lc.LessonId equals l.Id
                              where l.SeqNum == lessonNum
                              join rl in db.ResultInLessons on l.Id equals rl.LessonId
                              select rl;

                if (results.Any())
                {
                    //Get max result by student
                    var studentsBestResults = from r in results
                                              group r by r.StudentId into stdRes
                                              select stdRes.Max(x => x.Result);

                    //Count how many students achieved PASS result
                    lStats.StFinished = 0;
                    foreach (var stdRes in studentsBestResults)
                    {
                        if (stdRes >= StudyController.MIN_RES_TO_PASS)
                        {
                            lStats.StFinished++;
                        };
                    };

                    //Getting questions results
                    int lessonId = GetLessonId(lessonNum, classGroupId);
                    var query = from rq in db.ResultInQuestions
                                where rq.LessonId == lessonId
                                group rq by rq.QuestionNum into qRes
                                select qRes;

                    foreach (var question in query)
                    {
                        lStats.QLDist.Add(new QResLessonDistribution
                        {
                            QNum = question.Key,
                            RightCount = 0,
                            WrongCount = 0
                        });

                        foreach (var res in question)
                        {
                            if(res.Result == true)
                            {
                                lStats.QLDist.Last().RightCount++; 
                            }
                            else
                            {
                                lStats.QLDist.Last().WrongCount++;
                            };
                        };
                    };

                    //Adding empty question objects to fill up the list to 10
                    for (int i = lStats.QLDist.Count; i < NUM_OF_QUESTIONS_IN_LESSON; i++)
                    {
                        lStats.QLDist.Add(new QResLessonDistribution()
                        {
                            QNum = i + 1,
                            RightCount = 0,
                            WrongCount = 0
                        });
                    };

                    lStats.StTried = studentsBestResults.Count();
                    lStats.FinishedOfTriedPercent = (double) lStats.StFinished / (double) lStats.StTried;
                    lStats.AvgBestRes = studentsBestResults.Average();
                    lStats.AvgRes = results.Average(x => x.Result);

                    //Build students list
                    lStats.Students = GetStudentsList(classGroupId);
                }

                else
                {
                    lStats.SetDefaults();
                };

                return lStats;
            }
            catch (Exception ex)
            {
                throw ex;
            };
        }

        [HttpGet]
        [Route("api/Statistics/ClassStats/{ClassGroupId}")]
        public ClassStats ClassStats(int classGroupId)
        {
            try
            {
                LoginController.checkOnAccess(this.Request.Headers);
            }
            catch (Exception ex)
            {
                throw ex;
            };

            var db = new DBModel();
            ClassStats cStats = new ClassStats
            {
                RDist = new List<LessonResDistribution>(),
                ADist = new List<LessonAttempts>()
            };

            //try
            //{
                //Get all the records of the results in the class grouped by lesson, then grouped by student
                var results = from lc in db.LessonsToClasses
                              where lc.ClassId == classGroupId
                              join l in db.Lessons on lc.LessonId equals l.Id
                              join rl in db.ResultInLessons on l.Id equals rl.LessonId
                              group rl by l.SeqNum into lRes
                              select new {
                                  lRes.Key,
                                  studentsRes = from lrg in lRes
                                  group lrg by lrg.StudentId into studRes
                                  select studRes
                              };

                if (results.Any())
                {
                    List<double> avgRes;
                    List<double> avgBestRes;

                    /*------------------------------------------------------------------------------------------
                    *                                 Lesson Results Distribution
                    *------------------------------------------------------------------------------------------*/
                    //Loop through each lesson
                    foreach (var lesson in results)
                    {
                        avgRes = new List<double>();
                        avgBestRes = new List<double>();

                        //Create lesson object
                        cStats.RDist.Add(new LessonResDistribution
                        {
                            LNum = lesson.Key
                        });

                        //Loop through each student's results in the lesson and calc his average and max results
                        foreach (var res in lesson.studentsRes)
                        {
                            avgRes.Add(res.Average(x => x.Result));
                            avgBestRes.Add(res.Max(x => x.Result));
                        };

                        //Calc overall 'average' and 'average best' results
                        cStats.RDist.Last().AvgRes = avgRes.Average();
                        cStats.RDist.Last().AvgBestRes = avgBestRes.Average();                        
                    };

                    //Adding empty lessons objects to fill up the list to 10
                    for (int i = cStats.RDist.Count; i < NUM_OF_LESSONS_IN_CLASS; i++)
                    {
                        cStats.RDist.Add(new LessonResDistribution()
                        {
                            LNum = i + 1,
                            AvgRes = 0,
                            AvgBestRes = 0
                        });
                    };

                    /*------------------------------------------------------------------------------------------
                    *                               Lesson attempts and completion distribution
                    *------------------------------------------------------------------------------------------*/
                    int passed;

                    //Loop through each lesson
                    foreach (var lesson in results)
                    {
                        passed = 0;

                        //Create lesson object
                        cStats.ADist.Add(new LessonAttempts
                        {
                            LNum = lesson.Key
                        });

                        //Check each student's results to see if he passed the lesson
                        foreach (var res in lesson.studentsRes)
                        {
                            if (res.Max(x => x.Result >= StudyController.MIN_RES_TO_PASS))
                            {
                                passed++;
                            };
                        };

                        cStats.ADist.Last().StFinished = passed;
                        cStats.ADist.Last().StTried = lesson.studentsRes.Count();
                    };

                    //Adding empty lessons objects to fill up the list to 10
                    for (int i = cStats.ADist.Count; i < NUM_OF_LESSONS_IN_CLASS; i++)
                    {
                        cStats.ADist.Add(new LessonAttempts()
                        {
                            LNum = i + 1,
                            StFinished = 0,
                            StTried = 0
                        });
                    };

                    /*-------------------------------------------------------------------------------------------
                    *                               Course average result
                    *------------------------------------------------------------------------------------------*/
                    var query = from pc in db.ProgressInClasses
                                where pc.ClassId == classGroupId
                                select pc.Result;
                    if(query.Any())
                    {
                        cStats.AvgRes = (double)query.Average();
                    }

                    else
                    {
                        cStats.AvgRes = 0;
                    };
                }

                else
                {
                    cStats.AvgRes = 0;
                };

                return cStats;
            //}
            //catch (Exception ex)
            //{
            //    throw ex;
            //};
        }

        [HttpGet]
        [Route("api/Statistics/GetStudentsList/{ClassGroupId}")]
        public List<PersonDTO> GetStudentsList(int classGroupId)
        {
            var db = new DBModel();
            return (from sc in db.StudentsToClasses
                    where sc.ClassId == classGroupId
                    join s in db.Students on sc.StudentId equals s.Id
                    orderby s.Name
                    select new PersonDTO
                    {
                        Id = s.Id,
                        Name = s.Name
                    }).ToList();
        }

        [HttpGet]
        [Route("api/Statistics/StudentStats/{ClassGroupId}/{StudentId}")]
        public StudentStats StudentStats(int classGroupId, int studentId)
        {
            try
            {
                LoginController.checkOnAccess(this.Request.Headers);
            }
            catch (Exception ex)
            {
                throw ex;
            };

            var db = new DBModel();
            StudentStats sStats = new StudentStats
            {
                RDist = new List<LessonResDistribution>()
            };

            try
            {
                //Get all the records of the results in the class grouped by lesson, then grouped by student
                var results = from lc in db.LessonsToClasses
                              where lc.ClassId == classGroupId
                              join l in db.Lessons on lc.LessonId equals l.Id
                              join rl in db.ResultInLessons on l.Id equals rl.LessonId
                              where rl.StudentId == studentId
                              group rl by l.SeqNum into lRes
                              select lRes;

                if (results.Any())
                {
                    /*------------------------------------------------------------------------------------------
                    *                                 Lesson Results Distribution
                    *------------------------------------------------------------------------------------------*/
                    //Loop through each lesson and assign results
                    foreach (var lesson in results)
                    {
                        sStats.RDist.Add(new LessonResDistribution
                        {
                            LNum = lesson.Key,
                            AvgRes = lesson.Average(x => x.Result),
                            BestRes = lesson.Max(x => x.Result)
                        });
                    };

                    //Adding empty lessons objects to fill up the list to 10
                    for (int i = sStats.RDist.Count; i < NUM_OF_LESSONS_IN_CLASS; i++)
                    {
                        sStats.RDist.Add(new LessonResDistribution()
                        {
                            LNum = i + 1,
                            AvgRes = 0,
                            BestRes = 0
                        });
                    };

                    /*-------------------------------------------------------------------------------------------
                    *                               Course average result
                    *------------------------------------------------------------------------------------------*/
                    sStats.AvgRes = (from pc in db.ProgressInClasses
                                     where pc.ClassId == classGroupId && pc.StudentId == studentId
                                     select pc.Result).First();
                }

                else
                {
                    sStats.AvgRes = 0;
                };

                return sStats;
            }
            catch (Exception ex)
            {
                throw ex;
            };
        }

        [HttpGet]
        [Route("api/Statistics/LessonInStudentStats/{ClassGroupId}/{StudentId}/{LessonNum}")]
        public List<QResLessonDistribution> LessonInStudentStats(int classGroupId, int studentId, int lessonNum)
        {
            try
            {
                LoginController.checkOnAccess(this.Request.Headers);
            }
            catch (Exception ex)
            {
                throw ex;
            };

            var db = new DBModel();
            List<QResLessonDistribution> qDist = new List<QResLessonDistribution>();
            
            try
            {
                int lessonId = GetLessonId(lessonNum, classGroupId);
                var results = from rq in db.ResultInQuestions
                              where rq.LessonId == lessonId && rq.StudentId == studentId
                              group rq by rq.QuestionNum into qRes
                              select qRes;

                if (results.Any())
                {
                    foreach (var lesson in results)
                    {
                        qDist.Add(new QResLessonDistribution
                        {
                            QNum = lesson.Key,
                            RightCount = 0,
                            WrongCount = 0
                        });

                        foreach (var res in lesson)
                        {
                            if (res.Result == true)
                            {
                                qDist.Last().RightCount++;
                            }
                            else
                            {
                                qDist.Last().WrongCount++;
                            };
                        }
                    };

                    //Adding empty question objects to fill up the list to 10
                    for (int i = qDist.Count; i < NUM_OF_QUESTIONS_IN_LESSON; i++)
                    {
                        qDist.Add(new QResLessonDistribution()
                        {
                            QNum = i + 1,
                            RightCount = 0,
                            WrongCount = 0
                        });
                    };
                };

                return qDist;
            }
            catch (Exception ex)
            {
                throw ex;
            };
        }

        /*===================================================================================
          Internal functions
        ===================================================================================*/
        internal static int LessonsCompleted(int studentId)
        {
            var db = new DBModel();
            return (from rl in db.ResultInLessons
                    where rl.StudentId == studentId && rl.Result >= StudyController.MIN_RES_TO_PASS
                    group rl by rl.LessonId into lessonRes
                    select lessonRes).Count();
        }

        internal static int QCorrAnswered(int studentId)
        {
            var db = new DBModel();
            return (from rq in db.ResultInQuestions
                    where rq.StudentId == studentId && rq.Result
                    select rq).Count();
        }

        internal static int GetLessonId(int lessonNum, int courseId)
        {
            var db = new DBModel();
            return (from lc in db.LessonsToClasses
                    where lc.ClassId == courseId
                    join l in db.Lessons on lc.LessonId equals l.Id
                    where l.SeqNum == lessonNum
                    select l.Id).First();
        }
    }
}
