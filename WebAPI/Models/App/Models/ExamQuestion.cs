﻿namespace WebAPI.Models.App
{
    public class ExamQuestion
    {
        public int Id { get; set; }
        public string Category { get; set; }
        public int Grade { get; set; }
        public string Question { get; set; }
        public string AnswerA { get; set; }
        public string AnswerB { get; set; }
        public string AnswerC { get; set; }
        public string AnswerD { get; set; }
        public int RightAnswer { get; set; }
    }
}