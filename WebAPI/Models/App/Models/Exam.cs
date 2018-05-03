using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebAPI.Models
{
    public class Exam
    {
        public int Id { get; set; }
        public int ClassId { get; set; }
        public int QuestionId { get; set; }
    }
}