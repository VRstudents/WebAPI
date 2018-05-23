using System.Collections.Generic;

namespace WebAPI.Models.App
{
    public class StudentTeacher
    {
        public string userName { get; set; }
        public string role { get; set; }
        public int schoolId { get; set; }
        public int grade { get; set; }
        public List<string> categories { get; set; }
    }
}