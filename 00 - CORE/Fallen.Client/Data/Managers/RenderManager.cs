// Source: RenderManager
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
using Client.Data.Gos;
using Client.Data.Graphics.DeferredContext;
using Client.Data.Graphics.Shaders;
using Engine.Core;
using Engine.Data;
using Engine.Graphics;
using Engine.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Client.Data.Managers
{
    public class RenderManager : ManagerService
    {
        #region Nested Types
        [Flags]
        public enum PostProcess : uint
        {
            None = 0,
            SSAO = 1,
            FXAA = 2,
            ColorGrading = 3,
        }

        #endregion

        #region Properties

        public Texture2D FullscreenTexture => this.m_screenTarget;

        public Point Resolution
        {
            get => this.m_resolution;
            set
            {
                if (this.m_resolution != value)
                {
                    this.m_resolution = value;
                    this.DeferredSystem.Resolution = value;

                    this.OnResolutionChanged(this, EventArgs.Empty);
                }
            }
        }

        public GraphicsDevice GraphicsDevice =>
            ServiceProvider.Instance.GetService<IGraphicsDeviceService>()
            .GraphicsDevice;

        public ICamera Camera { get; set; }

        public DeferredRenderSystem DeferredSystem { get; private set; }

        #endregion


        #region Fields

        public event EventHandler ResolutionChanged;

        private RenderTarget2D m_screenTarget;
        private Point m_resolution;
        private ScreenQuad m_fullScreenQuad;
        private FinalCombineEffect m_combineEffect;
        private uint m_postProcessFlags;
        #endregion


        public RenderManager() : base()
        {
            ServiceProvider.Instance.AddService<RenderManager>(this);
        }

        #region Public Methods
        public void RenderToTexture(Time frameTime)
        {
            if (this.Camera == null) { return; }

            this.DeferredSystem.Camera = this.Camera;
            this.DeferredSystem.Begin();
            this.DeferredSystem.RenderToGBuffer(frameTime);
            this.DeferredSystem.End();

        }

        public void TogglePostProcess(PostProcess flags)
        {
            if (this.m_postProcessFlags.HasFlag((uint)flags))
            {
                this.m_postProcessFlags &= ~(uint)flags;
            }
            else { this.m_postProcessFlags |= (uint)flags; }
        }
        #endregion

        #region Public Overrides
        public override void Initialize()
        {
            this.DeferredSystem = new DeferredRenderSystem(this.GraphicsDevice);
            this.m_combineEffect = new FinalCombineEffect();
            this.m_postProcessFlags = 0u;

            this.TogglePostProcess(PostProcess.SSAO);
            this.TogglePostProcess(PostProcess.ColorGrading);
            this.TogglePostProcess(PostProcess.FXAA);

            this.DeferredSystem.Initialize();
        }

        #endregion

        #region Virtual Methods
        protected virtual void OnResolutionChanged(object sender, EventArgs e)
        {
            this.Recreate(ref this.m_screenTarget);
            ResolutionChanged?.Invoke(sender, e);
        }
        #endregion

        #region Protected Overrides
        protected override void Dispose(bool disposing)
        {
            this.m_postProcessFlags = 0;

            this.m_screenTarget?.Dispose();
            this.DeferredSystem?.Dispose();
            this.m_combineEffect?.Dispose();
            this.m_fullScreenQuad?.Dispose();

            this.m_screenTarget = null;
            this.DeferredSystem = null;
            this.m_combineEffect = null;
            this.m_fullScreenQuad = null;
            this.ResolutionChanged = null;

            base.Dispose(disposing);
        }

        #endregion

        #region Private Methods
        private void Recreate(ref RenderTarget2D m_screenTarget)
        {
            m_screenTarget?.Dispose();
            m_screenTarget = new RenderTarget2D(
                this.GraphicsDevice,
                this.m_resolution.X,
                this.m_resolution.Y,
                false,
                SurfaceFormat.Color,
                DepthFormat.Depth24Stencil8);
        }
        #endregion

    }
}
