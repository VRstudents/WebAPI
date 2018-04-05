using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebAPI.Models.App
{
    public class LessonsToClass
    {
        public int Id { get; set; }
        public int ClassId { get; set; }
        public int LessonId { get; set; }
        public bool IsActive { get; set; }
    }
}