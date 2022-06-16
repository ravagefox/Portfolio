// Source: DebugDrawExtensions
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

using Engine.Graphics;
using Microsoft.Xna.Framework;
using Wargame.Data.Math;

namespace Wargame.Extensions
{
    internal static class DebugDrawExtensions
    {
        public static void DrawWireBox(
            this DebugDraw draw,
            BoundingOrientedBox box,
            Color color)
        {
            var cubeIndices = new ushort[] { 0, 1, 1, 2, 2, 3, 3, 0, 4, 5, 5, 6, 6, 7, 7, 4, 0, 4, 1, 5, 2, 6, 3, 7 };
            draw.DrawWireShape(box.GetCorners(), cubeIndices, color);
        }
    }
}
