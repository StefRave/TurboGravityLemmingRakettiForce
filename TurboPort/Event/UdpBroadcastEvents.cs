using System;
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

        public void BroadCast(NetworkData data)
        {
            byte[] dgram = new byte[data.Data.Count + 2];
            dgram[0] = (byte) (data.PlayerId / 256);
            dgram[1] = (byte)data.PlayerId;
            Array.Copy(data.Data.Array, data.Data.Offset, dgram, 2, data.Data.Count);
            udpClient.Send(dgram, dgram.Length, broadcastIp);
        }
    }
}