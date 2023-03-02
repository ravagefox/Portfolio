// Source: GradientEffect
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
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Client.Data.Graphics.Shaders
{
    public class GradientEffect : Shader
    {
        public Color StartColor { get; set; }

        public Color EndColor { get; set; }

        public float Alpha { get; set; }

        public float Position { get; set; }


        private EffectParameter m_alphaParam;
        private EffectParameter m_startColorParam;
        private EffectParameter m_endColorParam;
        private EffectParameter m_posParam;


        public GradientEffect()
            : base(LoadShaderBytecode("Shaders\\Gradient"))
        {
            this.m_alphaParam = this.Effect.Parameters["mAlpha"];
            this.m_posParam = this.Effect.Parameters["mPos"];
            this.m_endColorParam = this.Effect.Parameters["mEndColor"];
            this.m_startColorParam = this.Effect.Parameters["mStartColor"];
        }

        protected override void OnApply()
        {
            this.m_startColorParam?.SetValue(this.StartColor.ToVector3());
            this.m_endColorParam?.SetValue(this.EndColor.ToVector3());
            this.m_alphaParam?.SetValue(this.Alpha);
            this.m_posParam?.SetValue(this.Position);
        }
    }
}
