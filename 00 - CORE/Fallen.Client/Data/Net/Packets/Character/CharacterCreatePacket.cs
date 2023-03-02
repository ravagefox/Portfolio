// Source: CharacterCreatePacket
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

using Fallen.Common;
using Fallen.Common.Dbx;

namespace Client.Data.Net.Packets.Character
{
    internal class CharacterCreatePacket : UserPacket
    {
        public CharacterInfo CharacterInfo { get; set; }


        public CharacterCreatePacket()
            : base(GameOpCodes.CMSG_CHAR_CREATE)
        {
            this.Enqueue(() =>
            {
                var blob = this.CharacterInfo.ToArray();
                this.Write(blob.Length);
                this.Write(blob);
            });
        }
    }
}
