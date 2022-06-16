// Source: Vector4Extensions
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
using Microsoft.Xna.Framework;

namespace Wargame.Extensions
{
    internal static class Vector4Extensions
    {
        public static Vector3 ToVector3(this Vector4 v)
        {
            return new Vector3(v.X, v.Y, v.Z) / v.W;
        }

        public static Vector4 Round(this Vector4 v)
        {
            return new Vector4(
                (float)Math.Round(v.X),
                (float)Math.Round(v.Y),
                (float)Math.Round(v.Z),
                (float)Math.Round(v.W));
        }
    }
}
