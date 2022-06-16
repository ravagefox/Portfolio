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

using Microsoft.Xna.Framework.Graphics;

namespace Wargame.Data.Graphics.Shaders
{
    public sealed class DeferredObjectEffect : ShaderBase
    {
        public Texture2D DiffuseTexture { get; set; }

        public bool TextureEnabled { get; set; }


        private EffectParameter mDiffuseTexParam;
        private EffectParameter mTexEnabledParam;


        public DeferredObjectEffect()
            : base(LoadShaderBytecode("Shaders\\deferred\\gbuffer"))
        {
            this.mDiffuseTexParam = this.Effect.Parameters["mAlbedo"];
            this.mTexEnabledParam = this.Effect.Parameters["mTextureEnabled"];
        }

        protected override void OnApply()
        {
            this.mDiffuseTexParam.SetValue(this.DiffuseTexture);
            this.mTexEnabledParam.SetValue(this.TextureEnabled);
        }
    }
}
