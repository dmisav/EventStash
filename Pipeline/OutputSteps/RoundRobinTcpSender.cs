using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Pipeline.OutputSteps
{
    public class RoundRobinTcpSender
    {
        private readonly SocketWrapper[] _sockets;
        private readonly RoundRobinNumber _roundRobinNumber;

        public RoundRobinTcpSender(int port, int socketNumber)
        {
            _sockets = new SocketWrapper[socketNumber];
            _roundRobinNumber = new RoundRobinNumber(socketNumber);

            for (var i = 0; i < socketNumber; i++)
            {
                _sockets[i] = new SocketWrapper(port);
            }
        }

        public Task Send(string message)
        {
            return Task.Run(() =>
            {
                int nextNumber = _roundRobinNumber.GetNextNumber();
                _sockets[nextNumber].Send(message);
            });
        }
    }

    public class SocketWrapper
    {
        private readonly Socket _socket;
        private readonly int _port;

        public SocketWrapper(int port)
        {
            _socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
            _port = port;
            Start();
        }

        public void Start()
        {
            try
            {
                _socket.Connect(new IPEndPoint(IPAddress.Loopback, _port));
            }
            catch (SocketException exception)
            {
            }
        }

        public void Send(string message)
        {
            byte[] msg = Encoding.ASCII.GetBytes(message);
            SendBytes(msg, true);
        }

        private void SendBytes(byte[] bytes, bool tryReconnect)
        {
            try
            {
                _socket.Send(bytes);
            }
            catch (SocketException ex)
            {
                if (tryReconnect)
                {
                    Start();
                    SendBytes(bytes, false);
                }

                // drop message
            }
        }
    }

    public class RoundRobinNumber
    {
        private readonly int _maxNumbers;
        private int _lastNumber = 0;

        public RoundRobinNumber(int maxNumbers)
        {
            _maxNumbers = maxNumbers;
        }

        public int GetNextNumber()
        {
            int nextNumber = Interlocked.Increment(ref _lastNumber);

            int result = nextNumber % _maxNumbers;

            return result >= 0 ? result : -result;
        }
    }
}