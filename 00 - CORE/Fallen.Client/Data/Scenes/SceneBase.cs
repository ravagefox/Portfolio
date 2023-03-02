// Source: SceneBase
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

using Client.Data.IO;
using Engine.Core;
using Engine.Data;
using Engine.Graphics.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Client.Data.Scenes
{
    public abstract class SceneBase : Scene
    {
        public GraphicsDevice GraphicsDevice =>
            ServiceProvider.Instance.GetService<IGraphicsDeviceService>().GraphicsDevice;

        public AssetManager Content =>
            ServiceProvider.Instance.GetService<AssetManager>();

        public UserInterface Dialog { get; set; }


        public static readonly Color ClearColor = new Color(0.0f, 0.2f, 0.4f, 1.0f);



        public SceneBase()
        {
        }

        protected override void Load()
        {
            base.Load();
        }

        protected override void Unload()
        {
            base.Unload();
        }

        protected override void Update(Time frameTime)
        {
            this.Dialog?.Update(frameTime);
            base.Update(frameTime);
        }

        protected override void Render(Time frameTime)
        {
            this.Dialog?.Apply(frameTime);
            base.Render(frameTime);
        }
    }
}
