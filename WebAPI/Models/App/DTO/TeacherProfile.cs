using System.Collections.Generic;

namespace WebAPI.Models
{
    public class TeacherProfile
    {
        public List<string> categories { get; set; }
        public List<SCourses> classes { get; set; }
        public string Picture { get; set; }
    }
}