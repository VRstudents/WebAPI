namespace WebAPI.Models.App
{
    public class ResultInQuestion
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public int LessonId { get; set; }
        public int QuestionNum { get; set; }
        public bool Result { get; set; }
    }
}