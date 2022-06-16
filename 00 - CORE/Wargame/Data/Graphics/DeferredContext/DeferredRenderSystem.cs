// Source: DeferredRenderSystem
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
using Engine.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Wargame.Data.Gos;
using Wargame.Data.Gos.Components;
using Wargame.Data.Graphics.Shaders;

namespace Wargame.Data.Graphics.DeferredContext
{
    public enum DeferredRenderTarget
    {
        Albedo = 0,
        Normal = 1,
        Depth = 2,
    }


    public sealed class DeferredRenderSystem : RenderSystemBase
    {
        private RenderTargetBinding[] FirstPassBindings =>
            new RenderTargetBinding[]
            {
                new RenderTargetBinding(this.albedoTarget, 0),
                new RenderTargetBinding(this.normalTarget, 0),
                new RenderTargetBinding(this.depthTarget, 0),
            };


        private RenderTarget2D albedoTarget, normalTarget, depthTarget;
        private ScreenQuad screenQuad;
        private ClearEffect clearEffect;
        private DeferredObjectEffect deferredEffect;



        public DeferredRenderSystem(GraphicsDevice graphicsDevice)
            : base(graphicsDevice)
        {
        }


        public Texture2D GetTexture(DeferredRenderTarget target)
        {
            return (Texture2D)this.FirstPassBindings[(int)target].RenderTarget;
        }




        public override void Begin()
        {
            this.GraphicsDevice.SetRenderTargets(this.FirstPassBindings);
            this.clearEffect.Apply(this.clearEffect.GetTechnique("ClearBuffer"));
            this.screenQuad.Render();
        }


        public void RenderToGBuffer(IEnumerable<GameObject> gameObjects)
        {
            this.SetGraphicsStates(
                RasterizerState.CullCounterClockwise,
                DepthStencilState.Default,
                blendState: BlendState.Opaque);

            this.deferredEffect.View = Camera.Current.View;
            this.deferredEffect.Projection = Camera.Current.Projection;

            foreach (var obj in gameObjects)
            {
                if (obj.GetComponent<ModelRenderer>() is ModelRenderer renderer &&
                    renderer.IsVisible &&
                    renderer.Model != null)
                {
                    if (renderer.IsInViewSpace(Camera.Current.GetBoundingFrustum()))
                    {
                        var boneTransforms = renderer.GetBoneTransforms();

                        Matrix world;
                        foreach (var modelMesh in renderer.Model.Meshes)
                        {
                            world = boneTransforms.Any() ?
                                    boneTransforms[modelMesh.ParentBone.Index] :
                                    Matrix.Identity;

                            world *= obj.GetComponent<Transform>().GetWorld();

                            this.deferredEffect.World = world;

                            // TODO: implement a texture lookup dictionary based on the model mesh.
                            this.deferredEffect.TextureEnabled = renderer.Texture != null;
                            this.deferredEffect.DiffuseTexture = renderer.Texture;

                            this.deferredEffect.Apply(
                                this.deferredEffect.GetTechnique("DeferredDefault"));

                            renderer.Render(modelMesh);
                        }
                    }
                }
            }
        }

        public override void End()
        {
            this.GraphicsDevice.SetRenderTarget(null);
            this.ResetGraphicsState();
        }

        protected override void OnInitialize(object sender, EventArgs e)
        {
            this.screenQuad = new ScreenQuad();
            this.clearEffect = new ClearEffect();
            this.deferredEffect = new DeferredObjectEffect();

            base.OnInitialize(sender, e);
        }

        protected override void OnResolutionChanged(object sender, EventArgs e)
        {
            this.RecreateRenderTarget2D(ref this.albedoTarget);
            this.RecreateRenderTarget2D(ref this.normalTarget);

            this.depthTarget?.Dispose();
            this.depthTarget = new RenderTarget2D(
                this.GraphicsDevice,
                this.Resolution.X,
                this.Resolution.Y,
                false,
                SurfaceFormat.Single,
                DepthFormat.Depth24);

            base.OnResolutionChanged(sender, e);
        }

        protected override void OnDispose()
        {
            this.albedoTarget?.Dispose();
            this.normalTarget?.Dispose();
            this.depthTarget?.Dispose();
            this.deferredEffect?.Dispose();
            this.clearEffect?.Dispose();

            this.screenQuad = null;
        }
    }
}
