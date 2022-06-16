// Source: Vector2Extensions
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
    public static class Vector2Extensions
    {
        public static Vector2 MoveInFigure8(this Vector2 _, float time)
        {
            var scale = 2 / (3 - (float)Math.Cos(2 * time));

            return new Vector2(scale * (float)Math.Cos(time), scale * (float)Math.Sin(2 * time) / 2);
        }
    }
}
