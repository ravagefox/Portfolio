// Source: UserPacket
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
using System.Collections.Generic;
using Engine.Net;
using Engine.Net.Messaging;
using Fallen.Common;

namespace Client.Data.Net
{
    internal class UserPacket : PacketWriter
    {

        private List<Action> userActions = new List<Action>();
        private static ulong nonce = 0UL;


        public UserPacket(AuthCodes code) : base((uint)code, PacketType.AuthPacket)
        {
        }

        public UserPacket(GameOpCodes code) : base((uint)code, PacketType.GamePacket)
        {
        }

        public void Enqueue(Action userAction)
        {
            this.userActions.Add(userAction);
        }

        protected virtual void WriteData()
        {
            if (this.userActions.Count > 0)
            {
                this.userActions.ForEach(a => a.Invoke());
                this.userActions.Clear();
            }
        }

        public void Send(NetworkClient client)
        {
            nonce++;

            this.Write(nonce);
            this.WriteData();

            var data = this.ToArray();
            data = client.AESCipher.Encrypt(data);
            client.Send(data, 0, data.Length);
        }
    }
}
