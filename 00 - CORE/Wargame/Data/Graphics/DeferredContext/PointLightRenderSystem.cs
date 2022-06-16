// Source: PointLightRenderSystem
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
using Engine.Assets;
using Engine.Core;
using Engine.Data;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Wargame.Data.Gos;
using Wargame.Data.Gos.Components;
using Wargame.Data.Graphics.Shaders;

namespace Wargame.Data.Graphics.DeferredContext
{
    public sealed class PointLightRenderSystem : RenderSystemBase
    {
        public DeferredRenderSystem RenderSystem { get; }


        public Texture2D LightTexture => this.lightTarget;


        private ModelRenderer renderer;
        private PointLightEffect pointEffect;
        private RenderTarget2D lightTarget;
        private IEnumerable<PointLight> lights;


        public PointLightRenderSystem(
            DeferredRenderSystem renderSystem) :
            base(renderSystem.GraphicsDevice)
        {
            this.RenderSystem = renderSystem;
        }

        protected override void OnInitialize(object sender, EventArgs e)
        {
            var loader = ServiceProvider.Instance.GetService<ILoader>();
            this.renderer = new ModelRenderer();
            this.renderer.Model = loader.Load<Model>("Models\\Sphere");

            this.lights = Enumerable.Empty<PointLight>();
            this.pointEffect = new PointLightEffect(this.RenderSystem);
            this.RecreateRenderTarget2D(ref this.lightTarget);

            base.OnInitialize(sender, e);
        }


        public override void Begin()
        {
            this.GraphicsDevice.SetRenderTarget(this.lightTarget);
            this.GraphicsDevice.Clear(Color.Black);

            var blend = new BlendState()
            {
                AlphaSourceBlend = Blend.One,
                ColorSourceBlend = Blend.One,
                ColorDestinationBlend = Blend.One,
                AlphaDestinationBlend = Blend.One,
            };

            this.SetGraphicsStates(
                blendState: blend,
                depthState: DepthStencilState.None,
                rasterizerState: RasterizerState.CullNone,
                newSampler: new Tuple<int, SamplerState>(0, SamplerState.PointClamp));
        }

        public void Add(PointLight pointLight)
        {
            this.lights = this.lights.Append(pointLight);
        }

        public void Remove(PointLight pointLight)
        {
            this.lights = this.lights.Except(new[] { pointLight });
        }

        public void SetLights(IEnumerable<PointLight> lights)
        {
            this.lights = lights.ToList();
        }

        public void DrawLights(Camera camera)
        {
            this.pointEffect.Camera = camera;
            this.pointEffect.View = camera.View;
            this.pointEffect.Projection = camera.Projection;

            foreach (var light in this.lights)
            {
                if (this.IsInViewSpace(light, camera))
                {
                    var world = Matrix.CreateScale(light.Radius * MathUtil.PI) * Matrix.CreateTranslation(light.Transform.Position);

                    this.pointEffect.World = world;
                    this.pointEffect.LightColor = light.LightColor;
                    this.pointEffect.LightRadius = light.Radius;
                    this.pointEffect.LightIntensity = light.Intensity;
                    this.pointEffect.LightPosition = light.Transform.Position;

                    this.SetRasterizerState(camera, light);

                    foreach (var modelMesh in this.renderer.Model.Meshes)
                    {
                        this.pointEffect.Apply(this.pointEffect.GetCurrentTechnique());

                        this.renderer.Render(modelMesh);
                    }
                }
            }
        }

        private bool IsInViewSpace(PointLight light, Camera camera)
        {
            var frustum = camera.GetBoundingFrustum();

            return frustum.Intersects(light.BoundingSphere);
        }

        private void SetRasterizerState(Camera camera, PointLight light)
        {
            this.GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;

            if (this.IsInViewSpace(light, camera))
            {
                var frustum = camera.GetBoundingFrustum();

                var distanceToCenter = Vector3.Distance(camera.Transform.Position, light.Transform.Position);
                var radiusSquared = (float)System.Math.Pow(light.Radius, 2);

                if (distanceToCenter < (radiusSquared + frustum.Near.D))
                {
                    this.GraphicsDevice.RasterizerState = RasterizerState.CullClockwise;
                }
            }
        }

        public override void End()
        {
            this.GraphicsDevice.SetRenderTargets(null);
            this.ResetGraphicsState();
        }

        protected override void OnDispose()
        {
            this.lightTarget?.Dispose();
            this.pointEffect?.Dispose();
        }

        internal IEnumerable<GameObject> GetLights()
        {
            return this.lights;
        }
    }
}
