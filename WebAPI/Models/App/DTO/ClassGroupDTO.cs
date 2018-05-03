using System.Collections.Generic;
using WebAPI.Models.App;

namespace WebAPI.Models
{
    public class ClassGroupDTO
    {
        public int Id { get; set; }
        public string Category { get; set; }
        public int Grade { get; set; }
        public string Teacher { get; set; }
        public int numOfStudents { get; set; }
        public MessageDTO message { get; set; }
        public List<LessonDTO> lessons { get; set; }
        public List<int> lessonIDs { get; set; }
        public string TeacherUserName { get; set; }
        public bool ExamExists { get; set; }
    }
}