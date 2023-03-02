// Source: TerrainEffect
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

using Engine.Assets;
using Engine.Data;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Client.Data.Graphics.Shaders
{
    public class TerrainEffect : Effect
    {
        public Texture2D GrassTexture
        {
            get => base.Parameters["grassTexture"].GetValueTexture2D();
            set => base.Parameters["grassTexture"].SetValue(value);
        }

        public Texture2D SlopeTexture
        {
            get => base.Parameters["slopeTexture"].GetValueTexture2D();
            set => base.Parameters["slopeTexture"].SetValue(value);
        }

        public Texture2D RockTexture
        {
            get => base.Parameters["rockTexture"].GetValueTexture2D();
            set => base.Parameters["rockTexture"].SetValue(value);
        }

        public Color AmbientColor
        {
            get => new Color(new Vector4(base.Parameters["ambientColor"].GetValueVector3(), 1.0f));
            set => base.Parameters["ambientColor"].SetValue(value.ToVector4());
        }

        public Color DiffuseColor
        {
            get => new Color(new Vector4(base.Parameters["diffuseColor"].GetValueVector3(), 1.0f));
            set => base.Parameters["diffuseColor"].SetValue(value.ToVector4());
        }

        public Vector3 LightDirection
        {
            get => base.Parameters["lightDirection"].GetValueVector3();
            set => base.Parameters["lightDirection"].SetValue(value);
        }

        public Matrix World
        {
            get => base.Parameters["worldMatrix"].GetValueMatrix();
            set => base.Parameters["worldMatrix"].SetValue(value);
        }

        public Matrix View
        {
            get => base.Parameters["viewMatrix"].GetValueMatrix();
            set => base.Parameters["viewMatrix"].SetValue(value);
        }
        public Matrix Projection
        {
            get => base.Parameters["projectionMatrix"].GetValueMatrix();
            set => base.Parameters["projectionMatrix"].SetValue(value);
        }


        public TerrainEffect() :
            base(LoadEffectBytes("Shaders\\terrain.hlsl"))
        {
        }

        public TerrainEffect(GraphicsDevice graphicsDevice, byte[] effectCode, int index, int count) : base(graphicsDevice, effectCode, index, count)
        {
        }

        protected TerrainEffect(Effect cloneSource) : base(cloneSource)
        {
        }

        private static Effect LoadEffectBytes(string v)
        {
            var loader = ServiceProvider.Instance.GetService<ILoader>();
            return loader.Load<Effect>(v);
        }
    }
}
