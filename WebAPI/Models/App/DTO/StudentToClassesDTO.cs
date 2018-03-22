﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApplication1.Models.App
{
    public class StudentToClassesDTO
    {
        public string Category { get; set; }
        public int Grade { get; set; }
        public List<String> Students { get; set; }

        public StudentToClassesDTO(string category, int grade)
        {
            this.Category = category;
            this.Grade = grade;
            Students = new List<String>();
        }
    }
}