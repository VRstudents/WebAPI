using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebAPI.Models
{
    public class SCourses
    {
        public int CourseId { get; set; }
        public string Category { get; set; }
        public int Grade { get; set; }
        public double Result { get; set; }
    }
}