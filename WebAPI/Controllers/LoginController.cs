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

        /*------------------------------------------------------------------------------------------------------------------------
        Method for adding a new user to the system.
        If the user already exists new token is updated.
        Method returns true if the user exists AND filled the sign up form.
        Method returns true if the user doesn't exist OR hasn't filled the sign up form.
        ------------------------------------------------------------------------------------------------------------------------*/
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
                db.SaveChanges();
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

        /*------------------------------------------------------------------------------------------------------------------------
        Method for checking if the user is authenticated to control access to restricted web pages.
        ------------------------------------------------------------------------------------------------------------------------*/
        [HttpGet]
        [Route("api/Login/CheckAuth/{UserName}/{Token}")]
        public bool CheckAuth(string userName, string token)
        {
            var db = new DBModel();

            var query = (from u in db.Users
                        where u.UserName == userName && u.Token == token
                        select u).Any();

            if (query)
            {
                return true;
            }

            else
            {
                return false;
            }
        }

        /*------------------------------------------------------------------------------------------------------------------------
        Method for checking if the user has filled the signup form to prevent fully registred user from accessing the form again.
        ------------------------------------------------------------------------------------------------------------------------*/
        [HttpGet]
        [Route("api/Login/CheckIfAlreadyRegistered/{UserName}")]
        public bool CheckIfAlreadyRegistered(string userName)
        {
            //bool validation = checkOnAccess(this.Request.Headers);
            //if (!validation)
            //{
            //    return false;
            //}

            var db = new DBModel();

            var query = from u in db.Users
                        where u.UserName == userName
                        select u.FinishedSignUP;

           if (query.First() == 1)
           {
                return true;
           }

           else
           {
                return false;
           }
        }

        /*------------------------------------------------------------------------------------------------------------------------
        !---Method not in use---!
        Method to retrieve user ID based on hos token.
        ------------------------------------------------------------------------------------------------------------------------*/
        [HttpGet]
        [Route("api/Login/GetUserID/{Token}")]
        public int GetUserID(string token)
        {
            //bool validation = checkOnAccess(this.Request.Headers);
            //if (!validation)
            //{
            //    return -9999;
            //}

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

        /*------------------------------------------------------------------------------------------------------------------------
        Method to register user as a student or a teacher.
        ------------------------------------------------------------------------------------------------------------------------*/
        //[HttpPost]
        [HttpGet]
        //[Route("api/Login/AddNewStudentTeacher/{UserName}/{Role}/{SchoolId}/{Grade}")]
        [Route("api/Login/AddNewStudentTeacher/{UserName}/{Role}/{SchoolId}/{Grade}")]
        //public bool AddNewStudentTeacher(string userName, string role, int schoolId, int grade)
        public bool AddNewStudentTeacher(string userName, string role, int schoolId, int grade)
        {

            //bool validation = checkOnAccess(this.Request.Headers);
            //if (!validation)
            //{
            //    return false;
            //}

            var db = new DBModel();

            var query = (from u in db.Users
                         where u.UserName == userName
                         select u).First();

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

        /*------------------------------------------------------------------------------------------------------------------------
        Method to delete user token on signout.
        ------------------------------------------------------------------------------------------------------------------------*/
        [HttpGet]
        [Route("api/Login/SignOut/{UserName}")]
        public void SignOut(string userName)
        {
            //bool validation = checkOnAccess(this.Request.Headers);
            //if (!validation)
            //{
            //    return;
            //}

            var db = new DBModel();

            try
            {
                var query = from u in db.Users
                            where u.UserName == userName
                            select u;

                query.First().Token = "";
                db.SaveChanges();
                return;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /*------------------------------------------------------------------------------------------------------------------------
        Private method for checking if the user is authenticated to control access to WebAPI routes.
        ------------------------------------------------------------------------------------------------------------------------*/
        bool checkOnAccess(System.Net.Http.Headers.HttpRequestHeaders header)
        {
            if (header.Contains("Token"))
            {
                var token = header.GetValues("Token").First().ToString();
                var db = new DBModel();

                var query = (from u in db.Users
                             where u.Token == token
                             select u).Any();

                if (query)
                {
                    return true;
                }

                else
                {
                    return false;
                }
            }
            else 
            {
                return false;
            }
        }
    }
}