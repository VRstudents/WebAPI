using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Web.Http;
using System.Web.Http.Cors;
using WebAPI.Models.App.JSONFormat;
using WebApplication1.Models;
using WebApplication1.Models.App;

namespace WebApplication1.Controllers
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
                List<ClassGroupDTO> myClasses = new List<ClassGroupDTO>();

                foreach (var classGroup in query)
                {
                    myClasses.Add(classGroup);
                }

                return myClasses;
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

            var query = from lc in db.LessonToClass
                        join l in db.Lessons on lc.LessonId equals l.Id
                        join pc in db.ProgressInClass on lc.ClassId equals pc.ClassId
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
                List<StudentLessonDTO> myLessons = new List<StudentLessonDTO>();

                foreach (var lesson in query)
                {
                    myLessons.Add(lesson);
                }

                return myLessons;
            }

            else
            {
                throw new Exception("No lessons found");
            }
        }
    }
}