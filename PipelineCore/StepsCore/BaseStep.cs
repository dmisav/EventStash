using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Pipeline.PipelineCore.StepsCore
{
    public abstract class BaseStep<TIn, TOut> : IStep<TIn, TOut>
    {
        protected readonly ChannelReader<TIn> ChannelIn;
        protected readonly ChannelWriter<TOut> ChannelOut;
        public BaseStep(ChannelReader<TIn> channelIn, ChannelWriter<TOut> channelOut)
        {
            ChannelIn = channelIn;
            ChannelOut = channelOut;
        }
        public abstract TOut ProcessItem(TIn item);

        public virtual IAsyncEnumerable<TIn> ReadFromChannelAsync(CancellationToken ct)
        {
            return ChannelIn.ReadAllAsync(ct);
        }

        public virtual Task StartRoutine(CancellationToken ct)
        {
            return new Task(async () =>
            {
                await foreach (var item in ReadFromChannelAsync(ct))
                {
                    var processedItem = ProcessItem(item);
                    await WriteToChannelAsync(processedItem, ct);
                }
            }, ct, TaskCreationOptions.LongRunning);
        }

        public virtual async Task WriteToChannelAsync(TOut item, CancellationToken ct)
        {
            await ChannelOut.WriteAsync(item, ct);
        }
    }
}
