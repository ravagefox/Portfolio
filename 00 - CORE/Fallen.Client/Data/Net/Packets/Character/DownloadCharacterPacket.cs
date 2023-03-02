// Source: DownloadCharacterPacket
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
    public sealed class DownloadCharacterPacket : PacketBase
    {
        public string Owner { get; set; }

        public uint CharacterId { get; set; }

        public bool WorldCharacterRetrieval { get; set; }




        public DownloadCharacterPacket(NetworkClient client)
            : base(GameOpCodes.CMSG_CHAR_ENUM, client)
        {
        }


        protected override void WritePacket()
        {
            this.Write(this.WorldCharacterRetrieval);
            this.Write(this.Owner);

            if (this.WorldCharacterRetrieval)
            {
                this.Write(this.CharacterId);
            }
        }
    }
}
