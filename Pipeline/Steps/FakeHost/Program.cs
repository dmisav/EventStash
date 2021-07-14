using System;
using System.Threading;
using Pipeline.FakeSteps;
using ChannelPipeline = Pipeline.PipelineCore.Pipeline;

namespace FakeHost
{
    class Program
    {
        static void Main(string[] args)
        {
            var cts = new CancellationTokenSource();
            new ChannelPipeline()
                .AddInput(new FakeInput())
                .AddStep(new FakeStep(1))
                .AddStep(new FakeStep(2))
                .AddOutput(new FakeOutput())
                .Create(cts.Token)
                .Start();

            while (true)
            {
                var keyInput = Console.ReadKey(true);

                if (keyInput.Key == ConsoleKey.Escape)
                {
                    Console.WriteLine("Escape was pressed, cancelling...");
                    cts.Cancel();
                }
            }
        }
    }
}
