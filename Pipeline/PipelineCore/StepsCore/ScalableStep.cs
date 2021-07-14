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
    public abstract class ScalableStep<TIn, TOut> : BaseStep<TIn, TOut>
    {
        private readonly ScalingOptions _scalingOptions;
        private readonly TrendChecker _trendChecker;
        private readonly SemaphoreSlim _taskReloadGate;
        public ScalableStep(ScalingOptions scalingOptions) 
        {
            _scalingOptions = scalingOptions ?? throw new ArgumentNullException(nameof(scalingOptions));
            _trendChecker = new TrendChecker(scalingOptions.TrendDecisionCount);
            _taskReloadGate = new SemaphoreSlim(0);

            _trendChecker.StateActionRequired += ProcessStateAction;
        }

        public override Task StartRoutine(CancellationToken ct)
        {
            Task.Run(async () =>
            {
                await MonitorAsync(_scalingOptions.MonitoringOptions);
            }, ct);

            return new(async () =>
            {
                while (!ct.IsCancellationRequested)
                {
                    var scalingCts = new CancellationTokenSource();
                    var inputs = Split(_scalingOptions, scalingCts.Token);
                    var readers = inputs.Select(i => i.Reader).ToArray();

                    Merge(readers, ct);

                    await _taskReloadGate.WaitAsync(ct);

                    foreach (var ch in inputs)
                        ch.Writer.Complete();

                    scalingCts.Cancel();
                }
                
            }, ct, TaskCreationOptions.LongRunning);
        }

        private IList<Channel<TIn>> Split(ScalingOptions scalingOptions, CancellationToken ct)
        {
            var outputs = new Channel<TIn>[scalingOptions.ParallelCount];
            for (var i = 0; i < scalingOptions.ParallelCount; i++)
            {
                var options = new BoundedChannelOptions(50) { SingleReader = true, SingleWriter = true};
                outputs[i] = Channel.CreateBounded<TIn>(options);
            }

            Task.Run(async () =>
            {
                var index = 0;
                await foreach (var item in ChannelIn.ReadAllAsync(ct))
                {
                    await outputs[index].Writer.WriteAsync(item);
                    index = (index + 1) % scalingOptions.ParallelCount;
                }
            }, ct);

            return outputs;
        }

        private void Merge(IList<ChannelReader<TIn>> inputs, CancellationToken ct)
        {
            Task.Run(async () =>
            {
                async Task Redirect(ChannelReader<TIn> input)
                {
                    await foreach (var item in input.ReadAllAsync(ct))
                    {
                        var processedItem = ProcessItem(item);
                        await WriteToChannelAsync(processedItem, ct);
                    }
                }

                await Task.WhenAll(inputs.Select(i => Redirect(i)).ToArray());
            }, ct);
        }

        private void ProcessStateAction(object sender, State state)
        {
            switch (state)
            {
                case State.Growing:
                    var scaledCount = AutoScalerHelper.ScaleParallelCount(_scalingOptions);

                    if (_scalingOptions.ParallelCount == scaledCount)
                        return;

                    _scalingOptions.ParallelCount = scaledCount;
                    Console.WriteLine($"INCREASED: {_scalingOptions.ParallelCount }");
                    break;
                case State.Sinking:
                    var unscaledCount = AutoScalerHelper.UnscaleParallelCount(_scalingOptions);

                    if (_scalingOptions.ParallelCount == unscaledCount)
                        return;

                    _scalingOptions.ParallelCount = unscaledCount;
                    Console.WriteLine($"DECCREASED: {_scalingOptions.ParallelCount }");
                    break;
                case State.Steady:
                    return;
            }

            _taskReloadGate.Release();
        }

        private async Task MonitorAsync(CronOptions cronOptions)
        {
            var monitoringAction = new Action(() =>
            {
                var count = GetCount();
                Console.WriteLine($"NUmber of channels: {_scalingOptions.ParallelCount}");
                Console.WriteLine($"Events in the queue: {count}");
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
