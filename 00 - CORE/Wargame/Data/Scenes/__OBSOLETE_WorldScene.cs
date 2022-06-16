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

using System.Collections.Generic;
using System.Linq;
using Engine.Core;
using Engine.Data;
using Engine.Graphics;
using Engine.Graphics.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Wargame.Data.Gos;
using Wargame.Data.Gos.CameraTypes;
using Wargame.Data.Gos.Components;
using Wargame.Data.Gos.WeaponAssets;
using Wargame.Data.Graphics;
using Wargame.Data.Graphics.Shaders;

namespace Wargame.Data.Scenes
{
    public sealed class WorldScene : Scene
    {
        public GraphicsDevice GraphicsDevice =>
            ServiceProvider.Instance.GetService<IGraphicsDeviceService>()
            .GraphicsDevice;

        public DeferredRenderContext DeferredContext { get; }

        public CascadeShadowContext ShadowContext { get; }

        public PointLightContext PointLightContext { get; }

        public SsaoRenderContext SsaoContext { get; }


        private SpriteBatch spriteBatch;
        private DebugDraw debugDraw;
        private Camera firstPersonCamera;

        private DeferredObject cube, plane;

        private FinalCombineEffect combineEffect;
        private ScreenQuad combineQuad;

        private WeaponBase testWeapon;



        public WorldScene() : base()
        {
            this.DeferredContext = new DeferredRenderContext(this.GraphicsDevice);
            this.PointLightContext = new PointLightContext(this.DeferredContext);

            this.ShadowContext = new CascadeShadowContext(this.GraphicsDevice);
            this.SsaoContext = new SsaoRenderContext(this.DeferredContext);

            this.debugDraw = new DebugDraw(this.GraphicsDevice);
            this.spriteBatch = new SpriteBatch(this.GraphicsDevice);
            this.combineQuad = new ScreenQuad();
        }

        protected override void Load()
        {
            this.combineEffect = new FinalCombineEffect();
            this.combineQuad = new ScreenQuad();
            this.firstPersonCamera = new FirstPersonCamera()
            {
                FOVAngle = 84.4f,
                FarDistance = 100.0f,
                IsMouseCentered = true,
            };
            this.firstPersonCamera.SetCurrent();
            this.firstPersonCamera.Transform.SetLocation(Vector3.Up * 2);

            this.cube = new DeferredObject();
            this.plane = new DeferredObject();

            this.DeferredContext.InitializeRenderTargets();

            this.cube.SetModel("Models\\box");
            this.cube.SetTexture("Textures\\missing");

            this.plane.SetModel("Models\\plane");
            this.plane.SetTexture("Textures\\floor_wood");

            this.plane.IsShadowCaster = false;
            this.plane.CanReceiveShadows = true;

            this.Actors.Add(this.firstPersonCamera);
            this.Actors.Add(this.cube);
            this.Actors.Add(this.plane);

            this.testWeapon = new WeaponBase();
            this.testWeapon.Initialize();
            var transform = this.testWeapon.GetComponent<Transform>();
            transform.SetRotation(-55.0f, 0.0f, 0.0f);
            transform.UseUniformScale = false;
            transform.SetScale(new Vector3(0.025f, 0.035f, 0.035f));

            this.testWeapon.ShadowContext = this.ShadowContext;
            this.testWeapon.LoadModel("Models\\m4a1");

            var light0 = new PointLight(this.PointLightContext);
            light0.Transform.SetLocation(Vector3.Up * 2);
            light0.Radius = 5.0f;
            light0.LightColor = Color.Blue;
            this.PointLightContext.Add(light0);

            base.Load();
        }

        protected override void Update(Time frameTime)
        {
            this.testWeapon.Update(frameTime);
            this.ShadowContext.AnimateLight(frameTime);
            base.Update(frameTime);
        }

        protected override void Render(Time frameTime)
        {
            if (Camera.Current == null) { return; }

            this.ShadowContext.RenderShadowMaps(Camera.Current, this.Actors);
            this.ShadowContext.RenderScene(Camera.Current, this.Actors);

            this.PointLightContext.Begin();
            this.PointLightContext.DrawLights(Camera.Current);
            this.PointLightContext.End();

            this.DeferredContext.Begin();
            this.DeferredContext.RenderToGBuffer(Camera.Current, this.Actors);
            this.DeferredContext.End();


            this.SsaoContext.Render();


            this.debugDraw.Begin(Camera.Current.View, Camera.Current.Projection);
            this.debugDraw.DrawWireGrid(Vector3.Left, Vector3.Up, Vector3.Zero, 10.0f, Color.LightGray);
            this.debugDraw.End();


            this.GraphicsDevice.SetRasterizerState(RasterizerState.CullNone, out var oldRaster);
            this.combineEffect.ColorTarget = this.DeferredContext.FirstPassBindings[0].RenderTarget as Texture2D;
            this.combineEffect.SsaoTarget = this.SsaoContext.SsaoTarget;
            this.combineEffect.ShadowTarget = this.ShadowContext.ShadowScene;
            this.combineEffect.Apply();
            this.combineQuad.Render();
            this.GraphicsDevice.SetRasterizerState(oldRaster, out _);


            this.spriteBatch.Begin();
            this.VisualizeRenderTargets(this.DeferredContext.FirstPassBindings.Select(x => x.RenderTarget).Cast<Texture2D>().Concat(
                new[]
                {
                    this.ShadowContext.ShadowMap,
                    this.ShadowContext.ShadowScene,
                    this.SsaoContext.SsaoTarget,

                    this.PointLightContext.LightTexture,
                }));

            this.spriteBatch.End();

            //this.testWeapon.Render(frameTime);
        }

        private void VisualizeRenderTargets(IEnumerable<Texture2D> renderTargets)
        {
            var size = 100;
            var rect = new Rectangle(0, 0, size, size);

            var idx = 0;
            foreach (var target in renderTargets)
            {
                rect.X = idx++ * size;
                this.spriteBatch.Draw(target, rect, Color.White);
            }
        }
    }
}
