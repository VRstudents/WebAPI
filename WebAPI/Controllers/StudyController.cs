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
        [Route("api/Study/GetClassGroups")]
        public List<ClassGroupDTO> GetGroups()
        {
            try
            {
                bool validation = LoginController.checkOnAccess(this.Request.Headers);
                if (!validation)
                {
                    throw new Exception("Access denied.");
                }
            }
            catch (Exception ex)
            {
                throw ex;
            };

            var db = new DBModel();

            var query = from classGroup in db.ClassGroups
                        orderby classGroup.Grade, classGroup.Category
                        select new ClassGroupDTO
                        {
                            Category = classGroup.Category,
                            Grade = classGroup.Grade
                        };

            return query.ToList();
        }

        [Route("api/Study/GetStudents")]
        public List<PersonDTO> GetStudents()
        {
            try
            {
                bool validation = LoginController.checkOnAccess(this.Request.Headers);
                if (!validation)
                {
                    throw new Exception("Access denied.");
                }
            }
            catch (Exception ex)
            {
                throw ex;
            };

            var db = new DBModel();

            var query = from student in db.Students
                        orderby student.Name
                        select new PersonDTO {
                            Name = student.Name,
                            Grade = student.Grade//,
                            //SchoolId = student.SchoolId !!---In case of using, need to change school ID to school name
                        };

            return query.ToList();
        }

        [Route("api/Study/GetStudentsNamesByClassGroup/{ClassGroupId}")]
        public StudentToClassesDTO GetStudentsNamesByClassGroup(int classGroupId)
        {
            try
            {
                bool validation = LoginController.checkOnAccess(this.Request.Headers);
                if (!validation)
                {
                    throw new Exception("Access denied.");
                }
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

        [HttpPost]
        [Route("api/Study/UpdateStudentGrade")]
        public bool UpdateStudentGrade([FromBody]IdGrade data)
        {
            try
            {
                bool validation = LoginController.checkOnAccess(this.Request.Headers);
                if (!validation)
                {
                    throw new Exception("Access denied.");
                }
            }
            catch (Exception ex)
            {
                throw ex;
            };

            var db = new DBModel();

            int id = data.id;
            int grade = data.grade;

            try
            {
                var query = (from s in db.Students
                             where s.Id == id
                             select s).First();

                query.Grade = grade;

                var query2 = (from sc in db.StudentsToClasses
                              where sc.StudentId == id
                              select sc);

                foreach (var sc in query2)
                {
                    db.StudentsToClasses.Remove(sc);
                }

                db.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            };
        }
    }
}
