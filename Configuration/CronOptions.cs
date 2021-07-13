using System.Threading;

namespace Pipeline.Configuration
{
    public class CronOptions
    {
        public int IntervalSeconds { get; set; }
        public CancellationToken CancellationToken { get; set; }
    }
}
