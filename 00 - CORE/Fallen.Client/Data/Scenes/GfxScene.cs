// Source: GfxScene
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

using Client.Data.Managers;
using Engine.Core;
using Engine.Data;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Client.Data.Scenes
{
    public abstract class GfxScene : Scene
    {
        #region Properties

        public GraphicsDevice GraphicsDevice =>
            ServiceProvider.Instance.GetService<IGraphicsDeviceService>()
            .GraphicsDevice;

        public AssetManager Content =>
            ServiceProvider.Instance.GetService<AssetManager>();


        protected Color ClearColor = new Color(0.0f, 0.2f, 0.4f, 1.0f);
        #endregion

    }
}
