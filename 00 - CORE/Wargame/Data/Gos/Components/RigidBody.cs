// Source: RigidBody
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

using System.Linq;
using Engine.Core;
using Engine.Graphics.Data.IO;
using Microsoft.Xna.Framework;
using Wargame.Data.Math;
using Wargame.Extensions;

namespace Wargame.Data.Gos.Components
{
    public sealed class RigidBody : GameObjectComponent
    {
        public BoundingBox Bounds => this.GetBounds();
        public BoundingOrientedBox OBB => this.GetObb();

        public float Mass { get; set; } = -1.0f;

        private BoundingBox GetBounds()
        {
            var min = Vector3.One * float.MaxValue;
            var max = Vector3.One * float.MinValue;

            var box = new BoundingBox(min, max);

            if (this.GameObject.GetComponent<ModelRenderer>() is ModelRenderer r)
            {
                if (r.Model != null && r.Model.Tag is ModelTagData tagData)
                {
                    box = tagData.Bounds;
                    box = box.Transform(this.GameObject.GetComponent<Transform>());
                }
            }

            return box;
        }

        private BoundingOrientedBox GetObb()
        {
            var center = this.GameObject.GetComponent<Transform>().Position;
            var halfExtents = (this.Bounds.Max - this.Bounds.Min) / 2;
            var rotation = this.GameObject.GetComponent<Transform>().Rotation;

            return new BoundingOrientedBox(center, halfExtents, rotation);
        }


        public override void Initialize()
        {
            if (this.GameObject.GetComponent<ModelRenderer>() is ModelRenderer renderer)
            {

            }
        }

        public override void Update(Time gameTime)
        {
        }
    }
}
