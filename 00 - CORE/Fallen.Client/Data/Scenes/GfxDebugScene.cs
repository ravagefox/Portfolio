// Source: GfxDebugScene
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

using System.Windows.Forms;
using Client.Data.Gos;
using Client.Data.Gos.Components;
using Client.Data.Graphics.DeferredContext;
using Client.Data.Managers;
using Engine.Core;
using Engine.Data;
using Engine.Graphics.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Client.Data.Scenes
{
#if DEBUG
    internal class GfxDebugScene : GfxScene
    {
        private ICamera m_freeCamera;
        private RenderManager m_renderManager;
        private DynamicWorldObject m_cubeObj;
        private Skybox m_skybox;

        private SpriteBatch m_spriteBatch;

        public GfxDebugScene()
        {
        }

        protected override void Load()
        {
            this.m_spriteBatch = new SpriteBatch(this.GraphicsDevice);

            this.m_renderManager = ServiceProvider.Instance.GetService<RenderManager>();
            this.m_renderManager.Resolution = new Point(1280, 720);

            this.m_freeCamera = new DebugCamera();
            this.m_skybox = new Skybox();
            this.m_skybox.Camera = this.m_freeCamera;
            this.m_skybox.Transform.Scale = 50.0f;

            this.m_cubeObj = new DynamicWorldObject(uint.MaxValue);
            var model = new ModelRenderer();
            model.VisibleChanged += (s, e) =>
            {
                if (model.Visible)
                {
                    this.m_renderManager.DeferredSystem.ObjectsToRender.Add(this.m_cubeObj);
                }
                else
                {
                    this.m_renderManager.DeferredSystem.ObjectsToRender.Remove(this.m_cubeObj);
                }
            };
            model.Visible = true;
            this.m_cubeObj.Enabled = true;

            model.AssetPath = "Doodads\\Box.m2x";
            this.m_cubeObj.AddComponent(model);

            this.m_renderManager.DeferredSystem.ObjectsToRender.Add(this.m_cubeObj);

            this.m_renderManager.Camera = this.m_freeCamera;
            this.Actors.Add((DebugCamera)this.m_freeCamera);
            this.Actors.Add(this.m_cubeObj);
            this.Actors.Add(this.m_skybox);

            base.Load();
        }

        protected override void Unload()
        {
            base.Unload();
        }

        protected override void Update(Time frameTime)
        {
            var form = Form.ActiveForm;
            if (form != null)
            {
                form.Text = "FPS: " + frameTime.Fps.ToString();
            }

            base.Update(frameTime);
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
    }
#endif
}
