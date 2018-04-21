using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebAPI.Models.App.JSONFormat
{
    public class StudentTeacher
    {
        public string userName { get; set; }
        public string role { get; set; }
        public int schoolId { get; set; }
        public int grade { get; set; }
        public List<string> categories { get; set; }
    }
}