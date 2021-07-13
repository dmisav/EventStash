using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Pipeline.Models;

namespace Pipeline.FakeSteps
{
    public class FakeInput : IInput<string>
    {
        private ChannelWriter<string> _out;

        public Task StartRoutine(CancellationToken ct)
        {
            return new Task(async () =>
            {
                for (int i = 0; i < 100; i++)
                {
                    await WriteToChannelAsync($"Input {i};", ct);
                }
            }, ct);
        }

        public async Task WriteToChannelAsync(string item, CancellationToken ct)
        {
            await _out.WriteAsync(item, ct);
        }

        public void AssignOutputChannel(ChannelWriter<string> channel)
        {
            _out = channel;
        }
    }
}
