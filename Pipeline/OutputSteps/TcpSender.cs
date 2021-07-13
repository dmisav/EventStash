using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Pipeline.OutputSteps
{
    public class TcpSender
    {        
        Socket _socket;
        public void StartClient(string serverHostName,int port)
        {
            byte[] bytes = new byte[1024];

            try
            {
                IPHostEntry host = Dns.GetHostEntry(serverHostName);
                IPAddress ipAddress = host.AddressList[0];
                IPEndPoint remoteEP = new IPEndPoint(ipAddress, port);
                _socket = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
             
                try
                {
                    _socket.Connect(remoteEP);
                }
                catch (ArgumentNullException ane)
                {
                   //todo
                }
                catch (SocketException se)
                {
                   //todo
                }
                catch (Exception e)
                {
                   //todo
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        public void Send(string message)
        {
          byte[] msg = Encoding.ASCII.GetBytes(message);
          _socket.Send(msg);
        }

        public void ShutClient()
        {
            _socket.Shutdown(SocketShutdown.Both);
            _socket.Close();
        }
    }
}
