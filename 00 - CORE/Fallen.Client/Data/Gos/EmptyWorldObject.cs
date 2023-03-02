// Source: EmptyWorldObject
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

namespace Client.Data.Gos
{
    internal class EmptyWorldObject : WorldObject
    {
        public override WorldObjectType ObjectType => WorldObjectType.None;


        public EmptyWorldObject() : base(uint.MaxValue)
        {
        }

    }
}
