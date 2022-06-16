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
using System.Collections.Generic;
using System.Linq;
using Engine.Core;
using Engine.Core.Input;
using Engine.Data;
using Engine.Graphics;
using Engine.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Wargame.Data.Gos;
using Wargame.Data.Graphics.DeferredContext;
using Wargame.Data.Graphics.Shaders;

namespace Wargame.Data.Graphics
{
    public sealed class RenderManager : IDisposable
    {

        [Flags]
        public enum PostProcess : uint
        {
            SSAO = 1,
            FXAA = 2,
            ColorGrading = 4,
        }


        public Texture2D FullScreenTexture => this.screenTarget;

        public Point Resolution
        {
            get => this.resolution;
            set
            {
                if (this.resolution != value)
                {
                    this.resolution = value;

                    this.DeferredSystem.Resolution = value;
                    this.LightSystem.Resolution = value;
                    this.ShadowSystem.Resolution = value;
                    this.SSAOSystem.Resolution = value;

                    this.Recreate(ref this.screenTarget);
                }
            }
        }

        public float GlobalLightIntensity
        {
            get => this.globalLightIntensity;
            set
            {
                if (this.globalLightIntensity != value)
                {
                    this.globalLightIntensity = value;
                    this.ShadowSystem.Intensity = value;
                }
            }
        }

        public Rectangle ScreenRect => new Rectangle(
            0,
            0,
            this.GraphicsDevice.Viewport.Bounds.Width,
            this.GraphicsDevice.Viewport.Bounds.Height);

        public GraphicsDevice GraphicsDevice { get; }

        public DeferredRenderSystem DeferredSystem { get; }
        public PointLightRenderSystem LightSystem { get; }
        public CascadeShadowRenderSystem ShadowSystem { get; }
        public SsaoRenderSystem SSAOSystem { get; }


        private ScreenQuad screenQuad;
        private FinalCombineEffect combineEffect;

        private RenderTarget2D screenTarget;
        private uint postProcessFlags;
        private Point resolution;
        private float globalLightIntensity = 1.0f;
        private bool hideLights;


        public RenderManager(GraphicsDevice graphicsDevice)
        {
            this.GraphicsDevice = graphicsDevice;

            this.DeferredSystem = new DeferredRenderSystem(graphicsDevice);
            this.ShadowSystem = new CascadeShadowRenderSystem(graphicsDevice);
            this.LightSystem = new PointLightRenderSystem(this.DeferredSystem);
            this.SSAOSystem = new SsaoRenderSystem(this.DeferredSystem);

            this.screenQuad = new ScreenQuad();

            ServiceProvider.Instance.AddService<RenderManager>(this);
        }

        private void Recreate(ref RenderTarget2D screenTarget)
        {
            screenTarget?.Dispose();
            screenTarget = new RenderTarget2D(
                this.GraphicsDevice,
                this.resolution.X,
                this.resolution.Y,
                false,
                SurfaceFormat.ColorSRgb,
                DepthFormat.Depth24Stencil8);
        }

        public void Initialize()
        {
            this.TogglePostProcess(PostProcess.SSAO);
            this.TogglePostProcess(PostProcess.ColorGrading);
            this.TogglePostProcess(PostProcess.FXAA);
            ;

            this.combineEffect = new FinalCombineEffect();

            this.DeferredSystem.InitializeSystem();

            this.ShadowSystem.Quality = ShadowQuality.Ultra;
            this.ShadowSystem.FilterQuality = FilterSize.Filter7x7;
            this.ShadowSystem.LightDirection = new Vector3(0, 1, -1);
            this.ShadowSystem.InitializeSystem();

            this.LightSystem.InitializeSystem();
            this.SSAOSystem.InitializeSystem();

            var keyboard = ServiceProvider.Instance.GetService<Keyboard>();
            keyboard.Control.KeyUp += (s, e) =>
            {
                if (e.KeyCode == System.Windows.Forms.Keys.P)
                {
                    this.TogglePostProcess(PostProcess.SSAO);
                }

                if (e.KeyCode == System.Windows.Forms.Keys.L)
                {
                    this.hideLights = !this.hideLights;
                }
            };
        }


        public void RenderToTexture(IEnumerable<GameObject> actors)
        {
            var camera = Camera.Current;

            var nonLightOrCameraEntities =
                actors.Except(actors.OfType<PointLight>());
            nonLightOrCameraEntities =
                nonLightOrCameraEntities.Except(actors.OfType<Camera>());

            // Main-Process
            this.DeferredSystem.Begin();
            this.DeferredSystem.RenderToGBuffer(nonLightOrCameraEntities);
            this.DeferredSystem.End();

            this.ShadowSystem.SetActors(nonLightOrCameraEntities);
            this.ShadowSystem.Begin();
            this.ShadowSystem.RenderCascadeScene();
            this.ShadowSystem.End();

            this.LightSystem.Begin();
            if (!this.hideLights)
            {
                this.LightSystem.SetLights(actors.OfType<PointLight>());
                this.LightSystem.DrawLights(camera);
            }
            this.LightSystem.End();

            // Post-Process
            this.RenderPostProcess(camera);


            // Begin combine process
            this.GraphicsDevice.SetRenderTarget(this.screenTarget);
            this.GraphicsDevice.Clear(Color.Black);

            this.combineEffect.ColorTarget = this.DeferredSystem.GetTexture(DeferredRenderTarget.Albedo);
            this.combineEffect.LightTarget = this.LightSystem.LightTexture;
            this.combineEffect.ShadowTarget = this.ShadowSystem.SceneTexture;
            this.combineEffect.SsaoTarget = this.SSAOSystem.SsaoTexture;

            this.combineEffect.Apply(this.combineEffect.GetCurrentTechnique());
            this.screenQuad.Render();
            this.GraphicsDevice.SetRenderTarget(null);
        }

        private void RenderPostProcess(Camera camera)
        {
            this.SSAOSystem.Enable = this.postProcessFlags.HasFlag((uint)PostProcess.SSAO);

            this.SSAOSystem.Begin();
            this.SSAOSystem.RenderSSAO(camera);
            this.SSAOSystem.End();
        }

        public void TogglePostProcess(PostProcess process)
        {
            if (this.postProcessFlags.HasFlag((uint)process))
            {
                this.postProcessFlags &= ~(uint)process;
            }
            else { this.postProcessFlags |= (uint)process; }
        }

        public void Dispose()
        {
            this.DeferredSystem?.Dispose();
            this.ShadowSystem?.Dispose();
            this.LightSystem?.Dispose();

            ServiceProvider.Instance.RemoveService<RenderManager>();
        }

        public Point PointToClient(Point screenPoint)
        {
            var mouse = ServiceProvider.Instance.GetService<Mouse>();
            var dCtrl = ServiceProvider.Instance.GetService<GraphicsDeviceControl>();

            var scaledX = screenPoint.X * (this.resolution.X / (float)mouse.Control.Width);
            var scaledY = screenPoint.Y * (this.resolution.Y / (float)mouse.Control.Height);

            return new Point((int)scaledX, (int)scaledY);
        }
    }
}
