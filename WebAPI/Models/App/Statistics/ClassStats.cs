using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebAPI.Models.App.Statistics
{
    public class ClassStats
    {
        public List<LessonResDistribution> RDist { get; set; }
        public List<LessonAttempts> ADist { get; set; }
        public double AvgRes { get; set; }
    }
}