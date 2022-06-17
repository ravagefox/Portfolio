// Source: PointLight
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
using Wargame.Data.IO.Map;

namespace Wargame.Data.Gos
{
    public sealed class PointLight : WorldObject
    {
        public Color LightColor { get; set; }

        public float Radius { get; set; }

        public float Intensity { get; internal set; }


        public BoundingSphere BoundingSphere => new BoundingSphere(this.Transform.Position, this.Radius);


        public PointLight() : base()
        {
            this.Transform.UseUniformScale = true;
            this.Intensity = 1.0f;
        }


        protected override void OnDeserialize(object sender, MapReader reader)
        {
            this.Intensity = reader.ReadSingle();
            this.Radius = reader.ReadSingle();
            this.LightColor = reader.ReadColor();

            base.OnDeserialize(sender, reader);
        }

        protected override void OnSerialize(object sender, MapWriter writer)
        {
            writer.Write(this.Intensity);
            writer.Write(this.Radius);
            writer.Write(this.LightColor);

            base.OnSerialize(sender, writer);
        }
    }
}
