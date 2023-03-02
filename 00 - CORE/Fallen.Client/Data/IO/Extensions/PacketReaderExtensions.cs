// Source: PacketReaderExtensions
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

using Engine.Net.Messaging;
using Microsoft.Xna.Framework;

namespace Client.Data.IO.Extensions
{
    public static class PacketReaderExtensions
    {
        public static Vector3 ReadVector3(this PacketReader r)
        {
            return new Vector3(
                r.ReadSingle(),
                r.ReadSingle(),
                r.ReadSingle());
        }
    }
}
