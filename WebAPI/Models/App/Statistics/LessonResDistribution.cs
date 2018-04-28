using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebAPI.Models.App.Statistics
{
    public class LessonResDistribution
    {
        public int LNum { get; set; }
        public double AvgRes { get; set; }
        public double AvgBestRes { get; set; }
        public double BestRes { get; set; }
    }
}