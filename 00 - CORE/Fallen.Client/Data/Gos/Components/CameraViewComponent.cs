// Source: CameraViewComponent
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

using Engine.Core;
using Engine.Data;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Client.Data.Gos.Components
{
    public class CameraViewComponent : GameObjectComponent
    {
        public Matrix View { get; set; }

        public Matrix Projection { get; set; }

        public float ViewDistance { get; set; }

        public float Fov { get; set; }

        public float AspectRatio
        {
            get
            {
                var g = ServiceProvider.Instance.GetService<IGraphicsDeviceService>().GraphicsDevice;

                return (float)g.PresentationParameters.BackBufferWidth / g.PresentationParameters.BackBufferHeight;
            }
        }



        public override void Initialize()
        {
        }

        public override void Update(Time gameTime)
        {
        }
    }
}
