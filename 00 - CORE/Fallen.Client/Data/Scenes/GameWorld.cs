// Source: GameWorld
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
using Client.API;
using Client.Data.Gos;
using Client.Data.Graphics;
using Engine.Core;
using Engine.Data;
using Engine.Data.Scripting.Lua;
using Engine.Graphics.Linq;
using Fallen.Common;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Client.Data.Scenes
{
    public sealed class GameWorld : GfxScene
    {
        public DynamicWorldObject Player { get; set; }


        private AddonManager addonManager;
        private SpriteBatch spriteBatch;
        private NetworkManager networkManager;
        private WorldObjectManager objectManager;

        // Render systems
        private WorldFrame worldFrame;



        public MapId MapId { get; internal set; }
        public Camera Camera { get; internal set; }


        public GameWorld()
        {
            this.objectManager = new WorldObjectManager();
            this.worldFrame = SystemApi.GetInstance<WorldFrame>();
        }

        public WorldObjectManager GetObjectManager()
        {
            return this.objectManager;
        }



        protected override void Load()
        {
            this.spriteBatch = new SpriteBatch(this.GraphicsDevice);

            this.addonManager = UserLuaData.GetInstance<AddonManager>();
            this.AddOns = this.addonManager.LoadAddons();

            this.networkManager = ServiceProvider.Instance.GetService<NetworkManager>();
            this.networkManager.ClientDisconnected += this.NetworkManager_ClientDisconnected;

            base.Load();
        }

        private void NetworkManager_ClientDisconnected(object sender, System.EventArgs e)
        {
            if (ServiceProvider.Instance.GetService<ISceneContainer>() is ISceneContainer container)
            {
                var menu = new GfxMenu();
                container.LoadScene(menu);
            }
        }

        protected override void Unload()
        {
            this.AddOns?.ToList().ForEach(addon => addon.Unload());
            this.objectManager?.Unload();

            this.AddOns = null;
            this.objectManager = null;
            base.Unload();
        }

        protected override void Update(Time frameTime)
        {
            if (this.Camera != null && this.Camera.ViewDistance == 0f)
            {
                this.Camera.ViewDistance = GameSettings.ViewDistance;
            }

            this.Camera?.Update(frameTime);

            this.AddOns?.Where(c => c.Enabled).ToList().ForEach(addon => addon.Update(frameTime));
            this.objectManager.GameObjects.ToList().ForEach(o =>
            {
                if (o.Enabled) { o.Update(frameTime); }
            });
            base.Update(frameTime);
        }

        protected override void Render(Time frameTime)
        {
            this.GraphicsDevice.Clear(this.ClearColor);

            this.worldFrame.RenderGameObjects(this.objectManager.GameObjects);

            this.RenderFullScreen(frameTime);
            base.Render(frameTime);
        }

        private void RenderFullScreen(Time frameTime)
        {
            this.AddOns?.ToList().ForEach(c => c.Apply(frameTime));

            this.spriteBatch.Begin(SpriteSortMode.Deferred);
            this.spriteBatch.Draw(
                this.worldFrame.WorldTexture,
                this.GraphicsDevice.GetBounds(),
                Color.White);

            this.AddOns?.Where(c => c.Visible).ToList().ForEach(
                c =>
                {
                    var g = c.Graphics;
                    this.spriteBatch.Draw(g.GetRenderTarget(), g.GetScreenRect(), Color.White);
                });
            this.spriteBatch.End();
        }

        internal void SetPlayerId(DynamicWorldObject playerId)
        {
            this.Player = playerId;
        }
    }
}
