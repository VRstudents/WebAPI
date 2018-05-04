using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebAPI.Models.App
{
    public class ExamDTO
    {
        public int ClassId { get; set; }
        public string Category { get; set; }
        public List<int> QuestionIDs { get; set; }
        public List<ExamQuestionsDTO> Questions { get; set; }
}
}