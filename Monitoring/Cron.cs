using Pipeline.Configuration;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Pipeline.Monitoring
{
    public class Cron
    {
        private Cron() {}

        public static async Task RecurrAsync(CronOptions options)
        {
            while (!options.CancellationToken.IsCancellationRequested)
            {
                options.ActionToPerform();
                await Task.Delay(TimeSpan.FromSeconds(options.IntervalSeconds), options.CancellationToken);
            }
        }
    }
}
