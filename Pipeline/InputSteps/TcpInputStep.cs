using System;
using System.Buffers;
using System.IO.Pipelines;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Pipeline.Models;

namespace Pipeline.InputSteps
{
    public class TcpInputStep : IInput<string>
    {
        private readonly int _port;
        private ChannelWriter<string> _out;

        public TcpInputStep(int port) => _port = port;

        public async ValueTask WriteToChannelAsync(string item, CancellationToken ct)
        {
            await _out.WriteAsync(item, ct);
        }

        public Task StartRoutine(CancellationToken ct)
        {
            return new(async () =>
            {
                var listenSocket = new Socket(SocketType.Stream, ProtocolType.Tcp);
                listenSocket.Bind(new IPEndPoint(IPAddress.Loopback, _port));
                listenSocket.Listen();

                while (true)
                {
                    var socket = await listenSocket.AcceptAsync();
                    _ = ProcessLinesAsync(socket, ct);
                }
            }, ct);
        }

        public void AssignOutputChannel(ChannelWriter<string> channel)
        {
            _out = channel;
        }

        private async Task ProcessLinesAsync(Socket socket, CancellationToken ct)
        {
            // Create a PipeReader over the network stream
            var stream = new NetworkStream(socket);
            var reader = PipeReader.Create(stream);

            while (true)
            {
                ReadResult result = await reader.ReadAsync(ct);
                ReadOnlySequence<byte> buffer = result.Buffer;

                while (TryReadLine(ref buffer, out ReadOnlySequence<byte> line))
                {
                    // Process the line.
                    foreach (var segment in line)
                    {
                        await WriteToChannelAsync(Encoding.UTF8.GetString(segment.Span), ct);
                    }
                }

                // Tell the PipeReader how much of the buffer has been consumed.
                reader.AdvanceTo(buffer.Start, buffer.End);

                // Stop reading if there's no more data coming.
                if (result.IsCompleted)
                {
                    break;
                }
            }

            // Mark the PipeReader as complete.
            await reader.CompleteAsync();
        }

        private static bool TryReadLine(ref ReadOnlySequence<byte> buffer, out ReadOnlySequence<byte> line)
        {
            // Look for a EOL in the buffer.
            SequencePosition? position = buffer.PositionOf((byte) '\n');

            if (position == null)
            {
                line = default;
                return false;
            }

            // Skip the line + the \n.
            line = buffer.Slice(0, position.Value);
            buffer = buffer.Slice(buffer.GetPosition(1, position.Value));
            return true;
        }
    }
}