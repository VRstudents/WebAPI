using System.Collections.Generic;

namespace WebAPI.Models.App.Statistics
{
    public class StudentStats
    {
        public List<LessonResDistribution> RDist { get; set; }
        public double AvgRes { get; set; }
    }
}