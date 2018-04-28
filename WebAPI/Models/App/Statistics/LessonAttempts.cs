using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebAPI.Models.App.Statistics
{
    public class LessonAttempts
    {
        public int LNum { get; set; }
        public int StTried { get; set; }
        public int StFinished { get; set; }
    }
}