using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebAPI.Models.App
{
    public class Info
    {
        public int lessonsCompleted { get; set; }
        public int qCorrAnswered { get; set; }
        public List<StudentLessonDTO> best { get; set; }
        public List<StudentLessonDTO> worst { get; set; }
    }
}