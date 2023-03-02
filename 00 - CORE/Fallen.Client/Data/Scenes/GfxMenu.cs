// Source: GfxMenu
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
using Engine.Graphics.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Client.Data.Scenes
{
    public sealed class GfxMenu : GfxScene
    {

        public UserInterface UserInterface { get; set; }

        private AddonManager AddonManager =>
            ServiceProvider.Instance.GetService<AddonManager>();


        public GfxMenu()
        {
        }


        protected override void Load()
        {
            this.AddonManager.LoadInterface("Interface\\GlueXML\\AccountLogin.xml");

            base.Load();
        }

        protected override void Unload()
        {
            this.UserInterface?.Unload();
            this.UserInterface = null;

            base.Unload();
        }

        protected override void Update(Time frameTime)
        {
            this.UserInterface?.Update(frameTime);
            base.Update(frameTime);
        }

        protected override void Render(Time frameTime)
        {
            this.GraphicsDevice.Clear(this.ClearColor);

            this.TryRenderUserInterface(frameTime);
            base.Render(frameTime);
        }

        private void TryRenderUserInterface(Time frameTime)
        {
            try
            {
                if (this.UserInterface != null)
                {
                    this.UserInterface?.Apply(frameTime);

                    var gd = this.UserInterface.Graphics.SpriteBatch;
                    var bounds = this.UserInterface.Graphics.GetScreenRect();

                    gd.Begin(SpriteSortMode.Immediate);
                    gd.Draw(this.UserInterface.Graphics.GetRenderTarget(), bounds, Color.White);
                    gd.End();
                }
            }
            catch
            {
                // Do nothing, as this may crash the entire scene and/or game.
            }
        }
    }
}
