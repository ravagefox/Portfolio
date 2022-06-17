// Source: WorldScene
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

using System.Linq;
using Engine.Core;
using Engine.Data;
using Engine.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Wargame.Data.Graphics;
using Wargame.Data.Graphics.DeferredContext;
using Wargame.Data.IO;
using Wargame.Data.Scenes.StateManagers;

namespace Wargame.Data.Scenes
{
    public class WorldScene : Scene
    {
        public GraphicsDevice GraphicsDevice =>
            ServiceProvider.Instance.GetService<IGraphicsDeviceService>()
            .GraphicsDevice;

        public RenderManager RenderMaster { get; }

        public EditorRenderSystem EditSystem { get; }

        public GameStateManager StateManager { get; }

        private SpriteBatch spriteBatch;
        private Point resolution;
        private WorldState worldState;




        public WorldScene()
        {
            this.resolution = new Point(2560, 1440);

            this.spriteBatch = new SpriteBatch(this.GraphicsDevice);
            this.RenderMaster = new RenderManager(this.GraphicsDevice);
            this.EditSystem = new EditorRenderSystem(this.GraphicsDevice);

            this.worldState = new WorldState(this.RenderMaster);
            this.StateManager = new GameStateManager(this.worldState);
        }

        protected override void Load()
        {
            this.worldState.StartLevel = "Data\\Maps\\testmap.mzx";

            this.EditSystem.Resolution = this.resolution;
            this.EditSystem.InitializeSystem();

            this.RenderMaster.Resolution = this.resolution;
            this.RenderMaster.Initialize();

            this.EditSystem.Actors = this.worldState.GameObjects
                .Concat(this.RenderMaster.LightSystem.GetLights());
            this.StateManager.RestoreToDefault();
            base.Load();
        }

        protected override void Unload()
        {
            this.RenderMaster?.Dispose();
            this.EditSystem?.Dispose();
            base.Unload();
        }

        protected override void Update(Time frameTime)
        {
            this.StateManager.CurrentState?.Update(frameTime);
            base.Update(frameTime);
        }

        protected override void Render(Time frameTime)
        {
            this.GraphicsDevice.Clear(
                new Color(0.0f, 0.2f, 0.4f, 1.0f));

            this.StateManager.CurrentState?.Apply(frameTime);


            this.spriteBatch.Begin();
            this.spriteBatch.Draw(
                this.RenderMaster.FullScreenTexture,
                this.RenderMaster.ScreenRect,
                Color.White);
            this.spriteBatch.Draw(
                this.EditSystem.EditTexture,
                this.RenderMaster.ScreenRect,
                Color.White);
            this.spriteBatch.End();

            this.spriteBatch.Begin();
            this.VisualizeRenderTargets(new Texture2D[]
            {
                this.RenderMaster.DeferredSystem.GetTexture(DeferredRenderTarget.Albedo),
                this.RenderMaster.DeferredSystem.GetTexture(DeferredRenderTarget.Normal),
                this.RenderMaster.DeferredSystem.GetTexture(DeferredRenderTarget.Depth),

                this.RenderMaster.ShadowSystem.DepthTexture,
                this.RenderMaster.ShadowSystem.SceneTexture,

                this.RenderMaster.LightSystem.LightTexture,
                this.RenderMaster.SSAOSystem.SsaoTexture,
            });
            this.spriteBatch.End();
            base.Render(frameTime);
        }

        private void VisualizeRenderTargets(Texture2D[] texture2Ds)
        {
            if (DebugDraw.DebugMode)
            {
                var size = 100;
                var rect = new Rectangle(0, 0, size, size);

                for (var i = 0; i < texture2Ds.Length; i++)
                {
                    rect.X = (i * size) + 1;
                    this.spriteBatch.Draw(texture2Ds[i], rect, Color.White);

                    this.spriteBatch.DrawRectangle(rect, Color.Red);
                }
            }
        }
    }
}
