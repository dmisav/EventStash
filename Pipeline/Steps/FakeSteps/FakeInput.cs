using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Pipeline.Models;
using Pipeline.PipelineCore;

namespace Pipeline.FakeSteps
{
    public class FakeInput : IInput<string>
    {
        private ChannelWriter<string> _out;

        public Task StartRoutine(CancellationToken ct)
        {
            var tokenWrapper = new CancellationTokenWrapper(ct);

            return new Task(async () =>
            {
                for (int i = 1; i <= 1000000; i++)
                {
                    await WriteToChannelAsync($"Input {i};", tokenWrapper);
                }
            }, ct);
        }

        public async ValueTask WriteToChannelAsync(string item, CancellationTokenWrapper tokenWrapper)
        {
            await _out.WriteAsync(item, tokenWrapper.Token);
        }

        public void AssignOutputChannel(ChannelWriter<string> channel)
        {
            _out = channel;
        }
    }
}
