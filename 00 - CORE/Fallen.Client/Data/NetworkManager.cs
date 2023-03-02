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

using System;
using System.Net;
using System.Net.Sockets;
using Engine.Core.Threading;
using Engine.Diagnostics;
using Engine.Net;
using Engine.Net.Messaging;
using Fallen.Common;
using AuthPacketHandler = Engine.Net.Messaging.PacketHandler<Engine.Net.NetworkClient, Engine.Net.Messaging.NetMessage, Fallen.Common.AuthMessageAttribute>;
using WorldPacketHandler = Engine.Net.Messaging.PacketHandler<Engine.Net.NetworkClient, Engine.Net.Messaging.NetMessage, Fallen.Common.WorldMessageAttribute>;

namespace Client.Data
{
    public sealed class NetworkManager : IDisposable
    {
        public NetworkClient AuthClient { get; private set; }
        public NetworkClient WorldClient { get; private set; }


        public event EventHandler ClientDisconnected;


        private AuthPacketHandler authPacketHandler;
        private WorldPacketHandler worldPacketHandler;


        public NetworkManager()
        {
            this.AuthClient = this.CreateClient();
            this.WorldClient = this.CreateClient();

            this.authPacketHandler = new AuthPacketHandler();
            this.worldPacketHandler = new WorldPacketHandler();

            this.authPacketHandler.Register(typeof(NetworkManager).Assembly);
            this.worldPacketHandler.Register(typeof(NetworkManager).Assembly);
        }

        public void ConnectToAuthServer(IPAddress serverAddress)
        {
            this.AuthClient.Connect(serverAddress, PortInfos.AuthPort);
        }

        public void ConnectToWorldServer(IPAddress serverAddress)
        {
            this.WorldClient.Connect(serverAddress, PortInfos.WorldPort);
        }

        private NetworkClient CreateClient()
        {
            var client = new NetworkClient();
            client.DataReceived += this.OnDataReceive;
            client.ConnectionEstablished += this.OnConnected;

            return client;
        }

        private void OnConnected(object sender, EventArgs e)
        {
            if (sender is NetworkClient client)
            {
                client.BeginReceive();

                var connectionPoll = new PollEvent(100);
                connectionPoll.OnPoll += (s, x) =>
                {
                    if (client.TcpSocket != null)
                    {
                        var part1 = client.TcpSocket.Poll(1000, SelectMode.SelectRead);
                        var part2 = client.TcpSocket.Available == 0;

                        if (part1 && part2)
                        {
                            LogSystem.Instance.Error(
                                $"Disconnected from server {((IPEndPoint)client.TcpSocket.RemoteEndPoint).Address}",
                                typeof(NetworkManager));

                            client.Disconnect();
                            connectionPoll.Cancel();

                            ClientDisconnected?.Invoke(this, EventArgs.Empty);
                        }
                    }
                };
            }
        }

        private void OnDataReceive(object sender, SocketAsyncEventArgs e)
        {
            if (sender is NetworkClient client)
            {
                var decryptedData = client.AESCipher.Decrypt(e.Buffer);
                var netMessage = new NetMessage(decryptedData);

                if (netMessage.PacketType == PacketType.AuthPacket)
                {
                    if (!this.authPacketHandler.HandlePacket(client, netMessage))
                    {
                        LogSystem.Instance.Error(
                            $"Failed to handle packet {(AuthCodes)netMessage.PacketId}",
                            typeof(NetworkManager));
                    }
                }
                else if (netMessage.PacketType == PacketType.GamePacket)
                {
                    if (!this.worldPacketHandler.HandlePacket(client, netMessage))
                    {
                        LogSystem.Instance.Error(
                            $"Failed to handle packet {(GameOpCodes)netMessage.PacketId}",
                            typeof(NetworkManager));
                    }
                }
            }
        }

        public void DisconnectClients()
        {
            this.WorldClient.AESCipher = null;
            this.AuthClient.AESCipher = null;

            this.AuthClient.RSACipher = null;
            this.WorldClient.RSACipher = null;

            this.WorldClient?.Disconnect();
            this.AuthClient?.Disconnect();
        }

        public void Dispose()
        {
            this.authPacketHandler?.Unregister();
            this.worldPacketHandler?.Unregister();

            this.AuthClient?.Disconnect();
            this.WorldClient?.Disconnect();

            this.AuthClient.ConnectionEstablished -= this.OnConnected;
            this.WorldClient.DataReceived -= this.OnDataReceive;

            this.AuthClient = null;
            this.WorldClient = null;
            this.worldPacketHandler = null;
            this.authPacketHandler = null;
        }
    }
}
