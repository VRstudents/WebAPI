using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebAPI.Models.App
{
    public class ExamQuestionsDTO
    {
        public int Id { get; set; }
        public string Question { get; set; }
        public string AnswerA { get; set; }
        public string AnswerB { get; set; }
        public string AnswerC { get; set; }
        public string AnswerD { get; set; }
        public int RightAnswer { get; set; }
    }
}