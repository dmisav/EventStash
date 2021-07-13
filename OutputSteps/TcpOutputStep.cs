using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Pipeline.Models;

namespace Pipeline.OutputSteps
{
    public class TcpOutputStepIOutput : IOutput<string>
    {
        private readonly int _port;
        private ChannelReader<string> _in;

        public async IAsyncEnumerable<string> ReadFromChannelAsync(CancellationToken ct)
        {
            await foreach (var dataPoint in _in.ReadAllAsync(ct))
            {
                yield return dataPoint;
            }
        }

        public Task StartRoutine(CancellationToken ct)
        {
            throw new NotImplementedException();
        }

        public void AssignInputChannel(ChannelReader<string> channel)
        {
            _in = channel;
        }
    }
}
