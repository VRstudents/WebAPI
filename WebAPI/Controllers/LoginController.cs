using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Web.Http;
using System.Web.Http.Cors;
using WebAPI.Models.App.JSONFormat;
using WebApplication1.Models.App;

namespace WebApplication1.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class LoginController : ApiController
    {
        const string ROLE_STUDENT = "student";

        /*------------------------------------------------------------------------------------------------------------------------
        Method for adding a new user to the system.
        If the user already exists new token is updated.
        Method returns true if the user exists AND filled the sign up form.
        Method returns true if the user doesn't exist OR hasn't filled the sign up form.
        ------------------------------------------------------------------------------------------------------------------------*/
        [HttpPost]
        [Route("api/Login/AddUser")]
        public bool AddUser([FromBody]UserDetails data)
        {
            string token = data.token;
            string userName = data.userName;
            string name = data.name;

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
        [Route("api/Login/CheckAuth/{UserName}")]
        public bool CheckAuth(string userName)
        {
            var token = this.Request.Headers.GetValues("Token").First().ToString();
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
            try
            {
                bool validation = checkOnAccess(this.Request.Headers);
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

            var query = from u in db.Users
                        where u.UserName == userName
                        select u.FinishedSignUP;


            if (!query.Any())
            {
                throw new Exception("User does not exist in the system.");
            }

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
        Method to retrieve schools list for registration form.
        ------------------------------------------------------------------------------------------------------------------------*/
        [HttpGet]
        [Route("api/Login/GetSchools")]
        public List<SchoolDTO> GetSchools()
        {
            try
            {
                try
                {
                    bool validation = checkOnAccess(this.Request.Headers);
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

                var query = from s in db.Schools
                            select new SchoolDTO
                            {
                                Id = s.Id,
                                Name = s.Name
                            };

                return query.ToList();
            }
            catch (Exception ex)
            {
                throw ex;
            };
        }

        /*------------------------------------------------------------------------------------------------------------------------
        !---Method not in use---!
        Method to retrieve user ID based on hos token.
        ------------------------------------------------------------------------------------------------------------------------*/
        [HttpGet]
        [Route("api/Login/GetUserID")]
        public int GetUserID()
        {
            try
            {
                var token = this.Request.Headers.GetValues("Token").First().ToString();
                var db = new DBModel();

                var query = from u in db.Users
                            where u.Token == token
                            select u.Id;

                return query.First();
            }
            catch (Exception ex)
            {
                throw ex;
            };
        }

        /*------------------------------------------------------------------------------------------------------------------------
        Method to register user as a student or a teacher.
        ------------------------------------------------------------------------------------------------------------------------*/
        [HttpPost]
        [Route("api/Login/AddNewStudentTeacher")]
        public bool AddNewStudentTeacher([FromBody]StudentTeacher data)
        {
            try
            { 
                bool validation = checkOnAccess(this.Request.Headers);
                if (!validation)
                {
                    throw new Exception("Access denied.");
                }
            }
            catch (Exception ex)
            {
                throw ex;
            };

            string userName = data.userName;
            string role = data.role;
            int schoolId = data.schoolId;
            int grade = data.grade;

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
                };
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
            };
        }

        /*------------------------------------------------------------------------------------------------------------------------
        Method to delete user token on signout.
        ------------------------------------------------------------------------------------------------------------------------*/
        [HttpPost]
        [Route("api/Login/SignOut")]
        public void SignOut([FromBody]string userName)
        {
            try
            {
                bool validation = checkOnAccess(this.Request.Headers);
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
            };
        }

        /*------------------------------------------------------------------------------------------------------------------------
        Method for checking if the user is authenticated to control access to WebAPI routes.
        ------------------------------------------------------------------------------------------------------------------------*/
        internal static bool checkOnAccess(HttpRequestHeaders header)
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
                throw new Exception("No token present in request header.");
            }
        }
    }
}