// Source: SsaoEffect
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

using Engine.Math;
using Microsoft.Xna.Framework.Graphics;
using Wargame.Data.Graphics.DeferredContext;

namespace Wargame.Data.Graphics.Shaders
{
    public sealed class SsaoEffect : ShaderBase
    {
        public DeferredRenderSystem RenderSystem { get; }

        public bool Enable { get; set; } = true;
        public int TapNumber
        {
            get => this.mNumTaps;
            set
            {
                var tapV = MathHelper.Clamp(value, 1, MaxTaps);
                if (this.mNumTaps != tapV)
                {
                    this.mNumTaps = tapV;
                }
            }
        }
        public float Threshold { get; set; } = 0.05f;
        public float Scale { get; set; } = 1.0f;
        public float TapSize { get; set; } = 0.02f;


        public const int MaxTaps = 16;


        private EffectParameter mDepthParam;
        private EffectParameter mNormalParam;

        private EffectParameter mEnableParam;
        private EffectParameter mNumTapParam;
        private EffectParameter mScaleParam;
        private EffectParameter mTapSizeParam;
        private EffectParameter mThresholdParam;
        private int mNumTaps = MaxTaps;

        public SsaoEffect(DeferredRenderSystem renderSystem)
            : base(LoadShaderBytecode("Shaders\\PostProcess\\Ssao"))
        {
            this.RenderSystem = renderSystem;

            this.mDepthParam = this.Effect.Parameters["mDepth"];
            this.mNormalParam = this.Effect.Parameters["mNormal"];

            this.mEnableParam = this.Effect.Parameters["mEnableSSAO"];
            this.mNumTapParam = this.Effect.Parameters["mNumTaps"];
            this.mThresholdParam = this.Effect.Parameters["mThreshold"];
            this.mScaleParam = this.Effect.Parameters["mScale"];
            this.mTapSizeParam = this.Effect.Parameters["mTapSize"];
        }


        protected override void OnApply()
        {
            var dt = this.RenderSystem.GetTexture(DeferredRenderTarget.Depth);
            var nt = this.RenderSystem.GetTexture(DeferredRenderTarget.Normal);

            this.mDepthParam.SetValue(dt);
            this.mNormalParam.SetValue(nt);

            this.mEnableParam.SetValue(this.Enable);
            this.mNumTapParam.SetValue(this.TapNumber);
            this.mScaleParam.SetValue(this.Scale);
            this.mTapSizeParam.SetValue(this.TapSize);
            this.mThresholdParam.SetValue(this.Threshold);
        }
    }
}
