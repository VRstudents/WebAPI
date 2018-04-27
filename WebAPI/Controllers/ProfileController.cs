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
                LoginController.checkOnAccess(this.Request.Headers);
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
                            where sc.StudentId == studentId
                            join c in db.ClassGroups on sc.ClassId equals c.Id
                            select c;

                foreach (var item in query)
                {
                    profile.sCourses.Add(new SCourses
                    {
                        CourseId = item.Id,
                        Category = item.Category,
                        Grade = item.Grade
                    });
                };

                var query2 = from sc in db.StudentsToClasses
                             join c in db.ClassGroups on sc.ClassId equals c.Id
                             join pc in db.ProgressInClasses on c.Id equals pc.ClassId
                             where sc.StudentId == studentId && pc.StudentId == studentId
                             orderby c.Grade descending, pc.Result
                             select pc;

                foreach (var item in query2)
                {
                    foreach (var item2 in profile.sCourses)
                    {
                        if (item.ClassId == item2.CourseId)
                        {
                            item2.Result = item.Result;
                            break;
                        };
                    };
                };

                //Clases and assignments tab
                var query3 = from sc in db.StudentsToClasses
                             join lc in db.LessonsToClasses on sc.ClassId equals lc.ClassId
                             where sc.StudentId == studentId && lc.IsActive == true
                             join l in db.Lessons on lc.LessonId equals l.Id
                             orderby l.SeqNum descending
                             select new { lc, l };

                foreach (var item in query3.ToList())
                {
                    var query4 = from rl in db.ResultInLessons
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
                        Attempts = query4.Count(),
                        BestRes = query4.FirstOrDefault()
                    });
                };

                profile.sClasses.OrderByDescending(x => x.LessonNum).OrderBy(y => y.Attempts);

                //Achievements tab
                profile.qCorrAnswered = StatisticsController.QCorrAnswered(studentId);
                profile.lessonsCompleted = StatisticsController.LessonsCompleted(studentId);

                var query5 = from pc in db.ProgressInClasses
                             where pc.StudentId == studentId && pc.Result != 0
                             orderby pc.Result descending
                             select pc;

                //If a record for the student in the class exists
                if (query5.Any())
                {
                    profile.avgCourseRes = query5.Average(x => x.Result);
                    profile.bestCourseRes = query5.First().Result;
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

        [HttpGet]
        [Route("api/Profile/TeacherProfilePage/{UserName}")]
        public TeacherProfile TeacherProfilePage(string userName)
        {
            try
            {
                LoginController.checkOnAccess(this.Request.Headers);
            }
            catch (Exception ex)
            {
                throw ex;
            };

            TeacherProfile profile = new TeacherProfile();
            profile.categories = new List<string>();
            profile.classes = new List<SCourses>();
            int teacherId = LoginController.GetUserID(userName, "teacher");
            var db = new DBModel();

            try
            {
                //Subjects list
                profile.categories = (from tcat in db.TeacherCategories
                                      where tcat.TeacherId == teacherId
                                      select tcat.Category).ToList();

                //Classes list (CLassID, NAME, GRADE,)
                profile.classes = (from tcl in db.TeachersToClasses
                                   where tcl.TeacherId == teacherId
                                   join cg in db.ClassGroups on tcl.ClassId equals cg.Id
                                   orderby cg.Category, cg.Grade
                                   select new SCourses
                                   {
                                       CourseId = cg.Id,
                                       Category = cg.Category,
                                       Grade = cg.Grade
                                   }).ToList();

            }
            catch (Exception ex)
            {
                throw ex;
            };

            return profile;
        }
    }
}
