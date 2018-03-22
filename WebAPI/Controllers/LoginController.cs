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
    public class LoginController : ApiController
    {
        const string ROLE_STUDENT = "student";

        //[HttpPost]
        //[Route("api/Login/AddUser/{Token}/{UserName}/{Name}")]
        [HttpGet]
        [Route("api/Login/AddUser/{Token}/{UserName}/{Name}")]
        //public bool AddUser([FromBody]string token, [FromBody]string userName, [FromBody]string name)
        public bool AddUser(string token, string userName, string name)
        {
            var db = new DBModel();

            var query = (from user in db.Users
                        where user.UserName == userName
                        select user);

            if (query.Any())
            {
                query.First().Token = token;
                if (query.First().FinishedSignUP == 0)
                {
                    return false;
                }
                return true;
            }

            else
            {
                User user = new User()
                {
                    Token = token,
                    UserName = userName,
                    Name = name
                };

                db.Users.Add(user);
                db.SaveChanges();
                return false;
            }
        }

        [HttpGet]
        [Route("api/Login/GetUserID/{Token}")]
        public int GetUserID(string token)
        {
            var db = new DBModel();

            var query = from u in db.Users
                        where u.Token == token
                        select u.Id;

            try
            {
                return query.First();
            }
            catch (Exception ex)
            {
                return -9999;
            }
        }

        //[HttpPost]
        [HttpGet]
        //[Route("api/Login/AddNewStudent/{UserName}/{Role}/{SchoolId}/{Grade}")]
        [Route("api/Login/AddNewStudent/{UserName}/{Role}/{SchoolId}/{Grade}")]
        //public bool AddNewStudent(string userName, string role, int schoolId, int grade)
        public bool AddNewStudent(string userName, string role, int schoolId, int grade)
        {
            var db = new DBModel();

            var query = (from u in db.Users
                         where u.UserName == userName
                         select u).First(); ;

            if (role == ROLE_STUDENT)
            {
                Student student = new Student()
                {
                    UserName = userName,
                    Name = query.Name,
                    SchoolId = schoolId,
                    Grade = grade
                };

                try
                {
                    db.Students.Add(student);
                    query.FinishedSignUP = 1;
                    db.SaveChanges();
                    return true;
                }
                catch (Exception ex)
                {
                    return false;
                }
            }

            Teacher teacher = new Teacher()
            {
                UserName = userName,
                Name = query.Name,
                SchoolId = schoolId,
            };

            try
            {
                db.Teachers.Add(teacher);
                query.FinishedSignUP = 1;
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