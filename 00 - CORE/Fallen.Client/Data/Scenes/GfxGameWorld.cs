// Source: GfxGameWorld
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
using System.Linq;
using Client.Data.Gos;
using Client.Data.Gos.Components;
using Client.Data.Graphics.DeferredContext;
using Client.Data.Managers;
using Client.Data.Managers.GameManagers;
using Engine.Core;
using Engine.Graphics.Linq;
using Engine.Graphics.UI;
using Fallen.Common;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Client.Data.Scenes
{
    public sealed class GfxGameWorld : GfxScene
    {
        public MapId MapId { get; internal set; }

        public IEnumerable<UserInterface> AddOns { get; set; }

        public GameObjectManager ObjectManager { get; private set; }


        private RenderManager m_renderManager;
        private Skybox m_skybox;
        private ICamera m_playerCamera;

        private SpriteBatch m_spriteBatch;

        public GfxGameWorld()
        {
            this.ObjectManager = new GameObjectManager();
            this.ObjectManager.Initialize();

            this.AddOns = Enumerable.Empty<UserInterface>();
        }


        protected override void Load()
        {
            this.m_spriteBatch = new SpriteBatch(this.GraphicsDevice);
            this.m_renderManager = new RenderManager();

            var playerKeyInput = new KeyboardComponent();
            this.ObjectManager.Player.AddComponent(playerKeyInput);
            this.ObjectManager.Player.Initialize();

            base.Load();
        }

        protected override void Unload()
        {
            this.ObjectManager?.Dispose();

            base.Unload();
        }

        protected override void Render(Time frameTime)
        {
            this.GraphicsDevice.Clear(this.ClearColor);
            this.m_skybox.RenderSkybox(frameTime);

            this.m_renderManager.RenderToTexture(frameTime);

            var texture = this.m_renderManager.DeferredSystem.GetRenderTarget2D(DeferredState.Albedo);

            this.m_spriteBatch.Begin();
            this.m_spriteBatch.Draw(texture, this.GraphicsDevice.GetBounds(), Color.White);
            this.m_spriteBatch.End();


            base.Render(frameTime);
        }

        protected override void Update(Time frameTime)
        {
            this.ObjectManager?.Player.Update(frameTime);
            this.ObjectManager?.GameObjects.ToList().ForEach(c =>
            {
                if (c.Enabled)
                {
                    c.Update(frameTime);
                }
            });

            base.Update(frameTime);
        }
    }
}
