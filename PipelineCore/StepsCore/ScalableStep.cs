using Pipeline.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Pipeline.PipelineCore.StepsCore
{
    public abstract class ScalableStep<TIn, TOut> : RegularStep<TIn, TOut>
    {
        private readonly ScalingOptions _scalingOptions;

        public ScalableStep(ScalingOptions scalingOptions) 
        {
            _scalingOptions = scalingOptions;
        }

        public override Task StartRoutine(CancellationToken ct)
        {
            return new Task(() =>
            {
                var splittedChannels = Split(_scalingOptions, ct);
                Merge(splittedChannels, ct);
            }, ct, TaskCreationOptions.LongRunning);

        }

        private IList<ChannelReader<TIn>> Split(ScalingOptions scalingOptions, CancellationToken ct)
        {
            var outputs = new Channel<TIn>[scalingOptions.ParallelCount];
            for (var i = 0; i < scalingOptions.ParallelCount; i++)
                outputs[i] = Channel.CreateUnbounded<TIn>(new UnboundedChannelOptions { SingleReader = true, SingleWriter = true });

            Task.Run(async () =>
            {
                var index = 0;
                await foreach (var item in ReadFromChannelAsync(ct))
                {
                    outputs[index].Writer.TryWrite(item);
                    index = (index + 1) % scalingOptions.ParallelCount;
                }

                foreach (var ch in outputs)
                    ch.Writer.Complete();

            }, ct);

            return outputs.Select(ch => ch.Reader).ToArray();
        }

        private void Merge(IList<ChannelReader<TIn>> inputs, CancellationToken ct)
        {
            Task.Run(async () =>
            {
                async Task Redirect(ChannelReader<TIn> input)
                {
                    await foreach (var item in input.ReadAllAsync())
                    {
                        var processedItem = ProcessItem(item);
                        await WriteToChannelAsync(processedItem, ct);
                    }
                }

                await Task.WhenAll(inputs.Select(i => Redirect(i)).ToArray());
            }, ct);
        }
    }
}
