using Pipeline.Configuration;
using System;
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
        public ScalableStep(ChannelReader<TIn> channelIn, ChannelWriter<TOut> channelOut, ScalingOptions scalingOptions) : base(channelIn, channelOut) 
        {
            _scalingOptions = scalingOptions ?? throw new ArgumentNullException(nameof(scalingOptions));
        }

        public override Task StartRoutine(CancellationToken ct)
        {
            return new (() =>
            {
                var splittedChannels = Split(_scalingOptions, ct);
                Merge(splittedChannels, ct);
            }, ct, TaskCreationOptions.LongRunning);

        }

        private IList<ChannelReader<TIn>> Split(ScalingOptions scalingOptions, CancellationToken ct)
        {
            var parallelCount = scalingOptions.MaxParallelCount == 0 ? scalingOptions.ParallelCount : Math.Min(scalingOptions.ParallelCount, scalingOptions.MaxParallelCount);

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

        private void Scale()
        {
            var currParallelCount = _scalingOptions.ParallelCount;
            var newParallelCount = _scalingOptions.ScalingFactor * currParallelCount;

            if (_scalingOptions.MaxParallelCount != -1)
                newParallelCount = Math.Min(newParallelCount, _scalingOptions.MaxParallelCount);

            _scalingOptions.ParallelCount = newParallelCount;
        }

        private void Unscale()
        {
            var currParallelCount = _scalingOptions.ParallelCount;
            var newParallelCount = currParallelCount / _scalingOptions.ScalingFactor;

            _scalingOptions.ParallelCount = Math.Max(newParallelCount, 1);
        }
    }
}
