using System.Net;
using System.Net.Sockets;

namespace TurboPort.Event
{
    public class UdpBroadcastEvents
    {
        private readonly UdpClient udpClient;
        private readonly IPEndPoint broadcastIp;

        public UdpBroadcastEvents()
        {
            udpClient = new UdpClient();
            broadcastIp = new IPEndPoint(IPAddress.Parse("192.168.0.255"), 15000);
        }

        public void BroadCast(byte[] data, int length)
        {
            udpClient.Send(data, length, broadcastIp);
        }
    }
}