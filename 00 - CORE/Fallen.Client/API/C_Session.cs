// Source: C_Session
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
using System.IO;
using System.Linq;
using System.Net.Sockets;
using Engine.Diagnostics;
using Engine.Net;
using Engine.Net.Messaging;
using Fallen.Common;

namespace Client.API
{
    public partial class C_Session : SessionInfo
    {
        // -----------------------------------------------
        // Universal variables
        // -----------------------------------------------
        #region Fields and Properties

        public static C_Session Instance { get; private set; }

        public NetworkClient AuthClient { get; set; }
        public NetworkClient WorldClient { get; set; }

        private PacketHandler<NetworkClient, NetMessage, AuthMessageAttribute> AuthPacketHandler { get; }
        private PacketHandler<NetworkClient, NetMessage, WorldMessageAttribute> WorldPacketHandler { get; }

        public string Checksum { get; private set; }

        #endregion

        #region Constructors

        public C_Session()
        {
            Instance = this;

            this.AuthClient = this.CreateClient();
            this.WorldClient = this.CreateClient();

            this.AuthPacketHandler = new PacketHandler<NetworkClient, NetMessage, AuthMessageAttribute>(typeof(C_Session).Assembly);
            this.WorldPacketHandler = new PacketHandler<NetworkClient, NetMessage, WorldMessageAttribute>(typeof(C_Session).Assembly);

            bool checkAuth(NetworkClient client) { return true; }

            this.AuthPacketHandler.CheckAuthentication += checkAuth;
            this.WorldPacketHandler.CheckAuthentication += checkAuth;

        }

        public NetworkClient CreateClient()
        {
            var client = new NetworkClient();
            client.DataReceived += this.OnDataReceived;
            client.ConnectionEstablished += this.OnClientConnected;

            return client;
        }

        public void DisconnectFromServer()
        {
            void DC(NetworkClient client)
            {
                client.Disconnect();
            };

            DC(this.AuthClient);
            DC(this.WorldClient);
        }

        private void OnClientConnected(object sender, EventArgs e)
        {
            if (sender is NetworkClient client)
            {
                client.BeginReceive();
            }
        }

        public void SetChecksum(string checksum)
        {
            this.Checksum = checksum;
        }

        private void OnDataReceived(object sender, SocketAsyncEventArgs e)
        {
            var data = ((NetworkClient)sender).AESCipher.Decrypt(e.Buffer);

            var msg = new NetMessage(data);
            if (!msg.Succeeded && msg.PacketType != PacketType.PingPacket)
            {
                LogSystem.Instance.Error(msg.ErrorMessage, typeof(C_Session));
            }

            var handled = false;
            switch (msg.PacketReader.PacketType)
            {
                case PacketType.AuthPacket:
                    handled = this.AuthPacketHandler.HandlePacket(sender as NetworkClient, msg);
                    break;

                case PacketType.GamePacket:
                    handled = this.WorldPacketHandler.HandlePacket(sender as NetworkClient, msg);
                    break;

                case PacketType.PingPacket:
                    {
                        handled = true;
                        // Heartbeat.GetHeartbeat(sender as NetworkClient).SendHeartbeat(true);
                    }
                    break;
            }

            if (!handled && msg.PacketType != PacketType.PingPacket)
            {
                LogSystem.Instance.Error(
                    $"Failed to handle {msg.PacketType} packet with id {msg.PacketId}",
                    typeof(C_Session));
            }
        }

        private bool IsEncrypted(byte[] data)
        {
            var target = new char[] { 'W', 'X', 'P' };
            using (var r = new BinaryReader(new MemoryStream(data)))
            {
                if (r.ReadChars(3).SequenceEqual(target)) { return false; }
            }

            return true;
        }

        #endregion

        // -----------------------------------------------
        // Client message handlers
        // -----------------------------------------------
        #region Handler Messages

        #endregion

        // -----------------------------------------------
        // Public user methods
        // -----------------------------------------------
        #region Public API

        #endregion
    }
}
