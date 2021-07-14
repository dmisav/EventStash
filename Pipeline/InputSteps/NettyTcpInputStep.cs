using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Coldairarrow.DotNettySocket;
using Pipeline.Models;
using Pipeline.PipelineCore;

namespace Pipeline.InputSteps
{
    public class NettyTcpInputStep : IInput<string>
    {
        private readonly int _port;
        private ChannelWriter<string> _out;
        private ITcpSocketServer _theServer;

        public NettyTcpInputStep(int port)
        {
            _port = port;
        }

        public Task StartRoutine(CancellationToken ct)
        {
            var tokenWrapper = new CancellationTokenWrapper(ct);

            return new(async () =>
            {
                _theServer = await SocketBuilderFactory.GetTcpSocketServerBuilder(_port)
                    .OnConnectionClose((server, connection) =>
                    {
                        Console.WriteLine($"Connection closed,Connection Name[{connection.ConnectionName}],Current number of connections:{server.GetConnectionCount()}");
                    })
                    .OnException(ex =>
                    {
                        Console.WriteLine($"Server side exception:{ex.Message}");
                    })
                    .OnNewConnection((server, connection) =>
                    {
                        connection.ConnectionName = $"Name{connection.ConnectionId}";
                        Console.WriteLine($"New Connection:{connection.ConnectionName},Current number of connections:{server.GetConnectionCount()}");
                    })
                    .OnRecieve(async (server, connection, bytes) =>
                    {
                        var message = Encoding.UTF8.GetString(bytes);
                        await WriteToChannelAsync(message, tokenWrapper);
                        //Console.WriteLine($"Server:data{message}");
                        //await connection.Send(bytes);
                    })
                    .OnServerStarted(server =>
                    {
                        Console.WriteLine($"Service Start");
                    }).BuildAsync();
            }, ct, TaskCreationOptions.LongRunning);
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
