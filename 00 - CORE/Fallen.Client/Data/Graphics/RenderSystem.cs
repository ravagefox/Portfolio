// Source: WorldFrame
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
using System.Linq;
using Client.Data.Gos;
using Client.Data.Gos.Components;
using Engine.Core;
using Engine.Data;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Client.Data.Graphics
{
    public sealed class RenderSystem : IDisposable
    {
        public GraphicsDevice GraphicsDevice =>
            ServiceProvider.Instance.GetService<IGraphicsDeviceService>()
            .GraphicsDevice;


        private RenderTarget2D mAlbedoTarget;
        private BasicEffect basicEffect;


        public RenderSystem()
        {
            this.basicEffect = new BasicEffect(this.GraphicsDevice);
            this.mAlbedoTarget = new RenderTarget2D(
                this.GraphicsDevice,
                this.GraphicsDevice.PresentationParameters.BackBufferWidth,
                this.GraphicsDevice.PresentationParameters.BackBufferHeight,
                false,
                SurfaceFormat.Color,
                DepthFormat.Depth24Stencil8);
        }

        public void Begin()
        {
            this.GraphicsDevice.SetRenderTarget(this.mAlbedoTarget);
            this.GraphicsDevice.Clear(Color.TransparentBlack);
        }

        public void RenderGameObjects(GameObjectCollection objs)
        {
            foreach (var obj in objs.OfType<DynamicWorldObject>())
            {
                var modelRenderer = obj.GetComponent<ModelRenderer>();
                if (modelRenderer.IsVisible && modelRenderer.Model != null)
                {
                    this.RenderModel(obj.Transform, modelRenderer);
                }
            }
        }

        private void RenderModel(Transform transform, ModelRenderer modelRenderer)
        {
            foreach (var modelMesh in modelRenderer.Model.Meshes)
            {
                foreach (BasicEffect e in modelMesh.Effects)
                {
                    e.View = Camera.Current.View;
                    e.Projection = Camera.Current.Projection;
                    e.World = transform.GetWorld();
                    e.Texture = modelRenderer.Texture;
                    e.TextureEnabled = e.Texture != null;
                    e.DiffuseColor = Color.White.ToVector3();

                    e.EnableDefaultLighting();
                }

                modelMesh.Draw();
            }
        }

        public Texture2D End()
        {
            this.GraphicsDevice.SetRenderTarget(null);
            return this.mAlbedoTarget;
        }

        public void Dispose()
        {
            this.basicEffect?.Dispose();
            this.basicEffect = null;

            this.mAlbedoTarget?.Dispose();
            this.mAlbedoTarget = null;
        }
    }
}