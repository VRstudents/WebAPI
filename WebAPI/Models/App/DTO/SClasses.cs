﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebAPI.Models
{
    public class SClasses
    {
        public int CourseId { get; set; }
        public int LessonId { get; set; }
        public string Category { get; set; }
        public int LessonNum { get; set; }
        public string Description { get; set; }
        public int Attempts { get; set; }
        public double BestRes { get; set; }
    }
}