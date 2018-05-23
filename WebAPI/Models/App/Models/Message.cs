using System;

namespace WebAPI.Models.App
{
    public class Message
    {
        public int Id { get; set; }
        public int ClassId { get; set; }
        public string Text { get; set; }
        public DateTime Date { get; set; }
    }
}