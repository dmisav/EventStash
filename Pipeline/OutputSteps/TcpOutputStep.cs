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
        private readonly string _server;
        private ChannelReader<string> _in;
        private TcpSender _tcpSender;


        public TcpOutputStepIOutput(string server, int port)
        {
            _server = server;
            _port = port;
        }

        public async IAsyncEnumerable<string> ReadFromChannelAsync(CancellationToken ct)
        {
            await foreach (var dataPoint in _in.ReadAllAsync(ct))
            {
                yield return dataPoint;
            }
        }

        public Task StartRoutine(CancellationToken ct)
        {
            return new(async () =>
            {
                _tcpSender = new TcpSender(_server, _port);
                _tcpSender.StartClient();
                
                while (true)
                {
                    await foreach (var msg in ReadFromChannelAsync(ct))
                    {
                        _tcpSender.Send(msg);
                    }
                }
            }, ct);
        }

        public void AssignInputChannel(ChannelReader<string> channel)
        {
            _in = channel;
        }
    }
}
