// Source: PointLightEffect
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
    public sealed class PointLightEffect : ShaderBase
    {
        public DeferredRenderSystem RenderSystem { get; }

        public Vector3 LightPosition { get; set; }
        public Color LightColor { get; set; }
        public float LightIntensity { get; set; }
        public float LightRadius { get; set; }

        public Camera Camera { get; set; }


        private EffectParameter mLightPosParam;
        private EffectParameter mLightColorParam;
        private EffectParameter mLightIntensityParam;
        private EffectParameter mLightRadiusParam;
        private EffectParameter mCamPosParam;
        private EffectParameter mInvViewProjParam;

        private EffectParameter mDepthParam;
        private EffectParameter mNormalParam;
        private EffectParameter mColorParam;


        public PointLightEffect(DeferredRenderSystem renderSystem)
            : base(LoadShaderBytecode("Shaders\\Deferred\\pointlight"))
        {
            this.RenderSystem = renderSystem;

            this.mLightColorParam = this.Effect.Parameters["mLightColor"];
            this.mLightRadiusParam = this.Effect.Parameters["mLightRadius"];
            this.mLightIntensityParam = this.Effect.Parameters["mLightIntensity"];
            this.mLightPosParam = this.Effect.Parameters["mLightPosition"];

            this.mCamPosParam = this.Effect.Parameters["mCamPos"];
            this.mInvViewProjParam = this.Effect.Parameters["mInvViewProjection"];

            this.mDepthParam = this.Effect.Parameters["mDepth"];
            this.mNormalParam = this.Effect.Parameters["mNormal"];
            this.mColorParam = this.Effect.Parameters["mAlbedo"];
        }


        protected override void OnApply()
        {
            var ivp = Matrix.Invert(this.Camera.View * this.Camera.Projection);
            this.mInvViewProjParam.SetValue(ivp);
            this.mCamPosParam.SetValue(this.Camera.Transform.Position);

            this.mLightIntensityParam.SetValue(this.LightIntensity);
            this.mLightRadiusParam.SetValue(this.LightRadius);
            this.mLightColorParam.SetValue(this.LightColor.ToVector3());
            this.mLightPosParam.SetValue(this.LightPosition);

            this.mDepthParam.SetValue(this.RenderSystem.GetTexture(DeferredRenderTarget.Depth));
            this.mNormalParam.SetValue(this.RenderSystem.GetTexture(DeferredRenderTarget.Normal));
        }
    }
}
