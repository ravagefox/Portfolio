// Source: FinalCombineEffect
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

using Engine.Graphics;
using Microsoft.Xna.Framework.Graphics;

namespace Client.Data.Graphics.Shaders
{
    public sealed class FinalCombineEffect : Shader
    {
        public Texture2D ColorTarget { get; set; }
        public Texture2D ShadowTarget { get; set; }
        public Texture2D SsaoTarget { get; set; }
        public Texture2D LightTarget { get; set; }


        private EffectParameter m_albedoParam;
        private EffectParameter m_shadowMapParam;
        private EffectParameter m_ssaoParam;
        private EffectParameter m_lightParam;


        public FinalCombineEffect()
            : base(LoadShaderBytecode("Shaders\\Deferred\\Combine"))
        {
            this.m_albedoParam = this.Effect.Parameters["mAlbedo"];
            this.m_shadowMapParam = this.Effect.Parameters["mShadowMap"];
            this.m_ssaoParam = this.Effect.Parameters["mSsaoMap"];
            this.m_lightParam = this.Effect.Parameters["mLightMap"];
        }

        protected override void OnApply()
        {
            this.m_albedoParam?.SetValue(this.ColorTarget);
            this.m_shadowMapParam?.SetValue(this.ShadowTarget);
            this.m_ssaoParam?.SetValue(this.SsaoTarget);
            this.m_lightParam?.SetValue(this.LightTarget);
        }
    }
}
