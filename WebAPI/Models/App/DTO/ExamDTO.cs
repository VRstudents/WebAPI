using System.Collections.Generic;

namespace WebAPI.Models.App
{
    public class ExamDTO
    {
        public int ClassId { get; set; }
        public string Category { get; set; }
        public List<int> QuestionIDs { get; set; }
        public List<ExamQuestionsMDTO> Questions { get; set; }
    }
}