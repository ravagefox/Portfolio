// Source: PhysicsWorld
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

using System.Collections.Generic;
using System.Linq;
using Engine.Core;
using Engine.Data;
using Engine.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Wargame.Data.Gos;
using Wargame.Data.Gos.Components;
using Wargame.Data.Math;

namespace Wargame.Data.Physics
{
    public sealed class PhysicsWorld : WorldObject
    {
        public IEnumerable<WorldObject> Objects { get; set; }

        public BoundingBox WorldSpaceBounds
        {
            get
            {
                var t = this.Transform;
                var scale = t.Scale;

                var minX = t.Position.X - (scale.X / 2);
                var minY = t.Position.Y - (scale.Y / 2);
                var minZ = t.Position.Z - (scale.Z / 2);

                var maxX = t.Position.X + (scale.X / 2);
                var maxY = t.Position.Y + (scale.Y / 2);
                var maxZ = t.Position.Z + (scale.Z / 2);

                return new BoundingBox(
                    new Vector3(minX, minY, minZ),
                    new Vector3(maxX, maxY, maxZ));
            }
        }

        public BoundingOcttree<WorldObject> WorldOcttree { get; private set; }


        private Vector3 worldPosition;
        private float worldscale;


        public PhysicsWorld(
            Vector3 position,
            float scale) : base()
        {
            this.worldPosition = position;
            this.worldscale = scale;
        }

        public override void Initialize()
        {
            base.Initialize();

            this.Transform.UseUniformScale = true;
            this.Transform.SetUniformScale(this.worldscale);

            this.WorldOcttree = new BoundingOcttree<WorldObject>(
                this.worldPosition,
                this.Transform.Scale.X,
                1.0f);
        }

        public override void Update(Time frameTime)
        {
            if (!this.Objects.Any()) { return; }

            var objsInWorldSpace = this.GetWorldSpace(this.Objects);
            this.UpdateWorldPositions(objsInWorldSpace);

            base.Update(frameTime);
        }

        private void UpdateWorldPositions(IEnumerable<WorldObject> objsInWorldSpace)
        {
            objsInWorldSpace.ToList().ForEach(o => this.WorldOcttree.UpdateObject(o));
        }

        private IEnumerable<WorldObject> GetWorldSpace(IEnumerable<WorldObject> objects)
        {
            foreach (var obj in objects)
            {
                if (obj.GetComponent<RigidBody>() is RigidBody rigid &&
                    !((obj is Camera) || (obj is PointLight)))
                {
                    if (this.WorldSpaceBounds.Intersects(this.GetBoundingBox(rigid)))
                    {
                        yield return obj;
                    }
                }
            }
        }

        public IEnumerable<WorldObject> GetColliding(WorldObject obj)
        {
            if (!(obj.GetComponent<RigidBody>() is RigidBody body))
            {
                yield break;
            }

            var bodyBox = this.GetBoundingBox(body);
            if (!this.WorldOcttree.IsColliding(bodyBox)) { yield break; }

            var colliders = this.WorldOcttree.GetColliding(bodyBox);
            if (colliders.Count > 0)
            {
                foreach (var collider in colliders) { yield return collider; }
            }
            else { yield break; }
        }

        private BoundingBox GetBoundingBox(RigidBody rigid)
        {
            return BoundingBox.CreateFromPoints(rigid.OBB.GetCorners());
        }
    }
}
