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
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Wargame.Data.Gos;
using Wargame.Data.Graphics.Shaders;

namespace Wargame.Data.Graphics.DeferredContext
{
    public struct PointLightPositionRadiusColor : IVertexType
    {
        public static readonly VertexElement[] Elements = new VertexElement[]
        {
            new VertexElement(0, VertexElementFormat.Vector4, VertexElementUsage.Position, 0),
            new VertexElement(16, VertexElementFormat.Single, VertexElementUsage.TextureCoordinate, 0),
            new VertexElement(20, VertexElementFormat.Vector3, VertexElementUsage.TextureCoordinate, 1),
            new VertexElement(32, VertexElementFormat.Single, VertexElementUsage.TextureCoordinate, 2),
        };


        public VertexDeclaration VertexDeclaration => new VertexDeclaration(Elements);

        public Vector4 Position;
        public float Radius;
        public Vector3 Color;
        public float LightIntensity;

        public PointLightPositionRadiusColor(Vector4 position, float radius, Vector3 color, float lightIntensity = 1.0f)
        {
            this.Position = position;
            this.Radius = radius;
            this.Color = color;
            this.LightIntensity = lightIntensity;
        }
    }



    public sealed class PointLightRenderSystem : InstanceRenderSystemBase<PointLightPositionRadiusColor>
    {
        public Texture2D LightTexture => this.lightTarget;


        private RenderTarget2D lightTarget;
        private PointLightInstanceEffect instanceEffect;


        public PointLightRenderSystem(GraphicsDevice graphicsDevice) : base(graphicsDevice)
        {
        }

        protected override VertexDeclaration BuildVertexDeclaration()
        {
            return new VertexDeclaration(PointLightPositionRadiusColor.Elements);
        }

        protected override void OnInitialize(object sender, EventArgs e)
        {
            this.instanceEffect = new PointLightInstanceEffect();

            base.OnInitialize(sender, e);
        }

        protected override void OnResolutionChanged(object sender, EventArgs e)
        {
            this.RecreateRenderTarget2D(ref this.lightTarget);

            base.OnResolutionChanged(sender, e);
        }

        public override void Begin()
        {
            this.SetGraphicsStates(
                rasterizerState: RasterizerState.CullNone,
                blendState: BlendState.Additive,
                depthState: DepthStencilState.Default);

            this.GraphicsDevice.SetRenderTarget(this.lightTarget);
            this.GraphicsDevice.Clear(Color.Black);
        }

        public void DrawLights(DeferredRenderSystem renderSystem, Camera camera)
        {
            if (this.InstanceCount == 0)
            {
                return;
            }

            this.instanceEffect.World = Matrix.CreateScale(10.0f);
            this.instanceEffect.View = camera.View;
            this.instanceEffect.Projection = camera.Projection;
            this.instanceEffect.Camera = camera;
            this.instanceEffect.RenderContext = renderSystem;

            this.instanceEffect.Apply(this.instanceEffect.GetCurrentTechnique());
            this.GraphicsDevice.SetVertexBuffers(this.GetVertexBuffers());
            this.GraphicsDevice.Indices = this.GetIndexBuffer();

            this.GraphicsDevice.DrawInstancedPrimitives(
                PrimitiveType.TriangleList,
                0,
                0,
                this.GraphicsDevice.Indices.IndexCount / 3,
                this.InstanceCount);
        }

        public override void End()
        {
            this.GraphicsDevice.SetRenderTargets(null);
            this.GraphicsDevice.SetVertexBuffers(null);
            this.GraphicsDevice.Indices = null;

            this.ResetGraphicsState();
        }

        protected override void OnDispose()
        {
            this.lightTarget?.Dispose();
            this.instanceEffect?.Dispose();
        }
    }
}
