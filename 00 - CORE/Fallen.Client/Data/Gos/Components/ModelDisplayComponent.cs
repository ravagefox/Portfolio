// Source: ModelDisplayComponent
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
using Microsoft.Xna.Framework.Graphics;

namespace Client.Data.Gos.Components
{
    public sealed class ModelRenderer : WorldObjectComponent
    {
        public Model Model { get; set; }

        public Texture2D Texture { get; set; }

        public bool UseDefaultCameraMatrices { get; set; }

        public bool DebugMode { get; set; }

        public Matrix Projection { get; set; }

        public Matrix View { get; set; }

        public Matrix World { get; set; }


        public ModelTagData TagData => this.Model?.Tag as ModelTagData;


        private DebugDraw debugDraw;


        public ModelRenderer() : base()
        {
        }


        public override void Initialize()
        {
            this.debugDraw = new DebugDraw(this.WorldObject.GraphicsDevice);
        }

        public override void Update(Time gameTime)
        {
        }

        public override void Apply(Time frameTime)
        {
            if (this.Model != null)
            {
                foreach (var mesh in this.Model.Meshes)
                {
                    foreach (BasicEffect effect in mesh.Effects)
                    {
                        effect.View = this.View;
                        effect.World = this.World;
                        effect.Projection = this.Projection;

                        effect.TextureEnabled = this.Texture != null;
                        effect.Texture = this.Texture;

                        effect.DiffuseColor = Color.White.ToVector3();
                        effect.AmbientLightColor = Color.White.ToVector3();
                    }
                    mesh.Draw();
                }
            }

            if (this.TagData != null)
            {
                this.debugDraw.Begin(Camera.Current.View, Camera.Current.Projection, this.World);
                this.debugDraw.DrawWireBox(this.TagData.Bounds, Color.Red);
                this.debugDraw.End();
            }
            base.Apply(frameTime);
        }

    }
}
