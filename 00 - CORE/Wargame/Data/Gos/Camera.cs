// Source: Camera
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

using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Wargame.Data.Gos
{
    public class Camera : WorldObject
    {
        public static Camera Current { get; private set; }

        public static IEnumerable<Camera> AllCameras => allCameras;

        public Matrix View { get; set; }

        public Matrix Projection { get; set; }


        private static readonly HashSet<Camera> allCameras = new HashSet<Camera>();


        public Camera() : base()
        {
            allCameras.Add(this);
        }

        public void SetCurrent()
        {
            if (Current == null)
            {
            }
            else
            {
                // TODO: implement 'blending' between old and new camera.
                // Achieve this through the use of scripted camera movement.
            }

            Current = this;
        }

        public BoundingFrustum GetBoundingFrustum()
        {
            return new BoundingFrustum(this.View * this.Projection);
        }
    }
}
