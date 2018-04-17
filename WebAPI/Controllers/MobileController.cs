using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Web.Http;
using System.Web.Http.Cors;
using WebAPI.Models;
using WebAPI.Models.App;
using WebAPI.Models.App.JSONFormat;

namespace WebAPI.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class MobileController : ApiController
    {
        const int MIN_RES_TO_PASS = 5;

        /*------------------------------------------------------------------------------------------------------------------------
        Method to authenticate mobile user against his unique authentication code in DB
        ------------------------------------------------------------------------------------------------------------------------*/
        [HttpPost]
        [Route("api/Mobile/Auth")]
        public PersonDTO Auth([FromBody]string code)
        {
            if (code == null)
            {
                throw new Exception("Code cannot be null");
            }

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

                return new PersonDTO() {
                    Id = query2.Id,
                    Name = query2.Name,
                    SchoolName = query3.First(),
                    Grade = query2.Grade,
                    Picture = ""
                };
            }

            else
            {
                throw new Exception("Code is not valid");
            }
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
                throw new Exception("You are not registered to any class");
            }
        }

        /*------------------------------------------------------------------------------------------------------------------------
        Method to get class lessons. The method is triggered when student enters a class in the game.
        ------------------------------------------------------------------------------------------------------------------------*/
        [HttpGet]
        [Route("api/Mobile/GetLessons/{ClassId}/{StudentId}")]
        public List<StudentLessonDTO> GetLessons(int classId, int studentId)
        {
            var db = new DBModel();

            try
            {
                var query = from lc in db.LessonsToClasses
                            join l in db.Lessons on lc.LessonId equals l.Id
                            where lc.ClassId == classId
                            select new StudentLessonDTO
                            {
                                Id = l.Id,
                                Name = l.Name,
                                SeqNum = l.SeqNum,
                                IsActive = lc.IsActive,
                                IsPassed = false
                            };

                if (query.Any())
                {
                    List<StudentLessonDTO> myLessons = query.ToList();

                    foreach (var item in myLessons) 
                    {
                        var query2 = from rl in db.ResultInLessons
                                     where rl.StudentId == studentId && rl.LessonId == item.Id && rl.Result >= MIN_RES_TO_PASS
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
                    throw new Exception("No lessons found");
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
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
            }
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
                //Progress in class is only calculated if the result of the lesson is PASSES (more then MIN_RES_TO_PASS)
                if (data.Result >= MIN_RES_TO_PASS)
                {
                    //Get progress in class record for the student in the class
                    var query = from pc in db.ProgressInClasses
                                join lc in db.LessonsToClasses on pc.ClassId equals lc.ClassId
                                where pc.StudentId == data.StudentId
                                select pc;

                    var query2 = from l in db.Lessons
                                 where l.Id == data.LessonId
                                 select l.SeqNum;

                    //If previous record of progress in class for the student exists
                    if (query.Any())
                    {
                        query.First().Result = (query.First().Result * query.First().FinishedLessonNum + data.Result) / (query.First().FinishedLessonNum + 1);
                        
                        if (query.First().FinishedLessonNum < query2.First())
                        {
                            query.First().FinishedLessonNum = query2.First();
                        };
                    }

                    //If no previous record of progress in the class for the student exists
                    else
                    {
                        var query3 = from lc in db.LessonsToClasses
                                     where lc.LessonId == data.LessonId
                                     select lc.ClassId;

                        ProgressInClass pcData = new ProgressInClass();
                        pcData.ClassId = query3.First();
                        pcData.StudentId = data.StudentId;
                        pcData.FinishedLessonNum = query2.First();
                        pcData.Result = data.Result;
                        db.ProgressInClasses.Add(pcData);

                        data.Attempts = 1;
                    }
                }

                //Updating attempts number based on existing records
                var query4 = from rl in db.ResultInLessons
                             where rl.StudentId == data.StudentId
                             orderby rl.Attempts descending
                             select rl.Attempts;
                data.Attempts = query4.FirstOrDefault() + 1;

                //Saving the changes
                db.ResultInLessons.Add(data);
                db.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
}
    }
}