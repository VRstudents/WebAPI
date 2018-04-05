using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebAPI.Models
{
    public class Lesson
    {
        public int Id { get; set; }
        public string Category { get; set; }
        public string Name { get; set; }
        public int SeqNum { get; set; }
    }
}