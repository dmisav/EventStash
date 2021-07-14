using System.Threading;

namespace Pipeline.Configuration
{
    public class ScalingOptions
    {
        public int ParallelCount { get; set; }
        public int MaxParallelCount { get; set; } = -1; // no limitation by default
        public int ScalingFactor { get; set; } = 2;
        public int TrendDecisionCount { get; set; }
        public CronOptions MonitoringOptions { get; set; }
    }
}
