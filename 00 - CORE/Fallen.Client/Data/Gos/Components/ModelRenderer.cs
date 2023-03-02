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

using System;
using System.IO;
using Engine.Assets;
using Engine.Core;
using Engine.Data;
using Engine.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Client.Data.Gos.Components
{
    public sealed class ModelRenderer :
        GameObjectComponent,
        Engine.Graphics.IDrawable
    {
        #region Properties
        public bool Visible
        {
            get => this.m_visible;
            set
            {
                if (this.m_visible != value)
                {
                    this.m_visible = value;
                    VisibleChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public int DrawOrder { get; set; }

        public string AssetPath
        {
            get => this.m_assetPath;
            set
            {
                if (this.m_assetPath != value)
                {
                    this.pendingChanges = true;
                    this.m_assetPath = value;
                }
            }
        }

        public Model Model { get; private set; }

        public string TextureRelativeDirectory { get; set; }

        public GraphicsDevice GraphicsDevice =>
            ServiceProvider.Instance.GetService<IGraphicsDeviceService>()
            .GraphicsDevice;

        #endregion

        #region Fields

        public event EventHandler VisibleChanged;

        private bool m_visible;
        private bool pendingChanges;
        private string m_assetPath;
        #endregion



        public Matrix[] GetBoneTransforms()
        {
            var boneTransforms = new Matrix[this.Model.Bones.Count];
            if (boneTransforms.Length > 0)
            {
                this.Model.CopyAbsoluteBoneTransformsTo(boneTransforms);
            }

            return boneTransforms;
        }

        public bool IsInCameraView(BoundingFrustum frustum)
        {
            if (this.Model == null) { return false; }
            var transform = this.GameObject.GetComponent<Transform>();

            BoundingBox bounds;
            Vector3 min, max;
            if (this.Model.Tag is M2XMeshTagData tagData)
            {
                min = tagData.Bounds.Min;
                max = tagData.Bounds.Max;
            }
            else
            {
                min = Vector3.One * (-transform.Scale / 2);
                max = Vector3.One * (transform.Scale / 2);
            }

            bounds = new BoundingBox(
                transform.Position + min,
                transform.Position + max);

            return frustum.Intersects(bounds);
        }

        public void Render(ModelMesh mesh)
        {
            var gd = ServiceProvider.Instance.GetService<IGraphicsDeviceService>()
                .GraphicsDevice;

            foreach (var part in mesh.MeshParts)
            {
                gd.SetVertexBuffer(part.VertexBuffer);
                gd.Indices = part.IndexBuffer;
                gd.DrawIndexedPrimitives(
                    PrimitiveType.TriangleList,
                    part.VertexOffset,
                    part.StartIndex,
                    part.PrimitiveCount);
            }
        }

        public void Apply(Time frameTime)
        {
        }

        public override void Initialize()
        {
        }

        public override void Update(Time gameTime)
        {
            if (this.pendingChanges)
            {
                var content = ServiceProvider.Instance.GetService<ILoader>();
                this.Model = content.Load<Model>(this.m_assetPath);

                this.pendingChanges = false;
            }

            if (string.IsNullOrEmpty(this.TextureRelativeDirectory) &&
                !string.IsNullOrEmpty(this.m_assetPath))
            {
                this.TextureRelativeDirectory = Path.GetDirectoryName(this.m_assetPath);
            }
        }
    }
}
