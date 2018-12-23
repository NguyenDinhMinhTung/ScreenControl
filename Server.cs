using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace ScreenControl
{
    class Server
    {
        public delegate void ActionAcceptConnect(String ip, byte[] data);
        public delegate void ActionReceive(byte[] data);
        public delegate void ReceiveMessage(byte[] data);

        const int port = 6000;

        Socket server;
        IPEndPoint targetIP;
        EndPoint Remote;

        Thread thReceiveMess;
        
        public bool isConnected = false;

        ActionAcceptConnect AcceptConnect;
        ActionReceive Receive;
        ReceiveMessage receiveMessage;

        public Server(ActionAcceptConnect AcceptConnect, ActionReceive Receive)
        {
            this.AcceptConnect = AcceptConnect;
            this.Receive = Receive;
        }

        public void StartServer()
        {
            targetIP = getClientIP();
            server = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            //string welcome = "Hello server";
            //data = Encoding.ASCII.GetBytes(welcome);
            //server.SendTo(data, data.Length, SocketFlags.None, ipep);
            IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
            Remote = (EndPoint)sender;
            isConnected = true;
            AcceptConnect("", null);

            thReceiveMess = new Thread(thReceiveMessage);
            thReceiveMess.IsBackground = true;
            thReceiveMess.Start();

            SendData(0, 0, 0);
            //data = new byte[1024];
            //int recv = server.ReceiveFrom(data, ref Remote);
            //Console.WriteLine("Thong diep duoc nhan tu {0}:", Remote.ToString());
            //Console.WriteLine(Encoding.ASCII.GetString(data, 0, recv));
        }

        private void thReceiveMessage()
        {
            byte[] data;
            while (true)
            {
                data = new byte[1024];
                int recv = server.ReceiveFrom(data, ref Remote);
                receiveMessage(data);
            }
        }

        IPEndPoint getClientIP()
        {
            while (true)
            {
                var remote = new IPEndPoint(IPAddress.Any, port);
                var client = new UdpClient(port);
                var buffer = client.Receive(ref remote);
                var result = Encoding.UTF8.GetString(buffer);

                if (result.ToUpper().StartsWith("IP"))
                {
                    var ip = result.Substring(2);
                    client.Close();
                    return new IPEndPoint(IPAddress.Parse(ip), port);
                }
            }
        }
        public void close()
        {
            SendData(9, 0, 0);
            server?.Close();
            if ((bool)thReceiveMess?.IsAlive)
            {
                thReceiveMess.Abort();
            }
        }

        public void Send(byte[] data)
        {
            if (isConnected)
                server.SendTo(data, data.Length, SocketFlags.None, targetIP);
        }

        public void SendData(byte comm, double x, double y)
        {
            if (isConnected)
            {
                byte[] data = new byte[17];

                data[0] = comm;

                byte[] bx = BitConverter.GetBytes(x);
                byte[] by = BitConverter.GetBytes(y);

                for (int i = 0; i < 8; i++)
                {
                    data[i + 1] = bx[i];
                    data[i + 9] = by[i];
                }

                server.SendTo(data, data.Length, SocketFlags.None, targetIP);
            }
        }
    }
}
