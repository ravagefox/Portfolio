// Source: WorldScene
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

using System.IO;
using System.Linq;
using Engine.Core;
using Engine.Core.Input;
using Engine.Data;
using Engine.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Wargame.Data.Gos;
using Wargame.Data.Gos.CameraTypes;
using Wargame.Data.Gos.Components;
using Wargame.Data.Graphics;
using Wargame.Data.Graphics.DeferredContext;
using Wargame.Data.Graphics.Shaders;
using Wargame.Data.IO.Map;
using Wargame.Data.IO.Map.Serializers;
using Wargame.Data.Physics;

namespace Wargame.Data.Scenes
{
    public class WorldScene : Scene
    {
        public GraphicsDevice GraphicsDevice =>
            ServiceProvider.Instance.GetService<IGraphicsDeviceService>()
            .GraphicsDevice;

        private AssetManager Content =>
            ServiceProvider.Instance.GetService<AssetManager>();

        public RenderManager RenderMaster { get; }

        public PointLightRenderSystem LightSystem => this.RenderMaster.LightSystem;

        public EditorRenderSystem EditSystem { get; }

        public PhysicsWorld Physics { get; }


        private DeferredObject planeObject, cubeObject;
        private FirstPersonCamera playerCamera;

        private FinalCombineEffect combine;
        private ScreenQuad combineQuad;

        private SpriteBatch spriteBatch;
        private Point resolution;
        private MainWindow window;
        private float globalLightIntensity = 1.0f;

        public WorldScene()
        {
            this.resolution = new Point(2560, 1440);

            this.Physics = new PhysicsWorld(Vector3.Zero, 10.0f);
            this.Physics.Objects = this.Actors.OfType<WorldObject>();

            this.spriteBatch = new SpriteBatch(this.GraphicsDevice);
            this.RenderMaster = new RenderManager(this.GraphicsDevice);
            this.EditSystem = new EditorRenderSystem(this.GraphicsDevice);

            this.planeObject = new DeferredObject();
            this.cubeObject = new DeferredObject();
            this.playerCamera = new FirstPersonCamera();
        }

        protected override void Load()
        {
            this.window = ServiceProvider.Instance.GetService<MainWindow>();
            this.RenderMaster.Resolution = this.resolution;
            this.RenderMaster.Initialize();

            var light0 = new PointLight();
            light0.Transform.SetLocation(Vector3.Forward + Vector3.Up);
            light0.LightColor = Color.Red;
            light0.Radius = 5.0f;
            light0.Intensity = .50f;
            light0.Enabled = true;

            var light1 = new PointLight();
            light1.Transform.SetLocation(Vector3.Backward + Vector3.Right + Vector3.Up);
            light1.LightColor = Color.Blue;
            light1.Radius = 5.0f;
            light1.Intensity = .70f;
            light1.Enabled = true;

            this.Actors.Add(light0);
            this.Actors.Add(light1);

            var planeRenderer = new ModelRenderer()
            {
                Model = this.Content.Load<Model>("Models\\plane"),
                Texture = this.Content.Load<Texture2D>("Textures\\floor_wood"),
                CanReceiveShadows = true,
                IsShadowCaster = false,
            };
            var cubeRenderer = new ModelRenderer()
            {
                Model = this.Content.Load<Model>("Models\\box"),
                Texture = this.Content.Load<Texture2D>("Textures\\missing"),
                IsShadowCaster = true,
                CanReceiveShadows = true,
            };
            this.planeObject.AddComponent(planeRenderer);
            this.cubeObject.AddComponent(cubeRenderer);


            this.cubeObject.Transform.SetRotation(-1.4f, -0.25f, 0.1f);

            this.Actors.Add(this.Physics);
            this.Actors.Add(this.planeObject);
            this.Actors.Add(this.cubeObject);
            this.Actors.Add(this.playerCamera);

            this.playerCamera.IsMouseCentered = true;
            this.playerCamera.FOVAngle = 84.4f;
            this.playerCamera.FarDistance = 100.0f;
            this.playerCamera.Transform.SetLocation(Vector3.Up * 2);
            this.playerCamera.SetCurrent();

            var keyboard = ServiceProvider.Instance.GetService<Keyboard>();
            keyboard.Control.KeyUp += (s, e) =>
            {
                if (e.KeyCode == System.Windows.Forms.Keys.Up)
                {
                    this.RenderMaster.GlobalLightIntensity += 0.1f;
                }
                else if (e.KeyCode == System.Windows.Forms.Keys.Down)
                {
                    this.RenderMaster.GlobalLightIntensity -= 0.1f;
                }

                this.RenderMaster.GlobalLightIntensity = MathHelper.Clamp(this.RenderMaster.GlobalLightIntensity, 0.05f, 1.0f);
            };

            this.EditSystem.Actors = this.Actors.Concat(this.LightSystem.GetLights()).Except(new[] { this.Physics });
            this.EditSystem.Resolution = this.resolution;
            this.EditSystem.InitializeSystem();

            base.Load();

            var sceneSerializer = new SceneSerializer();
            using (var writer = new MapWriter(File.Create("Data\\Maps\\TestMap.mzx")))
            {
                sceneSerializer.Serialize(this, writer);

            }

        }

        protected override void Unload()
        {
            this.RenderMaster?.Dispose();
            this.EditSystem?.Dispose();
            base.Unload();
        }

        protected override void Update(Time frameTime)
        {
            this.Physics.Update(frameTime);
            base.Update(frameTime);
        }

        protected override void Render(Time frameTime)
        {
            this.GraphicsDevice.Clear(
                new Color(0.0f, 0.2f, 0.4f, 1.0f));

            this.RenderMaster.RenderToTexture(this.Actors);

            this.EditSystem.Begin();
            this.EditSystem.DrawHighlighted();
            this.EditSystem.RenderGizmos();
            this.EditSystem.End();

            this.spriteBatch.Begin();
            this.spriteBatch.Draw(
                this.RenderMaster.FullScreenTexture,
                this.RenderMaster.ScreenRect,
                Color.White);
            this.spriteBatch.Draw(
                this.EditSystem.EditTexture,
                this.RenderMaster.ScreenRect,
                Color.White);
            this.spriteBatch.End();

            this.spriteBatch.Begin();
            this.VisualizeRenderTargets(new Texture2D[]
            {
                this.RenderMaster.DeferredSystem.GetTexture(DeferredRenderTarget.Albedo),
                this.RenderMaster.DeferredSystem.GetTexture(DeferredRenderTarget.Normal),
                this.RenderMaster.DeferredSystem.GetTexture(DeferredRenderTarget.Depth),

                this.RenderMaster.ShadowSystem.DepthTexture,
                this.RenderMaster.ShadowSystem.SceneTexture,

                this.LightSystem.LightTexture,
                this.RenderMaster.SSAOSystem.SsaoTexture,
            });
            this.spriteBatch.End();
            base.Render(frameTime);
        }

        private void VisualizeRenderTargets(Texture2D[] texture2Ds)
        {
            if (DebugDraw.DebugMode)
            {
                var size = 100;
                var rect = new Rectangle(0, 0, size, size);

                for (var i = 0; i < texture2Ds.Length; i++)
                {
                    rect.X = (i * size) + 1;
                    this.spriteBatch.Draw(texture2Ds[i], rect, Color.White);

                    this.spriteBatch.DrawRectangle(rect, Color.Red);
                }
            }
        }
    }
}
