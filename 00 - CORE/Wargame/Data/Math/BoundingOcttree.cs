// Source: BoundingOctree
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
using Microsoft.Xna.Framework;
using Wargame.Extensions;

namespace Wargame.Data.Math
{


    public sealed partial class BoundingOcttree<TObject>
        where TObject : GameObject, IBoundContainer
    {
        private sealed class ObjectUpdateInfo
        {
            public TObject Object { get; }
            public BoundingBox NodeBounds { get; set; }
            public Node CurrentNode { get; internal set; }

            public ObjectUpdateInfo(TObject obj, BoundingBox bounds)
            {
                this.Object = obj;
                this.NodeBounds = bounds;
            }

        }

        public BoundingBox Bounds { get; }

        public int Count => this.objectsById.Count;


        private Node rootNode;
        private Dictionary<uint, ObjectUpdateInfo> objectsById;
        private float looseness;
        private float scale;
        private float minSize;


        public BoundingOcttree(Vector3 position, float scale, float looseness)
        {
            this.objectsById = new Dictionary<uint, ObjectUpdateInfo>();
            var min = position - (Vector3.One * (scale / 2));
            var max = position + (Vector3.One * (scale / 2));

            this.Bounds = new BoundingBox(min, max);

            this.looseness = MathUtil.Clamp(looseness, 1.0f, 2.0f);
            this.scale = scale;
            this.minSize = 1.0f;

            this.rootNode = new Node(scale, this.minSize, looseness, position);
        }


        public void Add(TObject obj)
        {
            var id = obj.Id.HighId;
            if (this.objectsById.ContainsKey(id))
            {
                return;
            }


            var count = 0;
            while (!this.rootNode.Add(obj, obj.BoundingBox))
            {
                this.Grow(obj.BoundingBox.GetCenter() - this.rootNode.Center);
                if (++count > 20) { return; }
            }

            var objBounds = obj.BoundingBox;
            this.objectsById[id] = new ObjectUpdateInfo(obj, this.rootNode.Bounds);
            this.objectsById[id].CurrentNode = this.rootNode.GetCollidingNode(ref objBounds);
        }

        public bool Remove(TObject obj)
        {
            var removed = this.rootNode.Remove(obj);
            if (removed && this.objectsById.Remove(obj.Id.HighId))
            {
                this.Shrink();
            }
            return removed;
        }

        public bool IsColliding(Ray ray, float maxDistance)
        {
            return this.rootNode.IsColliding(ref ray, maxDistance);
        }

        public bool IsColliding(BoundingBox checkBounds)
        {
            return this.rootNode.IsColliding(ref checkBounds);
        }

        public List<TObject> GetColliding(BoundingBox checkBounds)
        {
            var lst = new List<TObject>();
            this.rootNode.GetColliding(ref checkBounds, lst);
            return lst;
        }

        public List<TObject> GetColliding(Ray ray, float maxDistance)
        {
            var lst = new List<TObject>();
            this.rootNode.GetColliding(ref ray, lst, maxDistance);
            return lst;
        }

        private void Shrink()
        {
            this.rootNode = this.rootNode.ShrinkIfPossible(this.scale);
        }

        private void Grow(Vector3 dir)
        {
            var xDirection = dir.X >= 0 ? 1 : -1;
            var yDirection = dir.Y >= 0 ? 1 : -1;
            var zDirection = dir.Z >= 0 ? 1 : -1;

            var oldRoot = this.rootNode;
            var half = this.rootNode.BaseLength / 2;
            var newLength = this.rootNode.BaseLength * 2;
            var newCenter = this.rootNode.Center + new Vector3(xDirection * half, yDirection * half, zDirection * half);

            this.rootNode = new Node(newLength, this.minSize, this.looseness, newCenter);

            if (oldRoot.HasAnyObjects())
            {
                var rootPos = this.rootNode.BestFitChild(oldRoot.Center);
                var children = new Node[BoundingBox.CornerCount];
                for (var i = 0; i < BoundingBox.CornerCount; i++)
                {
                    if (i == rootPos) { children[i] = oldRoot; }
                    else
                    {
                        xDirection = i % 2 == 0 ? -1 : 1;
                        yDirection = i > 3 ? -1 : 1;
                        zDirection = (i < 2 || (i > 3 && i < 6)) ? -1 : 1;
                        children[i] = new Node(
                            oldRoot.BaseLength,
                            this.minSize,
                            this.looseness,
                            newCenter + new Vector3(xDirection * half, yDirection * half, zDirection * half));
                    }
                }

                this.rootNode.SetChildren(children);
            }
        }


        /// <summary>
        /// Updates the object inside the bounding tree space.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns>Returns whether a new node was discovered for the object.</returns>
        public bool UpdateObject(TObject obj)
        {
            var objBounds = obj.BoundingBox;
            if (!this.Bounds.Intersects(objBounds))
            {
                if (this.objectsById.ContainsKey(obj.Id.HighId))
                {
                    this.Remove(obj);
                }

                return false;
            }

            if (this.objectsById.TryGetValue(obj.Id.HighId, out var updateInfo))
            {
                if (this.rootNode.GetCollidingNode(ref objBounds) is Node newNode && updateInfo.CurrentNode != newNode)
                {
                    if (updateInfo.CurrentNode.Remove(obj) &&
                        newNode.Add(obj, objBounds))
                    {
                        updateInfo.CurrentNode = newNode;
                        return true;
                    }
                }
            }

            return false;
        }


        public bool Contains(TObject obj, out BoundingBox newNodeBounds)
        {
            var invalid = new BoundingBox(
                Vector3.One * float.PositiveInfinity,
                Vector3.One * float.NegativeInfinity);
            newNodeBounds = invalid;

            var bb = obj.BoundingBox;
            var colliding = this.rootNode.GetCollidingNode(ref bb);
            if (colliding != null)
            {
                var bounds = colliding.Bounds;
                newNodeBounds = bounds;

                return true;
            }

            return false;
        }

        public List<BoundingBox> GetAllBounds()
        {
            var lst = new List<BoundingBox>
            {
                this.Bounds
            };

            if (this.rootNode.HasAnyObjects())
            {
                var children = this.rootNode.GetChildren();
                for (var i = 0; i < children.Length; i++)
                {
                    lst.AddRange(children[i].GetChildren().Select(x => x.Bounds));
                }
            }

            return lst;
        }
    }
}
