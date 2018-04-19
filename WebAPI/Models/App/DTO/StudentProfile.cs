using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebAPI.Models
{
    public class StudentProfile
    {
        public List<SCourses> sCourses { get; set; }
        public List<SClasses> sClasses { get; set; }
        public int lessonsCompleted { get; set; }
        public int qCorrAnswered { get; set; }
        public double avgCourseRes { get; set; }
        public double bestCourseRes { get; set; }
    }
}