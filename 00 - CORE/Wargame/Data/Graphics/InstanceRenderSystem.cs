// Source: InstanceRenderSystem
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

using System;
using System.Collections.Generic;
using System.Linq;
using Engine.Graphics.Data.IO;
using Microsoft.Xna.Framework.Graphics;

namespace Wargame.Data.Graphics
{
    public abstract class InstanceRenderSystemBase<TVertex> : RenderSystemBase
        where TVertex : struct, IVertexType
    {
        public Model InstanceModel
        {
            get => this.instanceModel;
            set
            {
                if (this.instanceModel != value)
                {
                    this.instanceModel = value;
                    this.GenerateMeshBuffers();

                    this.OnInstanceModelChanged(this, EventArgs.Empty);
                }
            }
        }

        protected int InstanceCount => this.instanceHashset.Count;


        public event EventHandler InstanceModelChanged;



        private Model instanceModel;
        private VertexBuffer geometryVertexBuffer;
        private IndexBuffer geometryIndexBuffer;


        private HashSet<int> instanceHashset;
        private IEnumerable<TVertex> instanceData;
        private VertexBuffer instanceVertexBuffer;



        protected InstanceRenderSystemBase(GraphicsDevice graphicsDevice) :
            base(graphicsDevice)
        {
            this.instanceHashset = new HashSet<int>();
            this.instanceData = new List<TVertex>();
        }


        protected virtual void OnInstanceModelChanged(object sender, EventArgs e)
        {
            this.InstanceModelChanged?.Invoke(sender, e);
        }

        protected IndexBuffer GetIndexBuffer()
        {
            return this.geometryIndexBuffer;
        }

        protected VertexBufferBinding[] GetVertexBuffers()
        {
            return new VertexBufferBinding[]
            {
                new VertexBufferBinding(this.geometryVertexBuffer, 0, 0),
                new VertexBufferBinding(this.instanceVertexBuffer, 0, 1),
            };
        }

        public void Remove(TVertex instanceData)
        {
            var hashcode = instanceData.GetHashCode();
            if (!this.instanceHashset.Contains(hashcode)) { return; }

            if (this.instanceHashset.Remove(hashcode))
            {
                this.instanceData = this.instanceData.Except(new[] { instanceData });
                this.RebuildInstanceBuffers();
            }
        }

        public void Add(TVertex instanceData)
        {
            var hashcode = instanceData.GetHashCode();
            if (this.instanceHashset.Contains(hashcode)) { return; }

            if (this.instanceHashset.Add(hashcode))
            {
                this.instanceData = this.instanceData.Append(instanceData);
                this.RebuildInstanceBuffers();
            }
        }

        private void GenerateMeshBuffers()
        {
            this.geometryVertexBuffer?.Dispose();
            this.geometryIndexBuffer?.Dispose();

            if (this.instanceModel.Tag is ModelTagData tagData)
            {
                this.geometryIndexBuffer = new IndexBuffer(this.GraphicsDevice, IndexElementSize.ThirtyTwoBits, tagData.Indices.Length, BufferUsage.WriteOnly);
                this.geometryVertexBuffer = new VertexBuffer(this.GraphicsDevice, typeof(VertexPositionNormalTexture), tagData.Vertices.Length, BufferUsage.WriteOnly);

                this.geometryVertexBuffer.SetData(tagData.Vertices.ToArray());
                this.geometryIndexBuffer.SetData(tagData.Indices.ToArray());
            }
            else
            {
                throw new Exception(
                    "The tag assigned to the model must be of typeof(ModelTagData)");
            }
        }

        private void RebuildInstanceBuffers()
        {
            this.instanceVertexBuffer?.Dispose();
            this.instanceVertexBuffer = new VertexBuffer(
                this.GraphicsDevice,
                this.BuildVertexDeclaration(),
                this.InstanceCount,
                BufferUsage.WriteOnly);

            this.instanceVertexBuffer.SetData(this.instanceData.ToArray());
        }

        protected abstract VertexDeclaration BuildVertexDeclaration();
    }
}
