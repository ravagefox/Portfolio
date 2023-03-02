// Source: DeleteCharacterPacket
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

using Engine.Net;
using Fallen.Common;

namespace Client.Data.Net.Packets.Character
{
    public sealed class DeleteCharacterPacket : PacketBase
    {
        public int CharacterId { get; set; }


        public DeleteCharacterPacket(NetworkClient client) :
            base(GameOpCodes.CMSG_CHAR_DELETE, client)
        {
        }


        protected override void WritePacket()
        {
            this.Write(this.CharacterId);
        }
    }
}
