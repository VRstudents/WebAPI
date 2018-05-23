namespace WebAPI.Models.App
{
    public class ResultInLesson
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public int LessonId { get; set; }
        public int Result { get; set; }
    }
}