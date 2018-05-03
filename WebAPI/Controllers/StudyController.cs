using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Cors;
using WebAPI.Models.App.JSONFormat;
using WebAPI.Models;
using WebAPI.Models.App;

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
                int schoolId = GetSchoolId(userName, "student");                
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
        [Route("api/Study/LoadClassPage/{CourseId}/{UserName}")]
        public ClassGroupDTO LoadClassPage(int courseId, string userName)
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

                classGroup.message = (from m in db.Messages
                                      where m.ClassId == courseId
                                      select new MessageDTO {
                                          Text = m.Text,
                                          Date = m.Date
                                      }).FirstOrDefault();

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
                                          Description = l.Description,
                                          SeqNum = l.SeqNum
                                      }).ToList();

                classGroup.ExamExists = (from e in db.Exams
                                         where e.ClassId == courseId
                                         select e).Any();

                string role = (from u in db.Users
                               where u.UserName == userName
                               select u.Role).FirstOrDefault();

                if (role == "student")
                {
                    int studentId = LoginController.GetUserID(userName, "student");

                    foreach (var lesson in classGroup.lessons)
                    {
                        lesson.Result = (from rl in db.ResultInLessons
                                         where rl.StudentId == studentId && rl.LessonId == lesson.Id
                                         group rl by rl.Id into lessonRes
                                         select lessonRes.Max(x => x.Result)).FirstOrDefault();

                        if (lesson.Result >= MIN_RES_TO_PASS)
                        {
                            lesson.IsPassed = true;
                        }

                        else
                        {
                            lesson.IsPassed = false;
                        };
                    };
                };

                return classGroup;
            }
            catch (Exception ex)
            {
                throw ex;
            };
        }

        [HttpPost]
        [Route("api/Study/AddLessons")]
        public bool AddLessons(ClassGroupDTO data)
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
                foreach (var id in data.lessonIDs)
                {
                    var query = from lc in db.LessonsToClasses
                                where lc.ClassId == data.Id && lc.LessonId == id
                                select lc;

                    query.First().IsActive = true;
                };

                db.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            };
        }

        [HttpGet]
        [Route("api/Study/LoadCreateClassPage/{UserName}")]
        public List<string> LoadCreateClassPage(string userName)
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
                return (from t in db.Teachers
                        where t.UserName == userName
                        join tc in db.TeacherCategories on t.Id equals tc.TeacherId
                        select tc.Category).ToList();
            }
            catch (Exception ex)
            {
                throw ex;
            };
        }

        [HttpGet]
        [Route("api/Study/LoadAddLessonsPage/{Category}/{Grade}")]
        public ClassGroupDTO LoadAddLessonsPage(string category, int grade)
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
                ClassGroupDTO classGroup = new ClassGroupDTO();
                classGroup.lessons = new List<LessonDTO>();

                classGroup.lessons = (from l in db.Lessons
                                      where l.Category == category && l.Grade == grade
                                      orderby l.SeqNum
                                      select new LessonDTO
                                      {
                                          Id = l.Id,
                                          Name = l.Name,
                                          Description = l.Description,
                                          SeqNum = l.SeqNum
                                      }).ToList();

                return classGroup;
            }
            catch (Exception ex)
            {
                throw ex;
            };
        }

        [HttpPost]
        [Route("api/Study/CreateClass")]
        public bool CreateClass(ClassGroupDTO data)
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
                int schoolId = GetSchoolId(data.TeacherUserName, "teacher");

                //Create new ClassGroup record
                db.ClassGroups.Add(new ClassGroup
                {
                    Category = data.Category,
                    Grade = data.Grade,
                    SchoolId = schoolId
                });

                db.SaveChanges();

                int classId = (from c in db.ClassGroups
                                where c.Category == data.Category && c.Grade == data.Grade && c.SchoolId == schoolId
                                orderby c.Id descending
                                select c.Id).First();

                //Assign the teacher to the class by creating new Teacher to Class record
                int teacherId = LoginController.GetUserID(data.TeacherUserName, "teacher");
                db.TeachersToClasses.Add(new TeacherToClasses
                {
                    ClassId = classId,
                    TeacherId = teacherId
                });

                //Get the list of all lessons of the category for the grade
                var query = from l in db.Lessons
                            where l.Category == data.Category && l.Grade == data.Grade
                            select l.Id;

                //Add all the lessons of the category for the grade to the new class group
                foreach (var lessonId in query)
                {
                    bool isActive;

                    if (data.lessonIDs.Exists(x => x == lessonId))
                    {
                        isActive = true;
                    }

                    else
                    {
                        isActive = false;
                    };

                    db.LessonsToClasses.Add(new LessonsToClass
                    {
                        ClassId = classId,
                        LessonId = lessonId,
                        IsActive = isActive
                    });
                };

                db.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            };
        }

        [HttpPost]
        [Route("api/Study/PostMessage")]
        public bool PostMessage(Message data)
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
                var query = from m in db.Messages
                            where m.ClassId == data.ClassId
                            select m;

                if (query.Any())
                {
                    db.Messages.Remove(query.FirstOrDefault());
                };

                data.Date = DateTime.Now;
                db.Messages.Add(data);
                db.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            };
        }

        [HttpGet]
        [Route("api/Study/GetExamQuestions/{ClassId}")]
        public List<ExamQuestionsDTO> GetExamQuestions(int classId)
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
                var myClass = (from c in db.ClassGroups
                               where c.Id == classId
                               select new {
                                   c.Grade,
                                   c.Category
                               }).First();

                var query = from q in db.ExamQuestions
                            where q.Category == myClass.Category && q.Grade == myClass.Grade
                            select new ExamQuestionsDTO
                            {
                                Id = q.Id,
                                Question = q.Question,
                                AnswerA = q.AnswerA,
                                AnswerB = q.AnswerB,
                                AnswerC = q.AnswerC,
                                AnswerD = q.AnswerD,
                                RightAnswer = q.RightAnswer
                            };

                return query.ToList();
            }
            catch (Exception ex)
            {
                throw ex;
            };
        }

        /*===================================================================================
          Internal functions
        ===================================================================================*/
        internal static int GetSchoolId(string userName, string role)
        {
            var db = new DBModel();
            if (role == "student")
            {
                return (from st in db.Students
                        where st.UserName == userName
                        select st.SchoolId).First();
            }

            else
            {
                return (from t in db.Teachers
                        where t.UserName == userName
                        select t.SchoolId).First();
            };
            
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
