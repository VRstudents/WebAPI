using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebAPI.Models.App
{
    public class UserDetails
    {
        public string token { get; set; }
        public string userName { get; set; }
        public string name { get; set; }
        public int classId { get; set; }
        public string picData { get; set; }
    }
}