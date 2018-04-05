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
    public class TempController : ApiController
    {
        [Route("api/Temp/GetStudentName/{Id}")]
        public string GetStudentName(int id)
        {
            var db = new DBModel();

            var query = from student in db.Students
                        where student.Id == id
                        select student.Name;

            return query.First().ToString();
        }

        [HttpPost]
        [Route("api/Temp/TempUpdateStudentGrade")]
        public bool TempUpdateStudentGrade([FromBody]IdGrade data)
        {
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
