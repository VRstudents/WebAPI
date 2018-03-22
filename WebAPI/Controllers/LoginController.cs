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
        //[HttpPost]
        //[Route("api/Login/CheckUser/{Token}/{UserName}/{Name}")]
        [HttpGet]
        [Route("api/Login/CheckUser/{Token}/{UserName}/{Name}")]
        //public bool CheckUser([FromBody]string token, [FromBody]string userName, [FromBody]string name)
        public bool CheckUser(string token, string userName, string name)
        {
            var db = new DBModel();

            var query = (from user in db.Users
                        where user.UserName == userName
                        select user);

            if (query.Any())
            {
                query.First().Token = token;
                if (!query.First().FinishedSignUP)
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
    }
}