// Source: MapController
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

using System.Linq;
using Client.Data.Graphics.Shaders;
using Client.Data.IO;
using Engine.Core;
using Engine.Data;
using Engine.Graphics;
using Engine.Graphics.Linq;
using Fallen.Common;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using static Engine.Graphics.Terrain;

namespace Client.Data.Gos.Controllers
{
    public sealed class MapController : ControllerBase<DoodadWorldObject>
    {

        public Texture2D[] TerrainTextures { get; set; }

        public TerrainEffect TerrainShader { get; set; }


        private Terrain terrain;
        private TerrainChunk[] chunks;
        private DebugDraw debugDraw;
        private MapId mapId;

        public MapController(MapId mapId)
        {
            this.mapId = mapId;
        }

        public void LoadMap()
        {
            var assetmanager = ServiceProvider.Instance.GetService<AssetManager>();
            var fileNames = assetmanager.EnumerateDirectory("World", this.mapId.ToString());

            this.terrain = new Terrain(fileNames);
            this.TerrainShader = new TerrainEffect();
            this.TerrainTextures = new Texture2D[3]
                {
                    assetmanager.Load<Texture2D>("Textures\\grass.jpg"),
                    assetmanager.Load<Texture2D>("Textures\\slope.jpg"),
                    assetmanager.Load<Texture2D>("Textures\\stone.jpg"),
                };

            this.terrain.Load((uint)this.mapId);
            this.chunks = this.terrain.GetChunks();

            this.debugDraw = new DebugDraw(this.TerrainShader.GraphicsDevice);
        }

        public void Draw(Time gameTime)
        {
            var projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(84.4f), 1.78f, 0.01f, 1000.0f);
            var cameraFollow = Camera.Current.GetFollowObject();
            var frustum = new BoundingFrustum(Camera.Current.View * projection);

            var visible = this.chunks.Where(chunk => frustum.Intersects(chunk.Bounds));
            if (visible.Any())
            {
                this.TerrainShader.GrassTexture = this.TerrainTextures[0];
                this.TerrainShader.SlopeTexture = this.TerrainTextures[1];
                this.TerrainShader.RockTexture = this.TerrainTextures[2];
                this.TerrainShader.LightDirection = Vector3.Up;
                this.TerrainShader.DiffuseColor = Color.White;
                this.TerrainShader.AmbientColor = Color.White;

                this.debugDraw.Begin(Camera.Current.View, projection);

                var gd = this.TerrainShader.GraphicsDevice;
                gd.SetRasterizerState(RasterizerState.CullClockwise, out var oldRaster);
                gd.SetBlendState(BlendState.Opaque, out var oldBlend);
                gd.SetDepthStencilState(DepthStencilState.DepthRead, out var oldDepth);


                foreach (var chunk in visible)
                {
                    if (chunk == null) { continue; }

                    var chunkPos = new Vector3(chunk.XIndex * (float)chunk.Width,
                                               0.0f,
                                               chunk.YIndex * (float)chunk.Depth
                                               /*(float)chunk.Depth + (chunk.Depth * chunk.YIndex)*/);

                    this.TerrainShader.View = Camera.Current.View;
                    this.TerrainShader.Projection = projection;
                    this.TerrainShader.World =
                        Matrix.CreateTranslation(chunkPos);
                    this.TerrainShader.Techniques[0].Passes[0].Apply();

                    gd.SetVertexBuffer(chunk.VertexBuffer);
                    gd.Indices = chunk.IndexBuffer;
                    gd.DrawIndexedPrimitives(
                        PrimitiveType.TriangleList,
                        0,
                        0,
                        chunk.IndexBuffer.IndexCount / 3);

                    this.debugDraw.DrawWireBox(chunk.Bounds, Color.Red);
                }

                gd.SetRasterizerState(oldRaster, out _);
                gd.SetBlendState(oldBlend, out _);
                gd.SetDepthStencilState(oldDepth, out _);

                this.debugDraw.End();
            }
        }
    }
}
