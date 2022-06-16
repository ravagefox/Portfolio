// Source: PointLightInstanceEffect
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

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Wargame.Data.Gos;
using Wargame.Data.Graphics.DeferredContext;

namespace Wargame.Data.Graphics.Shaders
{
    public sealed class PointLightInstanceEffect : ShaderBase
    {
        public Camera Camera { get; set; }

        public DeferredRenderSystem RenderContext { get; set; }


        private EffectParameter mWorldParam;
        private EffectParameter mViewParam;
        private EffectParameter mProjParam;
        private EffectParameter mInvProjParam;
        private EffectParameter mNormParam;
        private EffectParameter mCamEyeParam;
        private EffectParameter mDepthParam;
        private EffectParameter mHalfPixelParam;
        private EffectParameter mColorParam;

        public PointLightInstanceEffect() : base(LoadShaderBytecode("Shaders\\lighting\\pointlightinstance"))
        {
            this.mCamEyeParam = this.Effect.Parameters["mCameraEye"];
            this.mWorldParam = this.Effect.Parameters["mWorld"];
            this.mViewParam = this.Effect.Parameters["mView"];
            this.mProjParam = this.Effect.Parameters["mProjection"];
            this.mInvProjParam = this.Effect.Parameters["mInvProjection"];
            this.mNormParam = this.Effect.Parameters["mNormal"];
            this.mDepthParam = this.Effect.Parameters["mDepth"];
            this.mHalfPixelParam = this.Effect.Parameters["mHalfPixel"];
            this.mColorParam = this.Effect.Parameters["mAlbedo"];
        }

        protected override void OnApply()
        {
            var nT = this.RenderContext.GetTexture(DeferredRenderTarget.Normal);
            var dT = this.RenderContext.GetTexture(DeferredRenderTarget.Depth);
            var cT = this.RenderContext.GetTexture(DeferredRenderTarget.Albedo);

            this.mColorParam?.SetValue(cT);
            this.mNormParam?.SetValue(nT);
            this.mDepthParam?.SetValue(dT);
            this.mInvProjParam?.SetValue(
                Matrix.Invert(this.Camera.View * this.Camera.Projection));
            this.mViewParam?.SetValue(this.Camera.View);
            this.mProjParam?.SetValue(this.Camera.Projection);
            this.mWorldParam?.SetValue(this.World);
            this.mHalfPixelParam?.SetValue(this.HalfPixel);
            this.mCamEyeParam?.SetValue(this.Camera.Transform.Position);
        }
    }
}
