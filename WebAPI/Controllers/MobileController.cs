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
        /*------------------------------------------------------------------------------------------------------------------------
        Method to authenticate mobile user against his unique authentication code in DB
        ------------------------------------------------------------------------------------------------------------------------*/
        [HttpPost]
        [Route("api/Mobile/Auth")]
        public PersonDTO Auth([FromBody]string code)
        {
            var db = new DBModel();

            var query = (from u in db.Users
                         where u.Code == code
                         select u).Any();

            if (query)
            {
                var query2 = (from u in db.Users
                             where u.Code == code
                             select u).First();

                return new PersonDTO() {
                    Id = query2.Id,
                    Name = query2.Name
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
                         select new ClassGroupDTO
                         {
                             Id = c.Id,
                             Category = c.Category
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
        Method to get classe lessons. The method is triggered when student enters a class in the game.
        ------------------------------------------------------------------------------------------------------------------------*/
        [HttpGet]
        [Route("api/Mobile/GetLessons/{ClassId}/{StudentId}")]
        public List<StudentLessonDTO> GetLessons(int classId, int studentId)
        {
            var db = new DBModel();

            var query = from lc in db.LessonsToClasses
                        join l in db.Lessons on lc.LessonId equals l.Id
                        join pc in db.ProgressInClasses on lc.ClassId equals pc.ClassId
                        where lc.ClassId == classId && pc.StudentId == studentId
                        select new StudentLessonDTO
                        {
                            Id = l.Id,
                            Name = l.Name,
                            SeqNum = l.SeqNum,
                            IsActive = lc.IsActive,
                            FinishedLessonNum = pc.FinishedLessonNum
                        };

            if (query.Any())
            {
                return query.ToList();
            }

            else
            {
                throw new Exception("No lessons found");
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
                var query = from pc in db.ProgressInClasses
                            join lc in db.LessonsToClasses on pc.ClassId equals lc.ClassId
                            where pc.StudentId == data.StudentId && lc.LessonId == data.LessonId

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