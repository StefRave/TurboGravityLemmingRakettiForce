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
        private BufferBlock<byte[]> bufferBlock;
        private IPAddress remoteEndPoint;

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
                    if(remoteEndPoint == null)
                        remoteEndPoint = receiveResult.RemoteEndPoint.Address;
                    else if (!remoteEndPoint.Equals(receiveResult.RemoteEndPoint.Address))
                        continue;
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

        public IEnumerable<byte[]> GetEvents()
        {
            while (true)
            {
                byte[] item;
                if (!bufferBlock.TryReceive(null, out item))
                    break;
                yield return item;
            }
        }
    }
}