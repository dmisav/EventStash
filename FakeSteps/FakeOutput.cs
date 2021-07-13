using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Pipeline.Models;

namespace Pipeline.FakeSteps
{
    public class FakeOutput : IOutput<string>
    {
        private ChannelReader<string> _in;

        public Task StartRoutine(CancellationToken ct)
        {
            return new Task(async () =>
            {
                while (!ct.IsCancellationRequested)
                {
                    await foreach (var item in ReadFromChannelAsync(ct))
                    {
                        var output = $"{item} Fake output completed.";
                        Console.WriteLine(output);
                    }
                }
            }, ct);
        }

        public IAsyncEnumerable<string> ReadFromChannelAsync(CancellationToken ct)
        {
            return _in.ReadAllAsync(ct);
        }

        public void AssignInputChannel(ChannelReader<string> channel)
        {
            _in = channel;
        }
    }
}
