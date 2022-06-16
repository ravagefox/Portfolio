// Source: WeaponBase
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
using Engine.Assets;
using Engine.Core;
using Engine.Data;
using Engine.Graphics.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Wargame.Data.Gos.Components;
using Wargame.Data.Graphics;
using Wargame.Extensions;

namespace Wargame.Data.Gos.WeaponAssets
{
    public class WeaponBase : GameObject
    {
        public CascadeShadowContext ShadowContext { get; set; }


        private ModelRenderer renderer;
        private Transform transform;
        private Vector2 swayOffset;
        private float swayAmount;
        private float swayFactor;
        private BasicEffect bEffect;
        private float lastSwayFactor;

        public WeaponBase() : base()
        {
            this.swayFactor = 1.0f;
            this.swayAmount = 0.0f;
            this.swayOffset = Vector2.Zero;

            this.renderer = new ModelRenderer();
            this.transform = new Transform();
            this.bEffect = new BasicEffect(this.renderer.GraphicsDevice);
        }

        public void LoadModel(string path)
        {
            var iloader = ServiceProvider.Instance.GetService<ILoader>();
            var model = iloader.Load<Model>(path);

            this.renderer.Model = model;
        }

        public void LoadTexture(string path)
        {
            var iloader = ServiceProvider.Instance.GetService<ILoader>();
            var texture = iloader.Load<Texture2D>(path);

            this.renderer.Texture = texture;
        }

        public override void Initialize()
        {
            this.AddComponent(this.renderer);
            this.AddComponent(this.transform);

            base.Initialize();
        }

        public void Render(Time frameTime)
        {
            if (this.renderer.Model != null)
            {
                this.SetGraphicsSettings(
                    RasterizerState.CullCounterClockwise,
                    DepthStencilState.None,
                    BlendState.Opaque,
                    out var oldRaster,
                    out var oldDepth,
                    out var oldBlend);

                this.bEffect.View = Camera.Current.View;
                this.bEffect.Projection = Camera.Current.Projection;
                this.bEffect.DiffuseColor = Color.White.ToVector3();
                this.bEffect.SpecularColor = Color.White.ToVector3();
                this.bEffect.SpecularPower = 0.5f;
                this.bEffect.TextureEnabled = this.renderer.Texture != null;
                this.bEffect.Texture = this.renderer.Texture;
                this.bEffect.LightingEnabled = true;

                this.bEffect.DirectionalLight0.Enabled = true;
                this.bEffect.DirectionalLight0.Direction = -this.ShadowContext.LightDirection;
                this.bEffect.DirectionalLight0.DiffuseColor = Color.Wheat.ToVector3();


                var scale = Matrix.CreateScale(this.transform.Scale);

                var boneTransforms = new Matrix[this.renderer.Model.Bones.Count];
                this.renderer.Model.CopyBoneTransformsTo(boneTransforms);

                foreach (var mesh in this.renderer.Model.Meshes)
                {
                    var world = boneTransforms.Any() ?
                                boneTransforms[mesh.ParentBone.Index] :
                                Matrix.Identity;

                    world *=
                        scale *
                        Matrix.CreateFromQuaternion(this.transform.Rotation) *
                        this.CalculateWorldMatrix(Camera.Current);

                    this.bEffect.World = world;
                    this.bEffect.CurrentTechnique.Passes[0].Apply();
                    this.renderer.Render(mesh);
                }

                this.SetGraphicsSettings(oldRaster, oldDepth, oldBlend, out _, out _, out _);
            }
        }


        public override void Update(Time frameTime)
        {
            this.swayAmount += 0.00011f * (float)frameTime.ElapsedFrameTime.TotalMilliseconds;

            this.swayOffset = Vector2.Zero.MoveInFigure8(this.swayAmount);

            if (Camera.Current is Camera camera)
            {
                if (camera.GetComponent<KeyboardComponent>() is KeyboardComponent keyComp)
                {
                    var velocity = keyComp.GetVelocity();

                    this.swayFactor = velocity.LengthSquared() == 0.0f ? .050f : 1.0f;

                    var lerpVelocity = Vector2.Lerp(
                        this.swayOffset,
                        this.swayOffset + new Vector2(velocity.X / 10, velocity.Z / 10),
                        0.25f);

                    this.swayOffset.X += lerpVelocity.X / 10;
                    this.swayOffset.Y += lerpVelocity.Y / 10;

                    this.swayAmount += (float)MathHelper.Lerp(this.swayFactor, this.lastSwayFactor, 0.15f) * 0.05f;

                    this.lastSwayFactor = this.swayFactor;
                }
            }

            base.Update(frameTime);
        }


        private void SetGraphicsSettings(
            RasterizerState cullCounterClockwise,
            DepthStencilState @default,
            BlendState opaque,
            out RasterizerState oldRaster,
            out DepthStencilState oldDepth,
            out BlendState oldBlend)
        {
            this.renderer.GraphicsDevice.SetRasterizerState(cullCounterClockwise, out oldRaster);
            this.renderer.GraphicsDevice.SetDepthStencilState(@default, out oldDepth);
            this.renderer.GraphicsDevice.SetBlendState(opaque, out oldBlend);
        }

        protected Matrix CalculateWorldMatrix(Camera camera)
        {
            var invCamWorld = Matrix.Invert(camera.View);
            var weaponWorld = invCamWorld;

            var trueSwayOffset = this.swayOffset / 10;

            weaponWorld.Translation += (invCamWorld.Forward * 1.0f) +
                                       (invCamWorld.Down * trueSwayOffset.Y * this.swayFactor) +
                                       (invCamWorld.Right * trueSwayOffset.X * this.swayFactor);

            weaponWorld.Translation += (invCamWorld.Down * 0.75f) +
                                       (invCamWorld.Right * 0.75f);
            return weaponWorld;
        }
    }
}
