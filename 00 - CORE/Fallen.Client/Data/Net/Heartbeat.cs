// Source: Heartbeat
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

using System.Collections.Generic;
using Engine.Net;
using Engine.Net.Messaging;
using Engine.Net.Security;
using Fallen.Common;

namespace Client.Data.Net
{
    public class Heartbeat : HeartbeatBase
    {
        private static readonly Dictionary<NetworkClient, Heartbeat> _heartbeats = new Dictionary<NetworkClient, Heartbeat>();


        public Heartbeat(NetworkClient client) : base(client)
        {
            _heartbeats[client] = this;
        }

        public static Heartbeat GetHeartbeat(NetworkClient client)
        {
            if (!_heartbeats.TryGetValue(client, out var heartbeat))
            {
                heartbeat = new Heartbeat(client);
            }

            return heartbeat;
        }

        protected override PacketWriter GetPingPacket()
        {
            return new PacketWriter((uint)AuthCodes.CMSG_HEARTBEAT, PacketType.PingPacket);
        }
    }
}
