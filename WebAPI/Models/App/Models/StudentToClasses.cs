using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApplication1.Models.App
{
    public class StudentToClasses
    {
        public int Id { get; set; }
        public int ClassId { get; set; }
        public int StudentId { get; set; }
    }
}