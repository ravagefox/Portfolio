// Source: SsaoRenderSystem
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

using System;
using Engine.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Wargame.Data.Gos;
using Wargame.Data.Graphics.Shaders;

namespace Wargame.Data.Graphics.DeferredContext
{
    public sealed class SsaoRenderSystem : RenderSystemBase
    {
        public DeferredRenderSystem RenderSystem { get; }

        public Texture2D SsaoTexture => this.ssaoTarget;

        public bool Enable { get; set; }


        private RenderTarget2D ssaoTarget;
        private SsaoEffect ssaoEffect;
        private ScreenQuad screenQuad;


        public SsaoRenderSystem(DeferredRenderSystem renderSystem)
            : base(renderSystem.GraphicsDevice)
        {
            this.RenderSystem = renderSystem;
        }

        protected override void OnInitialize(object sender, EventArgs e)
        {
            this.ssaoEffect = new SsaoEffect(this.RenderSystem);
            this.screenQuad = new ScreenQuad();

            base.OnInitialize(sender, e);
        }

        protected override void OnResolutionChanged(object sender, EventArgs e)
        {
            this.RecreateRenderTarget2D(ref this.ssaoTarget);
            base.OnResolutionChanged(sender, e);
        }


        public override void Begin()
        {
            this.GraphicsDevice.SetRenderTarget(this.ssaoTarget);
            this.GraphicsDevice.Clear(Color.White);
        }

        public void RenderSSAO(Camera camera)
        {
            if (!this.Enable) { return; }

            this.ssaoEffect.View = camera.View;
            this.ssaoEffect.Projection = camera.Projection;

            this.ssaoEffect.Apply(this.ssaoEffect.GetCurrentTechnique());
            this.screenQuad.Render();
        }

        public override void End()
        {
            this.GraphicsDevice.SetRenderTarget(null);
        }

        protected override void OnDispose()
        {
            this.ssaoEffect?.Dispose();
            this.ssaoTarget?.Dispose();
        }
    }
}
