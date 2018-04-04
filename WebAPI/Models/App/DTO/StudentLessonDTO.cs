using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApplication1.Models
{
    public class StudentLessonDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int SeqNum { get; set; }
        public bool IsActive { get; set; }
        public int FinishedLessonNum { get; set; }
    }
}