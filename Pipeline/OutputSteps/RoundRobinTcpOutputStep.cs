using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Pipeline.Models;

namespace Pipeline.OutputSteps
{
    public class RoundRobinTcpOutputStep : IOutput<string>
    {
        private readonly RoundRobinTcpSender _tcpSender;
        private ChannelReader<string> _in;

        public RoundRobinTcpOutputStep(int port, int socketNumber = 20)
        {
            _tcpSender = new RoundRobinTcpSender(port, socketNumber);
        }

        public Task StartRoutine(CancellationToken ct)
        {
            return new(async () =>
            {
                while (true)
                {
                    await foreach (var msg in ReadFromChannelAsync(ct))
                    {
                        _ = _tcpSender.Send(msg);
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