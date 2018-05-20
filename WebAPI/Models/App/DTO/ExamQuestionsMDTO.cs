using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebAPI.Models.App
{
    public class ExamQuestionsMDTO
    {
        public int Id { get; set; }
        public string Category { get; set; }
        public int Grade { get; set; }
        public string Question { get; set; }
        public AnswerVar AnswerA { get; set; }
        public AnswerVar AnswerB { get; set; }
        public AnswerVar AnswerC { get; set; }
        public AnswerVar AnswerD { get; set; }
    }

    public class AnswerVar
    {
        public string Answer { get; set; }
        public bool IsRight { get; set; }

        public AnswerVar (string answer, bool isRight)
        {
            Answer = answer;
            IsRight = isRight;
        }
    }
}