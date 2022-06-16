// Source: WorldObject
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
using Microsoft.Xna.Framework;
using Wargame.Data.Gos.Components;
using Wargame.Data.Graphics.Editor;
using Wargame.Data.IO;
using Wargame.Data.IO.Map;
using Wargame.Data.Math;

namespace Wargame.Data.Gos
{
    public abstract class WorldObject : GameObject, IBoundContainer, IObjectSerialization
    {
        public Transform Transform { get; }

        public virtual BoundingBox BoundingBox =>
            new BoundingBox(
                this.Transform.Position - (this.Transform.Scale / 2),
                this.Transform.Position + (this.Transform.Scale / 2));


        private RigidBody rigidBody;
        private GizmoComponent gizmo;


        public WorldObject() : base()
        {
            this.Transform = new Transform();
            this.rigidBody = new RigidBody();
            this.gizmo = new GizmoComponent();
        }

        public override void Initialize()
        {
            this.AddComponent(this.rigidBody);
            this.AddComponent(this.Transform);
            this.AddComponent(this.gizmo);

            base.Initialize();
        }

        protected virtual void OnDeserialize(object sender, MapReader reader)
        {
        }

        protected virtual void OnSerialize(object sender, MapWriter writer)
        {
        }

        public void Deserialize(MapReader reader)
        {
            this.OnDeserialize(this, reader);
        }

        public void Serialize(MapWriter writer)
        {
            this.OnSerialize(this, writer);
        }
    }
}
