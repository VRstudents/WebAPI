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
                QLDist = new List<QResLessonDistribution>()
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
                    }

                    lStats.StTried = studentsBestResults.Count();
                    lStats.FinishedOfTriedPercent = (double) lStats.StFinished / (double) lStats.StTried;
                    lStats.AvgBestRes = studentsBestResults.Average();
                    lStats.AvgRes = results.Average(x => x.Result);
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
