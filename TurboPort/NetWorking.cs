using System;
using System.Collections;

namespace tglrf
{
    public enum NetworkMessageType
    {
        ChatMessage,
    }


    /// <summary>
	/// Summary description for Networking.
	/// </summary>
	public class Networking
	{
        public delegate void PlayerChangedEventHandler(object sender);
        public delegate void MessageReceivedEventHandler(PlayerInformation playerName, string message);

        static private Networking instance;
        private Peer peer;
        private Hashtable playerList = new Hashtable();
        private int localPlayerId;

        public event PlayerChangedEventHandler PlayerChanged;
        public event MessageReceivedEventHandler MessageReceived;


        public PlayerInformation[] GetPlayerList()
        {
            PlayerInformation[] result = new PlayerInformation[playerList.Count];
            playerList.Values.CopyTo(result, 0);
            return result;
        }

		private Networking()
		{
            peer = new Peer();
            //peer.ApplicationDescription.SessionName = "Turbo Gravity Lemming Raketty Force";
            peer.PlayerCreated += new PlayerCreatedEventHandler(peer_PlayerCreated);
            peer.PlayerDestroyed += new PlayerDestroyedEventHandler(peer_PlayerDestroyed);
            peer.PeerInformation += new PeerInformationEventHandler(peer_PeerInformation);
            peer.Receive += new ReceiveEventHandler(peer_Receive);
		}

        public void SetPlayerInformation(string playerName)
        {
            PlayerInformation myinformation = new PlayerInformation();
            myinformation.Name = playerName;
            peer.SetPeerInformation(myinformation, 0);
        }

        /*public void Host(string sessionName)
        {
            Address hostAddress = new Address();
            hostAddress.ServiceProvider = Address.ServiceProviderTcpIp;  
            // Select TCP/IP service provider

            ApplicationDescription dpApp = new ApplicationDescription();
            dpApp.Flags = SessionFlags.FastSigned;
            dpApp.GuidApplication = Guid.NewGuid();     // Set the application GUID
            dpApp.SessionName = sessionName;            // Optional Session Name

            peer.Host(dpApp, hostAddress, HostFlags.OkToQueryForAddressing);
        }*/

        public bool DoWizzard()
        {
            ConnectWizard connectWizard = new ConnectWizard(peer, new Guid("ED48D6E8-91D1-4cf6-9ED3-CB427F76F17D"), "Tglrf");
            connectWizard.DefaultPort = 10293;
            return connectWizard.StartWizard();
        }

        public static Networking GetInstance()
        {
            if(instance == null)
            {
                instance = new Networking();
            }
            return instance;
        }

        private void peer_PlayerCreated(object sender, PlayerCreatedEventArgs e)
        {
            PlayerInformation peerInfo = peer.GetPeerInformation(e.Message.PlayerID);
            //Players oPlayer = new Players(e.Message.PlayerID,peerInfo.Name);
            // We lock the data here since it is shared across multiple threads.
            lock(playerList)
            {
                playerList[e.Message.PlayerID] = peerInfo;
            }
            if (peerInfo.Local)
            {
                localPlayerId = e.Message.PlayerID;
            }
            if(PlayerChanged != null)
            {
                PlayerChanged(this);
            }
        }

        private void peer_PlayerDestroyed(object sender, PlayerDestroyedEventArgs e)
        {
            lock(playerList)
            {
                playerList.Remove(e.Message.PlayerID);
            }
            if(PlayerChanged != null)
            {
                PlayerChanged(this);
            }
        }
        private void peer_PeerInformation(object sender, PeerInformationEventArgs e)
        {
            PlayerInformation peerInfo = peer.GetPeerInformation(e.Message.PeerID);
            //Players oPlayer = new Players(e.Message.PlayerID,peerInfo.Name);
            // We lock the data here since it is shared across multiple threads.
            lock(playerList)
            {
                playerList[e.Message.PeerID] = peerInfo;
            }
            if(PlayerChanged != null)
            {
                PlayerChanged(this);
            }
        }

        private void peer_Receive(object sender, ReceiveEventArgs e)
        {
            NetworkPacket packet = e.Message.ReceiveData;
            PlayerInformation player = (PlayerInformation)playerList[e.Message.SenderID];

            while(packet.Position < packet.Length)
            {
                NetworkMessageType messageType = (NetworkMessageType)(int)packet.Read(typeof(int));
                switch(messageType)
                {
                    case NetworkMessageType.ChatMessage:
                        string chatMessage = packet.ReadString();
                        this.MessageReceived(player, chatMessage);
                        break;
                }
            }
            e.Message.ReceiveData.Dispose(); // We no longer need the data, Dispose the buffer
        }

        public void SendChatMessage(string chatMessage)
        {
            NetworkPacket packet = new NetworkPacket();
            packet.Write((int)NetworkMessageType.ChatMessage);
            packet.Write(chatMessage);
            peer.SendTo((int)PlayerID.AllPlayers, packet, 0, SendFlags.Guaranteed);
        }

    }
}
