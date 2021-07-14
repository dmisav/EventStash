using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Pipeline.OutputSteps
{
    public class TcpSender
    {
        private Socket _socket;
        private string _serverHostName;
        private int _port;
        public TcpSender(string serverHostName, int port)
        {
            _serverHostName = serverHostName;
            _port = port;
        }

        public void StartClient()
        {
            {
                try
                {
                    IPHostEntry host = Dns.GetHostEntry(_serverHostName);
                    IPAddress ipAddress = host.AddressList[0];
                    IPEndPoint remoteEP = new IPEndPoint(ipAddress, _port);
                    _socket = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                    try
                    {
                        if (!_socket.Connected)
                        {
                            _socket.Connect(remoteEP);
                        }
                    }
                    catch (ArgumentNullException ane)
                    {
                        //todo                    
                    }
                    catch (SocketException se)
                    {
                        //todo
                        var i = 30;
                        while (i > 0)
                        {
                            Task.Delay(2000);
                            if (!_socket.Connected)
                            {
                                _socket.Connect(remoteEP);
                            }
                            i--;
                        }
                    }
                    catch (Exception e)
                    {
                        //todo
                    }

                }
                catch (Exception e)
                {
                    //todo
                }
            }
        }

        public void Send(string message)
        {
            byte[] msg = Encoding.ASCII.GetBytes(message);
            try
            {
                _socket.Send(msg);
            }
            catch (SocketException se)
            {
                StartClient();
                _socket.Send(msg);
            }
        }

        public void ShutClient()
        {
            _socket.Shutdown(SocketShutdown.Both);
            _socket.Close();
        }
    }
}
