using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Cors;
using WebAPI.Exceptions;
using WebAPI.Models;
using WebAPI.Models.App;

namespace WebAPI.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class MobileController : ApiController
    {
        /*------------------------------------------------------------------------------------------------------------------------
        Method to authenticate mobile user against his unique authentication code in DB
        ------------------------------------------------------------------------------------------------------------------------*/
        [HttpPost]
        [Route("api/Mobile/Auth")]
        public PersonDTO Auth([FromBody]string code)
        {
            if (code == null)
            {
                throw new InvalidCode();
            };

            var db = new DBModel();

            var query = (from u in db.Users
                         where u.Code == code
                         select u).Any();

            if (query)
            {
                var query2 = (from u in db.Users
                             where u.Code == code
                             join s in db.Students on u.UserName equals s.UserName
                             select s).First();

                var query3 = from st in db.Students
                             where st.UserName == query2.UserName
                             join sc in db.Schools on st.SchoolId equals sc.Id
                             select sc.Name;

                var query4 = (from u in db.Users
                              where u.Code == code
                              select u.Picture).First();

                return new PersonDTO() {
                    Id = query2.Id,
                    Name = query2.Name,
                    SchoolName = query3.First(),
                    Grade = query2.Grade,
                    Picture = query4
                };
            }

            else
            {
                throw new InvalidCode();
            };
        }

        /*------------------------------------------------------------------------------------------------------------------------
        Method to get student's classes. The method is triggered when student logins into the game.
        ------------------------------------------------------------------------------------------------------------------------*/
        [HttpGet]
        [Route("api/Mobile/GetClasses/{StudentId}")]
        public List<ClassGroupDTO> GetClasses(int studentId)
        {
            var db = new DBModel();

            var query = from sc in db.StudentsToClasses
                        join c in db.ClassGroups on sc.ClassId equals c.Id
                        where sc.StudentId == studentId
                        join tc in db.TeachersToClasses on c.Id equals tc.ClassId
                        join t in db.Teachers on tc.TeacherId equals t.Id
                        orderby c.Category
                        select new ClassGroupDTO
                        {
                            Id = c.Id,
                            Category = c.Category,
                            Teacher = t.Name
                        };

            if (query.Any())
            {
                return query.ToList();
            }

            else
            {
                throw new NotRegisteredToClasses();
            };
        }

        /*------------------------------------------------------------------------------------------------------------------------
        Method to get class lessons. The method is triggered when menu scene is loaded, once to load each course wall.
        ------------------------------------------------------------------------------------------------------------------------*/
        [HttpGet]
        [Route("api/Mobile/GetLessons/{ClassId}/{StudentId}")]
        public List<LessonDTO> GetLessons(int classId, int studentId)
        {
            var db = new DBModel();

            try
            {
                var query = from lc in db.LessonsToClasses
                            join l in db.Lessons on lc.LessonId equals l.Id
                            where lc.ClassId == classId
                            select new LessonDTO
                            {
                                Id = l.Id,
                                Name = l.Name,
                                SeqNum = l.SeqNum,
                                IsActive = lc.IsActive,
                                IsPassed = false
                            };

                if (query.Any())
                {
                    List<LessonDTO> myLessons = query.ToList();

                    foreach (var item in myLessons) 
                    {
                        var query2 = from rl in db.ResultInLessons
                                     where rl.StudentId == studentId && rl.LessonId == item.Id && rl.Result >= StudyController.MIN_RES_TO_PASS
                                     select rl;

                        if (query2.Any())
                        {
                            item.IsPassed = true;
                        }
                    }

                    return myLessons;
                }

                else
                {
                    throw new NoLessonsInClass();
                };
            }
            catch (Exception ex)
            {
                throw ex;
            };
        }

        /*------------------------------------------------------------------------------------------------------------------------
        Method to get statistical info. Triggered on clicking of "Get Info" button.
        ------------------------------------------------------------------------------------------------------------------------*/
        [HttpGet]
        [Route("api/Mobile/GetInfo/{StudentId}")]
        public Info GetInfo(int studentId)
        {
            var db = new DBModel();
            Info myInfo = new Info();
            myInfo.best = new List<LessonDTO>();
            myInfo.worst = new List<LessonDTO>();

            try
            {
                myInfo.lessonsCompleted = StatisticsController.LessonsCompleted(studentId);
                myInfo.qCorrAnswered = StatisticsController.QCorrAnswered(studentId);

                var query = from rl in db.ResultInLessons
                            where rl.StudentId == studentId
                            join l in db.Lessons on rl.LessonId equals l.Id
                            group rl by new
                            {
                                l.Id,
                                l.Category,
                                l.Name
                            } into lessonRes
                            select new LessonDTO
                            {
                                Id = lessonRes.Key.Id,
                                Category = lessonRes.Key.Category,
                                Name = lessonRes.Key.Name,
                                Result = lessonRes.Max(x => x.Result)
                            };

                myInfo.best = query.OrderByDescending(x => x.Result).Take(3).ToList();

                var query2 = from rl in db.ResultInLessons
                             where rl.StudentId == studentId
                             join l in db.Lessons on rl.LessonId equals l.Id
                             group rl by new
                             {
                                 l.Id,
                                 l.Category,
                                 l.Name
                             } into lessonRes
                             select new LessonDTO
                             {
                                 Id = lessonRes.Key.Id,
                                 Category = lessonRes.Key.Category,
                                 Name = lessonRes.Key.Name,
                                 Result = lessonRes.Min(x => x.Result)
                             };

                myInfo.worst = query2.OrderBy(x => x.Result).Take(3).ToList();

                return myInfo;
            }
            catch (Exception ex)
            {
                throw ex;
            };
        }

        /*------------------------------------------------------------------------------------------------------------------------
        Method to get teachers' messages. Triggered on clicking of "Get Messages" button.
        ------------------------------------------------------------------------------------------------------------------------*/
        [HttpGet]
        [Route("api/Mobile/GetMsgs/{StudentId}")]
        public List<MessageDTO> GetMsgs(int studentId)
        {
            var db = new DBModel();

            try
            {
              return (from sc in db.StudentsToClasses
                      where sc.StudentId == studentId                      
                      join c in db.ClassGroups on sc.ClassId equals c.Id
                      join m in db.Messages on sc.ClassId equals m.ClassId
                      orderby c.Category
                      select new MessageDTO
                      {
                        Text = m.Text,
                        Date = m.Date,
                        Category = c.Category
                      }).ToList();
            }
            catch (Exception ex)
            {
                throw ex;
            };
        }

        /*------------------------------------------------------------------------------------------------------------------------
        Method to get weekly challenges. Triggered on clicking of "Challenge Yourself" button.
        ------------------------------------------------------------------------------------------------------------------------*/
        [HttpGet]
        [Route("api/Mobile/GetChallenge/{StudentId}")]
        public List<ExamDTO> GetChallenge(int studentId)
        {
            List<ExamDTO> exams = new List<ExamDTO>();
            var db = new DBModel();

            try
            {
                var query = from sc in db.StudentsToClasses
                            where sc.StudentId == studentId
                            join e in db.Exams on sc.ClassId equals e.ClassId
                            join eq in db.ExamQuestions on e.QuestionId equals eq.Id
                            select new
                            {
                                e.ClassId,
                                eq
                            } into examQuestions
                            group examQuestions by examQuestions.ClassId;

                foreach (var category in query)
                {
                    exams.Add(new ExamDTO{
                        ClassId = category.Key,
                        Category = category.First().eq.Category,
                        Questions = new List<ExamQuestionsMDTO>()
                    });

                    foreach (var question in category)
                    {
                        AnswerVar[] vars = new AnswerVar[]
                        {
                            new AnswerVar(question.eq.AnswerA, false),
                            new AnswerVar(question.eq.AnswerB, false),
                            new AnswerVar(question.eq.AnswerC, false),
                            new AnswerVar(question.eq.AnswerD, false),
                        };

                        vars[question.eq.RightAnswer - 1].IsRight = true;

                        exams.Last().Questions.Add(new ExamQuestionsMDTO
                        {
                            Id = question.eq.Id,
                            Question = question.eq.Question,
                            AnswerA = vars[0],
                            AnswerB = vars[1],
                            AnswerC = vars[2],
                            AnswerD = vars[3]
                        });
                    };
                };

                return exams.OrderBy(x => x.Category).ToList();
            }
            catch (Exception ex)
            {
                throw ex;
            };
        }

        /*------------------------------------------------------------------------------------------------------------------------
        Method to update result of the student in each question of each lesson.
        The method is called on each answer and gets the result - right or wrong.
        ------------------------------------------------------------------------------------------------------------------------*/
        [HttpPost]
        [Route("api/Mobile/QuestionAnswer")]
        public bool QuestionAnswer([FromBody]ResultInQuestion data)
        {
            var db = new DBModel();

            try
            {
                db.ResultInQuestions.Add(data);
                db.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            };
        }

        /*------------------------------------------------------------------------------------------------------------------------
        Method to update result of the student in a lesson and progression in the class.
        The method is called on the end of a lesson.
        The method receives the number of right answers, updates ResultInLesson and ProgressInClass tables.
        ------------------------------------------------------------------------------------------------------------------------*/
        [HttpPost]
        [Route("api/Mobile/FinishLesson")]
        public bool FinishLesson([FromBody]ResultInLesson data)
        {
            var db = new DBModel();

            try
            {
                //Get class ID of the lesson
                var query = (from lc in db.LessonsToClasses
                             where lc.LessonId == data.LessonId
                             select lc.ClassId).First();

                //Get progress in class record for the student in the class
                var query2 = from pc in db.ProgressInClasses
                             join lc in db.LessonsToClasses on pc.ClassId equals lc.ClassId
                             where pc.StudentId == data.StudentId && pc.ClassId == query
                             select pc;

                //If no previous record of progress in the class for the student exists
                if (!query2.Any())
                {
                    ProgressInClass myProg = new ProgressInClass();
                    myProg.ClassId = query;
                    myProg.StudentId = data.StudentId;
                    myProg.Result = (double)data.Result;
                    db.ProgressInClasses.Add(myProg);
                }
                //If previous record of progress in the class for the student exists
                else
                {
                    //Get previous results of the student in the lesson
                    var query3 = from rl in db.ResultInLessons
                                 where rl.StudentId == data.StudentId
                                 select rl.Result;

                    //Calculating and saving new average result
                    double res = (query3.Sum() + (double)data.Result) / (query3.Count() + 1);
                    query2.First().Result = res;
                };
                  
                //Adding result in lesson and saving all the changes
                db.ResultInLessons.Add(data);
                db.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            };
        }
    }
}