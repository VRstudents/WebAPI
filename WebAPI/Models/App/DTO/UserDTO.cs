using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApplication1.Models.App
{
    public class UserDTO
    {
        public string Token { get; set; }
        public string UserName { get; set; }
        public string Picture { get; set; }
        public int SchoolId { get; set; }
    }
}