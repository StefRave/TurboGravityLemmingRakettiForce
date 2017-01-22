using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace TurboPort.Event
{
    public class UdpEventReceiver
    {
        private UdpClient udpClient;
        private readonly IPEndPoint broadcastIp;
        private readonly BufferBlock<byte[]> bufferBlock;

        public UdpEventReceiver()
        {
            broadcastIp = new IPEndPoint(IPAddress.Any, 15000);
            bufferBlock = new BufferBlock<byte[]>(new DataflowBlockOptions { BoundedCapacity = 1000 });
        }

        public async Task StartReceivingAsync()
        {
            udpClient = new UdpClient();
            udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            udpClient.Client.Bind(broadcastIp);
            try
            {
                while (true)
                {
                    var receiveResult = await udpClient.ReceiveAsync();
                    bufferBlock.Post(receiveResult.Buffer);
                }

            }
            catch (Exception ex)
            {
                Debugger.Launch();
                ex.ToString();
            }
        }

        public void StopReceiving()
        {
            udpClient?.Close();
        }

        public IEnumerable<NetworkData> GetEvents()
        {
            while (true)
            {
                byte[] item;
                if (!bufferBlock.TryReceive(null, out item))
                    break;
                if(item.Length < 2)
                    continue;
                int senderId = item[0] * 256 + item[1];
                yield return new NetworkData {PlayerId = senderId, Data = new ArraySegment<byte>(item, 2, item.Length - 2)};
            }
        }
    }

    public struct NetworkData
    {
        public int PlayerId;
        public ArraySegment<byte> Data;
    }
}