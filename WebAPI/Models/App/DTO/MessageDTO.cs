using System;

namespace WebAPI.Models.App
{
    public class MessageDTO
    {
        public int ClassId { get; set; }
        public string Text { get; set; }
        public DateTime Date { get; set; }
        public string Category { get; set; }
    }
}