// Source: DiffuseEffect
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
    public sealed class DiffuseEffect : Shader
    {
        public Texture2D Texture { get; set; }
        public bool TextureEnabled { get; set; }


        private EffectParameter m_texParam;
        private EffectParameter m_texEnabledParam;


        public DiffuseEffect()
            : base(LoadShaderBytecode("Shaders\\basic"))
        {
            this.m_texParam = this.Effect.Parameters["mAlbedo"];
            this.m_texEnabledParam = this.Effect.Parameters["mTextureEnabled"];
        }


        protected override void OnApply()
        {
            this.m_texParam?.SetValue(this.Texture);
            this.m_texEnabledParam?.SetValue(
                this.Texture != null && this.TextureEnabled);
        }
    }
}
