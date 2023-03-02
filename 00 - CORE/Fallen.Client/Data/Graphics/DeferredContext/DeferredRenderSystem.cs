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
using System.Linq;
using Client.Data.Gos;
using Client.Data.Gos.Components;
using Client.Data.Graphics.Shaders;
using Engine.Core;
using Engine.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Client.Data.Graphics.DeferredContext
{
    public enum DeferredState
    {
        Albedo,
        Normal,
        Depth,
    }

    public sealed class DeferredRenderSystem : RenderSystemBase
    {

        #region Properties

        public GameObjectCollection ObjectsToRender { get; }

        #endregion


        #region Fields
        private RenderTarget2D[] targets;
        private ClearEffect m_clearEffect;
        private DeferredObjectEffect m_deferredEffect;
        #endregion


        public DeferredRenderSystem(GraphicsDevice graphicsDevice)
            : base(graphicsDevice)
        {
            this.targets = new RenderTarget2D[3];
            this.ObjectsToRender = new GameObjectCollection();
        }



        #region Public Methods

        public RenderTarget2D GetRenderTarget2D(DeferredState state)
        {
            return this.targets[(int)state];
        }

        public override void Begin()
        {
            this.GraphicsDevice.SetRenderTargets(this.GetRenderTargetBindings());
            //this.m_clearEffect.Apply(this.m_clearEffect.GetCurrentTechnique());
            this.GraphicsDevice.Clear(Color.TransparentBlack);
        }

        public void RenderToGBuffer(Time frameTime)
        {
            // TODO: implement 3D skeletal animations

            if (this.ObjectsToRender.Count == 0) { return; }
            if (this.Camera == null) { return; }

            var samplerState = DeferredObjectEffect.TextureRepeatSampler;
            this.SetGraphicsStates(
                BlendState.AlphaBlend,
                RasterizerState.CullCounterClockwise,
                DepthStencilState.Default,
                new Tuple<int, SamplerState>(0, samplerState));

            foreach (DynamicWorldObject obj in this.ObjectsToRender)
            {
                if (obj.GetComponent<ModelRenderer>() is ModelRenderer modelRenderer &&
                    modelRenderer.IsInCameraView(this.Camera.GetBoundingFrustum()))
                {
                    // TODO: check octbounds to see what is actually visible.
                    this.RenderModel(modelRenderer);
                }
            }

            this.ResetGraphicsStates();
        }

        public override void End()
        {
            this.GraphicsDevice.SetRenderTargets(null);
        }
        #endregion

        #region Protected Overrides
        protected override void OnInitialized(object sender, EventArgs e)
        {
            this.m_deferredEffect = new DeferredObjectEffect();
            this.m_clearEffect = new ClearEffect();

            base.OnInitialized(sender, e);
        }

        protected override void OnResolutionChanged(object sender, EventArgs e)
        {
            this.RecreateRenderTarget(ref this.targets[(int)DeferredState.Albedo]);
            this.RecreateRenderTarget(ref this.targets[(int)DeferredState.Normal]);
            this.RecreateRenderTarget(ref this.targets[(int)DeferredState.Depth]);

            base.OnResolutionChanged(sender, e);
        }

        #endregion

        #region Private Methods

        private void RenderModel(ModelRenderer modelRenderer)
        {
            var transform = modelRenderer.GameObject.GetComponent<Transform>();
            var l_world = transform.GetWorld();
            var boneTransforms = modelRenderer.GetBoneTransforms();

            this.m_deferredEffect.View = this.Camera.View;
            this.m_deferredEffect.Projection = this.Camera.Projection;

            foreach (var modelMesh in modelRenderer.Model.Meshes)
            {
                var boneTransform = boneTransforms.Any() ?
                    boneTransforms[modelMesh.ParentBone.Index] : Matrix.Identity;

                this.m_deferredEffect.World = boneTransform * l_world;

                // TODO: bind texture to model mesh through lookup dictionary.
                if (modelMesh.Tag is M2XTextureTagData textureData)
                {
                    this.m_deferredEffect.TextureEnabled = textureData.HasTextures && textureData.TextureEnabled;

                    if (textureData.HasTextures)
                    {
                        // Index implies that the first is a diffuse texture contained in the texture map
                        this.m_deferredEffect.DiffuseTexture =
                            textureData.LoadTexture(modelRenderer.TextureRelativeDirectory + "\\" + textureData.TexturePaths[0]);
                    }
                }

                this.m_deferredEffect.Apply(this.m_deferredEffect.GetTechnique("DeferredDefault"));
                modelRenderer.Render(modelMesh);
            }
        }

        private RenderTargetBinding[] GetRenderTargetBindings()
        {
            return new RenderTargetBinding[]
            {
                new RenderTargetBinding(this.targets[(int)DeferredState.Albedo]),
                new RenderTargetBinding(this.targets[(int)DeferredState.Normal]),
                new RenderTargetBinding(this.targets[(int)DeferredState.Depth]),
            };
        }

        #endregion
    }
}
