using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Web.Http;
using System.Web.Http.Cors;
using WebAPI.Models.App;
using WebAPI.Exceptions;
using System.IO;
using System.Configuration;

namespace WebAPI.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class LoginController : ApiController
    {
        const int CODE_LENGHT = 6;

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
                query.First().Code = RandomString();
                db.SaveChanges();
                if (query.First().FinishedSignUP == 0)
                {
                    return false;
                }
                return true;
            }

            else
            {
                string code = RandomString();
                User user = new User()
                {
                    Token = token,
                    UserName = userName,
                    Name = name,
                    Code = code
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
                checkOnAccess(this.Request.Headers);
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
                    checkOnAccess(this.Request.Headers);
                }
                catch (Exception ex)
                {
                    throw ex;
                };

                var db = new DBModel();

                var query = from s in db.Schools
                            orderby s.Name
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
        Method to retrieve user's role. school and grade(in case of a student) based on username.
        ------------------------------------------------------------------------------------------------------------------------*/
        [HttpGet]
        [Route("api/Login/GetProfileDetails/{UserName}")]
        public PersonDTO GetProfileDetails(string userName)
        {
            try
            {
                checkOnAccess(this.Request.Headers);
            }
            catch (Exception ex)
            {
                throw ex;
            };

            try
            {
                var db = new DBModel();

                var query = (from u in db.Users
                             where u.UserName == userName
                             select u).FirstOrDefault();

                if (query.Role == "student")
                {
                    var query2 = (from st in db.Students
                                  where st.UserName == userName
                                  select st).FirstOrDefault();

                    var query3 = (from sc in db.Schools
                                  where sc.Id == query2.SchoolId
                                  select sc.Name).First();

                    return new PersonDTO()
                    {
                        Grade = query2.Grade,
                        Role = "student",
                        Code = query.Code,
                        SchoolName = query3,
                        Picture = query.Picture
                    };
                }

                 else
                {
                    var query2 = from t in db.Teachers
                                 where t.UserName == userName
                                 select t;

                    var query3 = (from sc in db.Schools
                                  where sc.Id == query2.FirstOrDefault().SchoolId
                                  select sc.Name).FirstOrDefault();

                    return new PersonDTO()
                    {
                        Role = "teacher",
                        Code = query.Code,
                        SchoolName = query3,
                        Picture = query.Picture
                    };
                }
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
                checkOnAccess(this.Request.Headers);
            }
            catch (Exception ex)
            {
                throw ex;
            };

            var db = new DBModel();

            var query = (from u in db.Users
                         where u.UserName == data.userName
                         select u).First();

            if (data.role == "student")
            {
                Student student = new Student()
                {
                    UserName = data.userName,
                    Name = query.Name,
                    SchoolId = data.schoolId,
                    Grade = data.grade
                };

                try
                {
                    query.Role = "student";
                    db.Students.Add(student);
                    query.FinishedSignUP = 1;
                    db.SaveChanges();
                    return true;
                }
                catch (Exception ex)
                {
                    throw ex;
                };
            }

            else
            {
                Teacher teacher = new Teacher()
                {
                    UserName = data.userName,
                    Name = query.Name,
                    SchoolId = data.schoolId,
                };

                try
                {
                    query.Role = "teacher";
                    db.Teachers.Add(teacher);
                    db.SaveChanges();

                    int teacherId = GetUserID(data.userName, "teacher");

                    foreach (var item in data.categories)
                    {
                        db.TeacherCategories.Add(new TeacherCategory()
                        {
                            TeacherId = teacherId,
                            Category = item
                        });
                    };

                    query.FinishedSignUP = 1;
                    db.SaveChanges();
                    return true;
                }
                catch (Exception ex)
                {
                    throw ex;
                };
            };
        }

        /*------------------------------------------------------------------------------------------------------------------------
        Method to save newly created avatar
        ------------------------------------------------------------------------------------------------------------------------*/
        [HttpPost]
        [Route("api/Login/SaveAvatar")]
        public bool SaveAvatar([FromBody]UserDetails data)
        {
            try
            {
                checkOnAccess(this.Request.Headers);
            }
            catch (Exception ex)
            {
                throw ex;
            };

            var db = new DBModel();

            try
            {
                var query = from u in db.Users
                            where u.UserName == data.userName
                            select u;

                int userId = query.First().Id;

                String path = ConfigurationManager.AppSettings["fs_path"].ToString();
                string imageName = userId + ".png";
                string imgPath = Path.Combine(path, imageName);
                byte[] imageBytes = Convert.FromBase64String(data.picData);
                File.WriteAllBytes(imgPath, imageBytes);

                query.First().Picture = ConfigurationManager.AppSettings["web_path"].ToString() + userId + ".png";
                db.SaveChanges();

                return true;
            }
            catch (Exception ex)
            {
                throw ex;
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
                checkOnAccess(this.Request.Headers);
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

        /*========================================================================================================================
        Internal functions
        ========================================================================================================================*/
        /*------------------------------------------------------------------------------------------------------------------------
        Method to retrieve student/teacher ID based on username.
        ------------------------------------------------------------------------------------------------------------------------*/
        internal static int GetUserID(string userName, string role)
        {
            try
            {
                var db = new DBModel();

                if (role == "student")
                {
                    var query = from s in db.Students
                                where s.UserName == userName
                                select s.Id;

                    return query.First();
                }

                else
                {
                    var query = from t in db.Teachers
                                where t.UserName == userName
                                select t.Id;

                    return query.First();
                };
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
                string token = header.GetValues("Token").First().ToString();
                var db = new DBModel();

                var query = from u in db.Users
                            where u.Token == token
                            select u;

                if (query.Any())
                {
                    return true;
                }

                else
                {
                    throw new AccessDeniedExc();
                };
            }
            else 
            {
                throw new AccessDeniedExc();
            };
        }

        /*------------------------------------------------------------------------------------------------------------------------
        Method for generating the code for mobile login. New unique code is generated on signing up and on each login to web site.
        ------------------------------------------------------------------------------------------------------------------------*/
        private static string RandomString()
        {
            var chars = "0123456789";
            var stringChars = new char[CODE_LENGHT];
            var random = new Random();
            var db = new DBModel();

            var query = from u in db.Users
                        select u.Code;

            while(true)
            {
                for (int i = 0; i < stringChars.Length; i++)
                {
                    stringChars[i] = chars[random.Next(chars.Length)];
                };

                var finalString = new String(stringChars);

                if (!query.Any(c => c == finalString))
                {
                    return finalString;
                };
            };
        }
    }
}