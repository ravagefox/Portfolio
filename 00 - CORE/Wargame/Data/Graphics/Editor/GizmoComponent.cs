// Source: GizmoComponent
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

using Engine.Core;
using Engine.Graphics;
using Engine.Graphics.Data.IO;
using Microsoft.Xna.Framework;
using Wargame.Data.Gos.Components;
using Wargame.Data.Math;
using Wargame.Extensions;

namespace Wargame.Data.Graphics.Editor
{
    public sealed class GizmoComponent : GameObjectComponent
    {
        public BoundingBox Bounds
        {
            get
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
        }

        public BoundingOrientedBox OBB
        {
            get
            {
                var center = this.GameObject.GetComponent<Transform>().Position;
                var halfExtents = (this.Bounds.Max - this.Bounds.Min) / 2;
                var rotation = this.GameObject.GetComponent<Transform>().Rotation;

                return new BoundingOrientedBox(center, halfExtents, rotation);
            }
        }


        public void RenderGizmo(DebugDraw debugDraw)
        {
            if (this.GameObject.GetComponent<Transform>() is Transform transform)
            {
                var scaleCopy = transform.Scale;
                transform.SetUniformScale(scaleCopy.X);

                debugDraw.Draw3DAxis(transform.Position, transform.Scale.X);

                transform.UseUniformScale = false;
                transform.Scale = scaleCopy;
            }
        }




        public override void Initialize()
        {
        }

        public override void Update(Time gameTime)
        {
        }
    }
}
