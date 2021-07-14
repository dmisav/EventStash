using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Pipeline.Models;

namespace Pipeline.PipelineCore.StepsCore
{
    public abstract class BaseStep<TIn, TOut> : IStep<TIn, TOut>
    {
        protected ChannelReader<TIn> ChannelIn;
        protected ChannelWriter<TOut> ChannelOut;

        public abstract TOut ProcessItem(TIn item);

        public virtual IAsyncEnumerable<TIn> ReadFromChannelAsync(CancellationTokenWrapper tokenWrapper)
        {
            return ChannelIn.ReadAllAsync(tokenWrapper.Token);
        }

        public virtual Task StartRoutine(CancellationToken ct)
        {
            var tokenWrapper = new CancellationTokenWrapper(ct);

            return new Task(async () =>
            {
                while (!ct.IsCancellationRequested)
                {
                    await foreach (var item in ReadFromChannelAsync(tokenWrapper))
                    {
                        var processedItem = ProcessItem(item);
                        await WriteToChannelAsync(processedItem, tokenWrapper);
                    }
                }
            }, ct, TaskCreationOptions.LongRunning);
        }

        public void AssignInputChannel(ChannelReader<TIn> channel)
        {
            ChannelIn = channel;
        }

        public void AssignOutputChannel(ChannelWriter<TOut> channel)
        {
            ChannelOut = channel;
        }

        public virtual async ValueTask WriteToChannelAsync(TOut item, CancellationTokenWrapper tokenWrapper)
        {
            await ChannelOut.WriteAsync(item, tokenWrapper.Token);
        }
    }
}
