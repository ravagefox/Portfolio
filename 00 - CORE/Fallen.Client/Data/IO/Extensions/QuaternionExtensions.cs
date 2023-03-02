// Source: QuaternionExtensions
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

using Microsoft.Xna.Framework;

namespace Client.Data.IO.Extensions
{
    public static class QuaternionExtensions
    {
        public static Vector3 GetEulerAngles(this Quaternion q1)
        {
            var quat = new Engine.Math.Quaternion(q1.X, q1.Y, q1.Z, q1.W);
            var euler = quat.EulerAngles;

            return new Vector3(euler.X, euler.Y, euler.Z);
        }
    }
}
