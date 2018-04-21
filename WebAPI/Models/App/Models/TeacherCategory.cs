using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebAPI.Models.App
{
    public class TeacherCategory
    {
        public int Id { get; set; }
        public int TeacherId { get; set; }
        public string Category { get; set; }
    }
}