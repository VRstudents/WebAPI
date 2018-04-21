using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebAPI.Models
{
    public class ClassGroupDTO
    {
        public int Id { get; set; }
        public string Category { get; set; }
        public int Grade { get; set; }
        public string Teacher { get; set; }
        public int numOfStudents { get; set; }
        public string message { get; set; }
        public List<LessonDTO> lessons { get; set; }
    }
}