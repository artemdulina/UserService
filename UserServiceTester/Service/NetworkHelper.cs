using System;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;

namespace Service
{
    public static class NetworkHelper
    {
        public static bool IsPortOpened(IPAddress ipAddress, int port)
        {
            bool open = false;
            using (TcpClient tcpClient = new TcpClient())
            {
                try
                {
                    //Console.WriteLine("Right before tcp connect. Now wait...");
                    //Thread.Sleep(1000);
                    tcpClient.Connect(ipAddress, port);
                    open = true;
                }
                catch (Exception)
                {
                    // ignored
                }
            }

            return open;
        }

        public static bool IsPortOpenedWithoutTcp(int port)
        {
            IPGlobalProperties ipGlobalProperties = IPGlobalProperties.GetIPGlobalProperties();
            TcpConnectionInformation[] tcpConnInfoArray = ipGlobalProperties.GetActiveTcpConnections();

            return tcpConnInfoArray.All(tcpi => port != tcpi.LocalEndPoint.Port);
        }
    }
}
