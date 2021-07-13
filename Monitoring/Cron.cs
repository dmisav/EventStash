using Pipeline.Configuration;
using System;
using System.Threading.Tasks;

namespace Pipeline.Monitoring
{
    public class Cron
    {
        private Cron() {}

        public static async Task RecurrAsync(Action actionToPerform, CronOptions options)
        {
            while (!options.CancellationToken.IsCancellationRequested)
            {
                actionToPerform();
                await Task.Delay(TimeSpan.FromSeconds(options.IntervalSeconds), options.CancellationToken);
            }
        }
    }
}
