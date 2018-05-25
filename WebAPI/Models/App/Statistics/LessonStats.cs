using System.Collections.Generic;

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
        public List<PersonDTO> Students { get; set; }

        public void SetDefaults()
        {
            StTried = StFinished = 0;
            FinishedOfTriedPercent = AvgRes = AvgBestRes = 0;
        }
    }
}