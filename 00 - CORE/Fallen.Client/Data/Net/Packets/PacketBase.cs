// Source: PacketBase
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

using Client.API;
using Engine.Net;
using Engine.Net.Messaging;
using Fallen.Common;

namespace Client.Data.Net.Packets
{
    public abstract class PacketBase : PacketWriter
    {
        public virtual bool RequireAuthentication { get; }

        public NetworkClient Client { get; }


        private static ulong nonceValue;
        private bool hasSent;


        public PacketBase(AuthCodes code, NetworkClient client) : base((uint)code, PacketType.AuthPacket)
        {
            this.RequireAuthentication = false;
            this.Client = client;
            this.hasSent = false;
        }

        public PacketBase(GameOpCodes code, NetworkClient client) : base((uint)code, PacketType.GamePacket)
        {
            this.RequireAuthentication = true;
            this.Client = client;
            this.hasSent = false;
        }


        protected abstract void WritePacket();


        public void SendPacket()
        {
            if (!this.hasSent)
            {
                nonceValue++;
                this.hasSent = true;

                this.Write(nonceValue);
                this.WritePacket();
                var data = this.ToArray();

                data = this.Client.AESCipher.Encrypt(data);
                this.Client.Send(data, 0, data.Length);
            }
        }
    }
}
