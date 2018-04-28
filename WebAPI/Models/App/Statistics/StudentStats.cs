using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebAPI.Models.App.Statistics
{
    public class StudentStats
    {
        public List<LessonResDistribution> RDist { get; set; }
        public double AvgRes { get; set; }
    }
}