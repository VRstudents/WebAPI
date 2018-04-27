using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebAPI.Models.App.Statistics
{
    public class LessonStats
    {
        public List<QResLessonDistribution> QLDist { get; set; }
        public int StTried { get; set; }
        public int StFinished { get; set; }
        public double FinishedOfTriedPercent { get; set; }
        public double AvgRes { get; set; }
        public double AvgBestRes { get; set; }

        public void SetDefaults()
        {
            StTried = StFinished = 0;
            FinishedOfTriedPercent = AvgRes = AvgBestRes = 0;
        }
    }

    public class QResLessonDistribution
    {
        public int QNum { get; set; }
        public double RightCount { get; set; }
        public double WrongCount { get; set; }
    }
}