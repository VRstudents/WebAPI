namespace WebAPI.Models
{
    public class Lesson
    {
        public int Id { get; set; }
        public string Category { get; set; }
        public string Name { get; set; }
        public int Grade { get; set; }
        public int SeqNum { get; set; }
        public string Description { get; set; }
    }
}