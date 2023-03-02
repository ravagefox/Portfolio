// Source: Skybox
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

using Client.Data.Gos;
using Client.Data.Gos.Components;
using Engine.Core;
using Engine.Graphics.Linq;
using Fallen.Common;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Client.Data.Graphics
{
    public sealed class Skybox : WorldObject
    {
        public override WorldObjectType ObjectType => WorldObjectType.Object;


        private ModelRenderer modelDisplayComponent;
        private float yaw;
        private Matrix skyboxWorld;
        private Matrix skyboxProjection;

        public Skybox() : base(uint.MaxValue)
        {
            this.modelDisplayComponent = new ModelRenderer();
        }

        public override void Initialize()
        {
            this.modelDisplayComponent.Model = this.Content.Load<Model>("Doodads\\Sphere.m2x");
            this.modelDisplayComponent.Texture = this.Content.Load<Texture2D>("Textures\\Missing.png");

            this.AddComponent(this.modelDisplayComponent);

            this.Transform.SetScale(Vector3.One * 1000.0f);
            base.Initialize();
        }

        public override void Update(Time frameTime)
        {
            var followTransform = Camera.Current.GetFollowObject().Transform;

            this.yaw += 1.0f * (float)frameTime.ElapsedFrameTime.TotalSeconds;
            this.yaw = MathF.Wrap(this.yaw, -MathF.PI, MathF.PI);

            this.Transform.SetRotation(this.yaw, 0.0f, 0.0f);
            this.skyboxWorld = this.Transform.GetWorld() *
                               Matrix.CreateTranslation(followTransform.Position);

            this.skyboxProjection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(84.4f), 1.78f, 0.01f, 7000.0f);

            base.Update(frameTime);
        }

        public void Apply(Time frameTime)
        {
            var skyboxRenderer = this.GetComponent<ModelRenderer>();
            if (skyboxRenderer != null)
            {
                this.GraphicsDevice.SetBlendState(BlendState.Opaque, out var oldBlend);
                this.GraphicsDevice.SetRasterizerState(RasterizerState.CullClockwise, out var oldRaster);
                this.GraphicsDevice.SetDepthStencilState(DepthStencilState.Default, out var oldDepth);

                //skyboxRenderer.UseDefaultCameraMatrices = false;
                skyboxRenderer.World = this.skyboxWorld;
                skyboxRenderer.View = Camera.Current.View;
                skyboxRenderer.Projection = this.skyboxProjection;
                skyboxRenderer.Apply(frameTime);

                this.GraphicsDevice.SetBlendState(oldBlend, out _);
                this.GraphicsDevice.SetRasterizerState(oldRaster, out _);
                this.GraphicsDevice.SetDepthStencilState(oldDepth, out _);
            }
        }

    }
}