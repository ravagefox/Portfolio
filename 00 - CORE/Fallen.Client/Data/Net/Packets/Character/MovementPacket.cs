// Source: MovementPacket
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
using Fallen.Common;

namespace Client.Data.Net.Packets.Character
{
    public sealed class MovementPacket : PacketBase
    {
        public uint PlayerId { get; }

        public ObjUpdateFlags Flags { get; set; } = ObjUpdateFlags.Movement;

        public MovementFlags MovementFlags { get; set; }


        public MovementPacket(uint playerId) :
            base(GameOpCodes.CMSG_CHAR_UPDATE, C_Session.Instance.WorldClient)
        {
            this.PlayerId = playerId;
        }



        protected override void WritePacket()
        {
            this.Write(this.PlayerId);
            this.Write((uint)this.Flags);

            this.Write((uint)this.MovementFlags);
        }
    }
}
