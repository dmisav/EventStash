using Pipeline.AutoScaling;
using Pipeline.Configuration;
using Pipeline.Helpers;
using Pipeline.Models;
using Pipeline.Monitoring;
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
        private readonly TrendChecker _trendChecker;

        private Task _currentRoutine;

        public ScalableStep(ScalingOptions scalingOptions) 
        {
            _scalingOptions = scalingOptions ?? throw new ArgumentNullException(nameof(scalingOptions));

            _trendChecker = new TrendChecker(scalingOptions.TrendDecisionCount);

            _trendChecker.StateActionRequired += ProcessStateAction;

            Task.Run(async () => 
            {
                await MonitorAsync(_scalingOptions.MonitoringOptions);
            }, _scalingOptions.MonitoringOptions.CancellationToken);
        }

        public override Task StartRoutine(CancellationToken ct)
        {
            _currentRoutine = new(() =>
            {
                var cancellationTokenWrapper = new CancellationTokenWrapper(ct);
                var splittedChannels = Split(_scalingOptions, cancellationTokenWrapper);
                Merge(splittedChannels, cancellationTokenWrapper);
            }, ct, TaskCreationOptions.LongRunning);

            return _currentRoutine;
        }

        private IList<ChannelReader<TIn>> Split(ScalingOptions scalingOptions, CancellationTokenWrapper cancellationTokenWrapper)
        {
            var parallelCount = scalingOptions.MaxParallelCount == 0 ? scalingOptions.ParallelCount : Math.Min(scalingOptions.ParallelCount, scalingOptions.MaxParallelCount);

            var outputs = new Channel<TIn>[scalingOptions.ParallelCount];
            for (var i = 0; i < scalingOptions.ParallelCount; i++)
                outputs[i] = Channel.CreateUnbounded<TIn>(new UnboundedChannelOptions { SingleReader = true, SingleWriter = true });

            Task.Run(async () =>
            {
                var index = 0;
                await foreach (var item in ReadFromChannelAsync(cancellationTokenWrapper))
                {
                    outputs[index].Writer.TryWrite(item);
                    index = (index + 1) % scalingOptions.ParallelCount;
                }

                foreach (var ch in outputs)
                    ch.Writer.Complete();

            }, cancellationTokenWrapper.Token);

            return outputs.Select(ch => ch.Reader).ToArray();
        }

        private void Merge(IList<ChannelReader<TIn>> inputs, CancellationTokenWrapper cancellationTokenWrapper)
        {
            Task.Run(async () =>
            {
                async Task Redirect(ChannelReader<TIn> input)
                {
                    await foreach (var item in input.ReadAllAsync())
                    {
                        var processedItem = ProcessItem(item);
                        await WriteToChannelAsync(processedItem, cancellationTokenWrapper);
                    }
                }

                await Task.WhenAll(inputs.Select(i => Redirect(i)).ToArray());
            }, cancellationTokenWrapper.Token);
        }

        private void ProcessStateAction(object sender, State state)
        {
            switch (state)
            {
                case State.Growing:
                    _scalingOptions.ParallelCount = AutoScalerHelper.ScaleParallelCount(_scalingOptions);
                    break;
                case State.Sinking:
                    _scalingOptions.ParallelCount = AutoScalerHelper.UnscaleParallelCount(_scalingOptions);
                    break;
                case State.Steady:
                    break;
            }

            _scalingOptions.Gate.Release();
        }

        private async Task MonitorAsync(CronOptions cronOptions)
        {
            var monitoringAction = new Action(() =>
            {
                var count = GetCount();
                _trendChecker.UpdateCount(count);
            });


            await Cron.RecurrAsync(monitoringAction, cronOptions);
        }

        private int GetCount()
        {
            return ChannelIn.Count;
        }
    }
}
