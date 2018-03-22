using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using WebApplication1.Models;
using WebApplication1.Models.App;

namespace WebApplication1.Controllers
{
    public class StudyController : ApiController
    {
        [Route("api/Study/GetClassGroups")]
        public List<ClassGroupDTO> GetGroups()
        {
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
        public List<StudentDTO> GetStudents()
        {
            var db = new DBModel();

            var query = from student in db.Students
                        orderby student.Name
                        select new StudentDTO {
                            Name = student.Name,
                            Grade = student.Grade,
                            SchoolId = student.SchoolId
                        };

            return query.ToList();
        }

        //[HttpPost]
        //[Route("api/Study/GetStudentsNamesByClassGroup")]
        [Route("api/Study/GetStudentsNamesByClassGroup/{classGroupId}")]
        //public StudentToClassesDTO GetStudentsNamesByClassGroup([FromBody]int classGroupId)
        public StudentToClassesDTO GetStudentsNamesByClassGroup(int classGroupId)
        {
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
                    return new StudentToClassesDTO("No such class group", 0);
                }

                else
                    return new StudentToClassesDTO("An error accured", 0);
            }
        }

        //[HttpPost]
        [HttpGet]
        //[Route("api/Study/UpdateStudentGrade/{studentId}")]
        [Route("api/Study/UpdateStudentGrade/{studentId}/{grade}")]
        //public bool UpdateStudentGrade(int studentId, [FromBody]int grade)
        public bool UpdateStudentGrade(int studentId, int grade)
        {
            var db = new DBModel();

            try
            {
                var query = (from s in db.Students
                            where s.Id == studentId
                            select s).First();

                query.Grade = grade;

                var query2 = (from sc in db.StudentsToClasses
                        where sc.StudentId == studentId
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
            }
        }

        //[HttpPost]
        [HttpGet]
        //[Route("api/Study/AddNewStudent/{name}/{schoolId}/{grade}")]
        [Route("api/Study/AddNewStudent/{userName}/{name}/{schoolId}/{grade}")]
        //public bool AddNewStudent(string name, int schoolId, int grade, [FromBody]string userName)
        public bool AddNewStudent(string userName, string name, int schoolId, int grade)
        {
            var db = new DBModel();

            var query = from s in db.Students
                        where s.UserName == userName
                        select s;

            if (query.Any())
            {
                return false;
            }

            Student student = new Student()
            {
                UserName = userName,
                Name = name,
                SchoolId = schoolId,
                Grade = grade
            };

            try
            {
                db.Students.Add(student);
                db.SaveChanges();
                return true;
            }

            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
