// Source: ModelRenderer
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
using Engine.Data;
using Engine.Graphics.Data.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Wargame.Data.Gos.Components
{
    public sealed class ModelRenderer : GameObjectComponent
    {
        public GraphicsDevice GraphicsDevice =>
            ServiceProvider.Instance.GetService<IGraphicsDeviceService>()
            .GraphicsDevice;

        public Model Model { get; set; }
        public Texture2D Texture { get; set; }
        public bool IsVisible { get; set; } = true;
        public bool IsShadowCaster { get; set; } = true;
        public bool CanReceiveShadows { get; set; } = true;

        public override void Initialize()
        {
        }

        public override void Update(Time gameTime)
        {
        }

        public bool IsInViewSpace(BoundingFrustum frustum)
        {
            if (this.Model.Tag is ModelTagData tagData)
            {
                var transform = this.GameObject.GetComponent<Transform>();
                var scaledBounds = new BoundingBox(
                    tagData.Bounds.Min * transform.Scale,
                    tagData.Bounds.Max * transform.Scale);

                return frustum.Intersects(scaledBounds);
            }

            return false;
        }

        public void Render(ModelMesh mesh)
        {
            foreach (var modelPart in mesh.MeshParts)
            {
                this.GraphicsDevice.SetVertexBuffer(modelPart.VertexBuffer);
                this.GraphicsDevice.Indices = modelPart.IndexBuffer;
                this.GraphicsDevice.DrawIndexedPrimitives(
                    PrimitiveType.TriangleList,
                    modelPart.VertexOffset,
                    modelPart.StartIndex,
                    modelPart.PrimitiveCount);
            }
        }

        public Matrix[] GetBoneTransforms()
        {
            var boneTransforms = new Matrix[this.Model.Bones.Count];
            if (boneTransforms.Length > 0)
            {
                this.Model.CopyAbsoluteBoneTransformsTo(boneTransforms);
            }
            return boneTransforms;
        }

        public int CalculateMipmapLevel()
        {
            if (this.Texture.LevelCount == 1) { return 0; }

            var transform = this.GameObject.GetComponent<Transform>();

            var mipmapLevels = this.Texture.LevelCount;
            var distances = new float[mipmapLevels];
            for (var i = 0; i < distances.Length; i++)
            {
                var farClip = -Camera.Current.GetBoundingFrustum().Far.D;
                var f = farClip / (i + 1);

                distances[i] = f;

                var camVector = transform.Position - Camera.Current.Transform.Position;
                var lengthSquared = camVector.LengthSquared();

                if (lengthSquared < distances[i]) { return i; }
            }

            return 0;
        }
    }
}