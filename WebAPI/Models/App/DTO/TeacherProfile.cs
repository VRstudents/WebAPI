using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebAPI.Models
{
    public class TeacherProfile
    {
        public List<string> categories { get; set; }
        public List<SCourses> classes { get; set; }
        public string Picture { get; set; }
    }
}