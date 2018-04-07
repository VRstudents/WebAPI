using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebAPI.Models.App
{
    public class ProgressInClass
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public int ClassId { get; set; }
        public double Result { get; set; }
        public int FinishedLessonNum { get; set; }
    }
}