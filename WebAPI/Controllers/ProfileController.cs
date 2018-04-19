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
    public class ProfileController : ApiController
    {
        [HttpGet]
        [Route("api/Profile/StudentProfilePage/{UserName}")]
        public StudentProfile StudentProfilePage(string userName)
        {
            try
            {
                bool validation = LoginController.checkOnAccess(this.Request.Headers);
                if (!validation)
                {
                    throw new AccessDeniedExc();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            };

            StudentProfile profile = new StudentProfile();
            profile.sCourses = new List<SCourses>();
            profile.sClasses = new List<SClasses>();            
            int studentId = LoginController.GetUserID(userName, "student");
            var db = new DBModel();

            try
            {
                //Courses tab
                var query = from sc in db.StudentsToClasses
                            join c in db.ClassGroups on sc.ClassId equals c.Id
                            join pc in db.ProgressInClasses on c.Id equals pc.ClassId
                            where sc.StudentId == studentId && pc.StudentId == studentId
                            orderby c.Grade descending, pc.Result
                            select new { c, pc };

                foreach (var item in query)
                {
                    profile.sCourses.Add(new SCourses
                    {
                        CourseId = item.c.Id,
                        Category = item.c.Category,
                        Grade = item.c.Grade,
                        Result = item.pc.Result
                    });
                };

                //Clases and assignments tab
                var query2 = from sc in db.StudentsToClasses
                             join lc in db.LessonsToClasses on sc.ClassId equals lc.ClassId
                             where sc.StudentId == studentId && lc.IsActive == true
                             join l in db.Lessons on lc.LessonId equals l.Id
                             orderby l.SeqNum descending
                             select new { lc, l };

                foreach (var item in query2.ToList())
                {
                    var query3 = from rl in db.ResultInLessons
                                 where rl.StudentId == studentId && rl.LessonId == item.l.Id
                                 orderby rl.Result descending
                                 select rl.Result;

                    profile.sClasses.Add(new SClasses
                    {
                        CourseId = item.lc.ClassId,
                        Category = item.l.Category,
                        LessonNum = item.l.SeqNum,
                        Name = item.l.Name,
                        Description = item.l.Description,
                        Attempts = query3.Count(),
                        BestRes = query3.FirstOrDefault()
                    });
                };

                profile.sClasses.OrderByDescending(x => x.LessonNum).OrderBy(y => y.Attempts);

                //Achievements tab
                profile.qCorrAnswered = StudyController.qCorrAnswered(studentId);
                profile.lessonsCompleted = StudyController.lessonsCompleted(studentId);

                var query4 = from pc in db.ProgressInClasses
                             where pc.StudentId == studentId && pc.Result != 0
                             orderby pc.Result descending
                             select pc;

                //If a record for the student in the class exists
                if (query4.Any())
                {
                    profile.avgCourseRes = query4.Average(x => x.Result);
                    profile.bestCourseRes = query4.First().Result;
                }
                //If no previous record exists
                else
                {
                    profile.avgCourseRes = profile.bestCourseRes = 0;
                };
            }
            catch (Exception ex)
            {
                throw ex;
            };

            return profile;
        }

    }
}
