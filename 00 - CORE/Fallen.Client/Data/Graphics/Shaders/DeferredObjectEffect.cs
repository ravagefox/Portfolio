// Source: DeferredObjectEffect
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
    public class DeferredObjectEffect : Shader
    {
        public Texture2D DiffuseTexture { get; set; }
        public bool TextureEnabled { get; set; }

        public int TextureRepeatCount { get; set; }

        public static SamplerState TextureRepeatSampler =>
            new SamplerState()
            {
                AddressU = TextureAddressMode.Wrap,
                AddressV = TextureAddressMode.Wrap,
                ComparisonFunction = CompareFunction.Greater,
                Filter = TextureFilter.Linear,
                FilterMode = TextureFilterMode.Comparison,
            };

        private EffectParameter m_diffuseTextureParam;
        private EffectParameter m_texEnabledParam;
        private EffectParameter m_texRepeatParam;


        public DeferredObjectEffect()
            : base(LoadShaderBytecode("Shaders\\Deferred\\RenderGBuffer"))
        {
            this.TextureRepeatCount = 1;

            this.m_diffuseTextureParam = this.Effect.Parameters["mAlbedo"];
            this.m_texEnabledParam = this.Effect.Parameters["mTextureEnabled"];
            this.m_texRepeatParam = this.Effect.Parameters["mRepeat"];
        }

        protected override void OnApply()
        {
            this.m_texRepeatParam?.SetValue(this.TextureRepeatCount);
            this.m_diffuseTextureParam?.SetValue(this.DiffuseTexture);
            this.m_texEnabledParam?.SetValue(
                this.DiffuseTexture != null && this.TextureEnabled);
        }
    }
}
