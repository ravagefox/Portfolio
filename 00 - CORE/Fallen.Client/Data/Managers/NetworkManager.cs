// Source: NetworkManager
/* 
   ---------------------------------------------------------------
                        CREXIUM PTY LTD
   ---------------------------------------------------------------

     The software is provided 'AS IS', without warranty of any kind,
   express or implied, including but not limited to the warrenties
   of merchantability, fitness for a particular purpose and
   noninfringement. In no event shall the authors or copyright
   holders be liable for any claim, damages, or other liability,
   whether in an action of contract, tort, or otherwise, arising
   from, out of or in connection with the software or the use of
   other dealings in the software.
*/

using System.Net;
using Engine.Data;
using Engine.Diagnostics;
using Engine.Net;
using Engine.Net.Messaging;
using Fallen.Common;
using AuthPacketHandler = Engine.Net.Messaging.PacketHandler<Engine.Net.NetworkClient, Engine.Net.Messaging.NetMessage, Fallen.Common.AuthMessageAttribute>;
using WorldPacketHandler = Engine.Net.Messaging.PacketHandler<Engine.Net.NetworkClient, Engine.Net.Messaging.NetMessage, Fallen.Common.WorldMessageAttribute>;

namespace Client.Data.Managers
{
    public sealed class NetworkManager : ManagerService
    {
        #region Properties
        public NetworkClient AuthClient { get; private set; }
        public NetworkClient WorldClient { get; private set; }


        public WorldPacketHandler WorldPacketHandler { get; private set; }
        public AuthPacketHandler AuthPacketHandler { get; private set; }
        #endregion


        public override void Initialize()
        {
            this.WorldPacketHandler = new WorldPacketHandler();
            this.AuthPacketHandler = new AuthPacketHandler();

            this.WorldPacketHandler.Register(typeof(NetworkManager).Assembly);
            this.AuthPacketHandler.Register(typeof(NetworkManager).Assembly);

            this.AuthClient = this.CreateClient();
            this.WorldClient = this.CreateClient();

            ServiceProvider.Instance.AddService<NetworkManager>(this);
        }

        #region Public Methods
        public void HookAuthHandler(MessageEventHandler<NetworkClient, NetMessage> handler)
        {
            this.AuthPacketHandler.Register(handler);
        }

        public void HookWorldHandler(MessageEventHandler<NetworkClient, NetMessage> handler)
        {
            this.WorldPacketHandler.Register(handler);
        }

        public void DisconnectClients()
        {
            this.AuthClient?.Disconnect();
            this.WorldClient?.Disconnect();
        }

        public void ConnectToWorldServer(IPAddress address)
        {
            if (this.WorldClient == null) { this.WorldClient = this.CreateClient(); }

            this.WorldClient.Connect(address, PortInfos.WorldPort);
        }

        public void ConnectToAuthService(IPAddress address)
        {
            if (this.AuthClient == null) { this.AuthClient = this.CreateClient(); }

            this.AuthClient.Connect(address, PortInfos.AuthPort);
        }
        #endregion

        #region Private & Protected
        private NetworkClient CreateClient()
        {
            var client = new NetworkClient();
            client.ConnectionEstablished += this.Client_ConnectionEstablished;
            client.DataReceived += this.Client_DataReceived;

            return client;
        }

        protected override void Dispose(bool disposing)
        {
            this.AuthClient?.Disconnect();
            this.WorldClient?.Disconnect();

            this.WorldPacketHandler?.Unregister();
            this.AuthPacketHandler?.Unregister();


            this.WorldClient = null;
            this.AuthClient = null;

            this.WorldPacketHandler = null;
            this.AuthPacketHandler = null;

            base.Dispose(disposing);
        }
        #endregion

        #region Event Handlers
        private void Client_DataReceived(object sender, System.Net.Sockets.SocketAsyncEventArgs e)
        {
            if (sender is NetworkClient netClient)
            {
                var buffer = netClient.AESCipher.Decrypt(e.Buffer);
                var netMsg = new NetMessage(buffer);

                if (netMsg.PacketType == PacketType.AuthPacket)
                {
                    if (!this.AuthPacketHandler.HandlePacket(netClient, netMsg))
                    {
                        LogSystem.Instance.Error(
                            $"Failed to handle AuthPacket with id {(AuthCodes)netMsg.PacketId}",
                            typeof(NetworkManager));
                    }
                }
                else if (netMsg.PacketType == PacketType.GamePacket)
                {
                    if (!this.WorldPacketHandler.HandlePacket(netClient, netMsg))
                    {
                        LogSystem.Instance.Error(
                            $"Failed to handle WorldPacket with id {(GameOpCodes)netMsg.PacketId}",
                            typeof(NetworkManager));
                    }
                }
            }
        }


        private void Client_ConnectionEstablished(object sender, System.EventArgs e)
        {
            if (sender is NetworkClient netClient)
            {
                netClient.BeginReceive();
            }
        }

        #endregion
    }
}
