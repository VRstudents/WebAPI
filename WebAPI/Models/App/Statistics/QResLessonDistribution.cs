using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebAPI.Models.App.Statistics
{
    public class QResLessonDistribution
    {
        public int QNum { get; set; }
        public double RightCount { get; set; }
        public double WrongCount { get; set; }
    }
}