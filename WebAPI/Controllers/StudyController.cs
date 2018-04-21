using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Cors;
using WebAPI.Models.App.JSONFormat;
using WebAPI.Models;
using WebAPI.Models.App;
using WebAPI.Exceptions;

namespace WebAPI.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class StudyController : ApiController
    {
        internal const int MIN_RES_TO_PASS = 5;

        [HttpGet]
        [Route("api/Study/GetStudentsNamesByClassGroup/{ClassGroupId}")]
        public StudentToClassesDTO GetStudentsNamesByClassGroup(int classGroupId)
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

            try
            {
                var query = from sg in db.StudentsToClasses
                            join s in db.Students on sg.StudentId equals s.Id
                            join c in db.ClassGroups on sg.ClassId equals c.Id
                            where sg.ClassId == classGroupId
                            select new
                            {
                                c.Category,
                                c.Grade,
                                s.Name
                            } into result
                            group result by new
                            {
                                result.Category,
                                result.Grade
                            };

                StudentToClassesDTO studentsNamesInClassGroup = new StudentToClassesDTO(query.First().Key.Category, query.First().Key.Grade);

                foreach (var student in query.First())
                {
                    studentsNamesInClassGroup.Students.Add(student.Name);
                }

                return studentsNamesInClassGroup;
            }
            catch (InvalidOperationException ex)
            {
                if (ex.Message == "Sequence contains no elements")
                {
                    return new StudentToClassesDTO("No such class group ot the class is empty", 0);
                }

                else
                    return new StudentToClassesDTO("An error accured", 0);
            };
        }

        [HttpGet]
        [Route("api/Study/ClassesToJoin/{UserName}")]
        public List<ClassGroupDTO> JoinClass(string userName)
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
            List <ClassGroupDTO> classesToJoin = new List<ClassGroupDTO>();
            List<ClassGroupDTO> tempList =  new List<ClassGroupDTO>();

            try
            {
                int schoolId = GetSchoolId(userName);                
                int grade = GetGrade(userName);
                int studentId = LoginController.GetUserID(userName, "student");

                //Get list of all classes in the student's school and grade
                tempList = (from c in db.ClassGroups
                                            where c.SchoolId == schoolId && c.Grade == grade
                                            join tc in db.TeachersToClasses on c.Id equals tc.ClassId
                                            join t in db.Teachers on tc.TeacherId equals t.Id
                                            select new ClassGroupDTO
                                            {
                                                Id = c.Id,
                                                Category = c.Category,
                                                Teacher = t.Name
                                            }).ToList();

                classesToJoin = tempList.ToList();

                //Get list of all classes that the student is registered for
                var query2 = (from sc in db.StudentsToClasses
                              where sc.StudentId == studentId
                              select sc.ClassId).ToList();

                int i = 0; //index deference when items deleted from the main list
                foreach (var item in tempList)
                {
                    foreach (var item2 in query2)
                    {
                        if (item.Id == item2)
                        {
                            classesToJoin.RemoveAt(tempList.IndexOf(item) - i);
                            i++;
                            break;
                        }
                    };
                };

                return classesToJoin;
            }
            catch (Exception ex)
            {
                throw ex;
            };
        }

        [HttpPost]
        [Route("api/Study/JoinClass")]
        public bool JoinClass([FromBody]UserDetails data)
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

            int studentId = LoginController.GetUserID(data.userName, "student");

            try
            {
                db.StudentsToClasses.Add(new StudentToClasses
                {
                    ClassId = data.classId,
                    StudentId = studentId
                });
                db.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
               throw ex;
            };
        }

        [HttpGet]
        [Route("api/Study/LoadClassPage/{CourseId}")]
        public ClassGroupDTO LoadClassPage(int courseId)
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

            try
            {
                ClassGroupDTO classGroup = (from c in db.ClassGroups
                                            where c.Id == courseId
                                            select new ClassGroupDTO 
                                            {
                                                Category = c.Category,
                                                Grade = c.Grade
                                            }).First();

                classGroup.Teacher = (from tc in db.TeachersToClasses
                                      where tc.ClassId == courseId
                                      join t in db.Teachers on tc.TeacherId equals t.Id
                                      select t.Name).First();

                classGroup.numOfStudents = (from sc in db.StudentsToClasses
                                            where sc.ClassId == courseId
                                            select sc).Count();

                //Message query should be build when all the messaging functionality will be developed
                classGroup.message = "You should pay attention to lesson number 3. It is very important!";

                classGroup.lessons = new List<LessonDTO>();

                classGroup.lessons = (from lc in db.LessonsToClasses
                                      where lc.ClassId == courseId
                                      join l in db.Lessons on lc.LessonId equals l.Id
                                      orderby l.SeqNum
                                      select new LessonDTO
                                      {
                                          Id = l.Id,
                                          Name = l.Name,
                                          IsActive = lc.IsActive,
                                          Description = l.Description
                                      }).ToList();

                return classGroup;
            }
            catch (Exception ex)
            {
                throw ex;
            };
        }

        /*===================================================================================
          Internal functions
         ==================================================================================*/
        internal static int lessonsCompleted(int studentId)
        {
            var db = new DBModel();
            return (from rl in db.ResultInLessons
                    where rl.StudentId == studentId && rl.Result >= MIN_RES_TO_PASS
                    group rl by rl.LessonId into lessonRes
                    select lessonRes).Count();
        }

        internal static int qCorrAnswered(int studentId)
        {
            var db = new DBModel();
            return (from rq in db.ResultInQuestions
                    where rq.StudentId == studentId && rq.Result
                    select rq).Count();
        }

        internal static int GetSchoolId(string userName)
        {
            var db = new DBModel();
            return (from st in db.Students
                    where st.UserName == userName
                    select st.SchoolId).First();
        }

        internal static int GetGrade(string userName)
        {
            var db = new DBModel();
            return (from st in db.Students
                    where st.UserName == userName
                    select st.Grade).First();
        }
    }
}
