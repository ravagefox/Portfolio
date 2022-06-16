// Source: CombineEffect
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

using Microsoft.Xna.Framework.Graphics;

namespace Wargame.Data.Graphics.Shaders
{
    public sealed class FinalCombineEffect : ShaderBase
    {
        public Texture2D ColorTarget { get; set; }

        public Texture2D ShadowTarget { get; set; }

        public Texture2D SsaoTarget { get; set; }

        public Texture2D LightTarget { get; set; }


        private EffectParameter mAlbedoParam;
        private EffectParameter mShadowMapParam;
        private EffectParameter mSsaoTargetParam;
        private EffectParameter mLightTargetParam;

        public FinalCombineEffect()
            : base(LoadShaderBytecode("shaders\\deferred\\combine.hlsl"))
        {
            this.mAlbedoParam = this.Effect.Parameters["mAlbedo"];
            this.mShadowMapParam = this.Effect.Parameters["mShadowMap"];
            this.mSsaoTargetParam = this.Effect.Parameters["mSsaoMap"];
            this.mLightTargetParam = this.Effect.Parameters["mLightMap"];

        }

        protected override void OnApply()
        {
            this.mAlbedoParam.SetValue(this.ColorTarget);
            this.mShadowMapParam.SetValue(this.ShadowTarget);
            this.mSsaoTargetParam.SetValue(this.SsaoTarget);
            this.mLightTargetParam.SetValue(this.LightTarget);

        }
    }
}
