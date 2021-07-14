using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Drawing;
using System.Text;
using System.Threading;
using Pipeline.FakeSteps;
using Pipeline.InputSteps;
using Pipeline.OutputSteps;
using Pipeline.Steps;
using Start.Image;

namespace Start
{
    class Program
    {
        private static IServiceProvider _serviceProvider;
        static void Main(string[] args)
        {
            RegisterServices();

            DrawLogo();

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

        private static void DrawLogo()
        {
            Console.OutputEncoding = Encoding.UTF8;

            Console.WriteLine("                                                    ");
            Console.WriteLine(" /\\   ‾\\  /‾‾‾‾\\  /‾‾‾‾\\  /‾‾‾‾\\  ‾|‾  /‾‾‾‾\\ ");
            Console.WriteLine(" | \\   |  |       |       |        |   |           ");
            Console.WriteLine(" |  \\  |  |----|  \\----\\  \\----\\   |   |----|  ");
            Console.WriteLine(" |   \\ |  |            |       |   |   |           ");
            Console.WriteLine(" \\_   \\/  \\____/  \\____/  \\____/  _|_  \\____/ ");
            Console.WriteLine("                                                    ");

            Bitmap bmpSrc = new Bitmap(@"Resources\Nessie.bmp", true);
            ImageDrawer.ConsoleWriteImage(bmpSrc);

            Console.WriteLine();
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
