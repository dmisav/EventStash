using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;
using Pipeline.FakeSteps;
using Pipeline.InputSteps;
using Pipeline.OutputSteps;
using Pipeline.Steps;

namespace Start
{
    class Program
    {
        private static IServiceProvider _serviceProvider;
        static void Main(string[] args)
        {
            RegisterServices();

            Console.WriteLine("Starting Execution");

            var cts = new CancellationTokenSource();
            //new Pipeline.PipelineCore.Pipeline()
            //    .AddInput(new FakeInput())
            //    .AddStep(new FakeStep(1))
            //    .AddStep(new FakeStep(2))
            //    .AddOutput(new FakeOutput())
            //    .Create(cts.Token)
            //    .Start();
            new Pipeline.PipelineCore.Pipeline()
                .AddInput(new TcpInputStep(555))
                .AddStep(new ParserStep())
                .AddStep(new SerializationStep())
                //.AddOutput(new TcpOutputStepIOutput("127.0.0.1", 556))
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

        private static void RegisterServices()
        {
            var collection = new ServiceCollection();
            var builder = new ContainerBuilder();
            //example of registering type. Add yours
            //builder.RegisterType<DemoService>().As<IDemoService>;

            builder.Populate(collection);
            var appContainer = builder.Build();
            _serviceProvider = new AutofacServiceProvider(appContainer);

            //non autoFac usage
            //_serviceProvider = collection.BuildServiceProvider();
            //var service = serviceProvider.GetService<Idemoservice>
            //service.DoSomewthing()
        }

        private static void DisposeServices()
        {
            if (_serviceProvider == null)
            {
                return;
            }
            if (_serviceProvider is IDisposable)
            {
                ((IDisposable)_serviceProvider).Dispose();
            }
        }

    }
}
