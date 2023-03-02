// Source: Skybox
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

using Client.Data.Gos.Components;
using Client.Data.Graphics.Shaders;
using Engine.Core;
using Engine.Data;
using Engine.Graphics;
using Engine.Graphics.Linq;
using Fallen.Common;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Client.Data.Gos
{
    public sealed class Skybox : DynamicWorldObject
    {
        public ICamera Camera { get; set; }

        private GradientEffect gradientEffect;
        private Texture2D skyTexture;
        private ModelRenderer sphereRenderer;
        private DiffuseEffect skyboxEffect;


        public Skybox()
            : base(uint.MaxValue, WorldObjectType.None)
        {
            this.sphereRenderer = new ModelRenderer();
        }

        public override void Initialize()
        {
            this.AddComponent(this.sphereRenderer);
            this.sphereRenderer.TextureRelativeDirectory = "Textures\\Skybox\\";
            this.sphereRenderer.AssetPath = "Doodads\\SkySphere";

            this.skyboxEffect = new DiffuseEffect();
            //this.gradientEffect = new GradientEffect();
            this.skyTexture = this.Generate(1, 512);

            base.Initialize();
        }

        private Texture2D Generate(int x, int y)
        {
            var g = ServiceProvider.Instance.GetService<IGraphicsDeviceService>()
                .GraphicsDevice;

            var texture = new Texture2D(g, x, y, false, SurfaceFormat.Color);
            var colors = new Color[x * y];
            for (var i = 0; i < colors.Length; i++)
            {
                colors[i] = Color.White;
            }

            texture.SetData(colors);
            return texture;
        }

        public override void Update(Time frameTime)
        {
            this.Transform.SetLocation(this.Camera.CameraEye + Vector3.Up);

            base.Update(frameTime);
        }

        public void RenderSkybox(Time frameTime)
        {
            this.sphereRenderer.GraphicsDevice.SetRasterizerState(RasterizerState.CullClockwise, out var oldState);

            foreach (var modelMesh in this.sphereRenderer.Model.Meshes)
            {
                this.skyboxEffect.World = this.Transform.GetWorld();
                this.skyboxEffect.View = this.Camera.View;
                this.skyboxEffect.Projection = this.Camera.Projection;

                if (modelMesh.Tag is M2XTextureTagData textureData)
                {
                    this.skyboxEffect.TextureEnabled =
                        textureData.HasTextures && textureData.TextureEnabled;

                    if (textureData.HasTextures)
                    {
                        // Index implies that the first is a diffuse texture contained in the texture map
                        this.skyboxEffect.Texture =
                            textureData.LoadTexture(this.sphereRenderer.TextureRelativeDirectory + "\\" + textureData.TexturePaths[0]);
                    }
                }

                this.skyboxEffect.Apply(this.skyboxEffect.GetCurrentTechnique());
                this.sphereRenderer.Render(modelMesh);
            }

            this.sphereRenderer.GraphicsDevice.SetRasterizerState(oldState, out _);
        }
    }
}
