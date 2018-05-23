namespace WebAPI.Models
{
    public class LessonDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int SeqNum { get; set; }
        public bool IsActive { get; set; }
        public bool IsPassed { get; set; }
        public string Category { get; set; }
        public int Result { get; set; }
        public string Description { get; set; }
    }
}