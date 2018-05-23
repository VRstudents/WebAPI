namespace WebAPI.Models.App
{
    public class ResultInExamQuest
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public int QuestionId { get; set; }
        public bool Result { get; set; }
    }
}