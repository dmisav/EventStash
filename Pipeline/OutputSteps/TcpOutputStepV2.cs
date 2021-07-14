using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Pipeline.Models;

namespace Pipeline.OutputSteps
{
    public class TcpOutputStepV2 : IOutput<string>
    {
        private readonly int _port;
        private readonly int _parallelism;
        private ChannelReader<string> _in;

        public TcpOutputStepV2(int port, int parallelism = 20)
        {
            _port = port;
            _parallelism = parallelism;
        }

        public Task StartRoutine(CancellationToken ct)
        {
            return new(async () =>
            {
                while (!ct.IsCancellationRequested)
                {
                    var channels = CreateChannels();

                    Split(channels, ct);

                    await Task.WhenAll(channels.Select(channel => Process(channel)).ToArray());
                }
            }, ct, TaskCreationOptions.LongRunning);
        }

        private Channel<string>[] CreateChannels()
        {
            var outputs = new Channel<string>[_parallelism];
            for (var i = 0; i < _parallelism; i++)
            {
                var options = new BoundedChannelOptions(_parallelism)
                    {SingleReader = true, SingleWriter = true};
                outputs[i] = Channel.CreateBounded<string>(options);
            }

            return outputs;
        }

        private void Split(Channel<string>[] channels, CancellationToken ct)
        {
            Task.Run(async () =>
            {
                var index = 0;
                await foreach (var item in ReadFromChannelAsync(ct))
                {
                    await channels[index].Writer.WriteAsync(item);
                    index = (index + 1) % _parallelism;
                }

                foreach (var ch in channels)
                    ch.Writer.Complete();
            }, ct, TaskCreationOptions.LongRunning);
        }

        private async Task Process(ChannelReader<string> input)
        {
            var socketWrapper = new SocketWrapper(_port);
            socketWrapper.Start();

            await foreach (var item in input.ReadAllAsync())
            {
                socketWrapper.Send(item);
            }
        }

        public IAsyncEnumerable<string> ReadFromChannelAsync(CancellationToken ct) => _in.ReadAllAsync(ct);

        public void AssignInputChannel(ChannelReader<string> channel) => _in = channel;
    }
}