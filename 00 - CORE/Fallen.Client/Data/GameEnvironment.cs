// Source: GameEnvironment
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
using System.Windows.Forms;
using Client.Data.Managers;
using Client.Data.Scenes;
using Engine.Assets;
using Engine.Core.Input;
using Engine.Data;
using Engine.Data.Scripting.Lua;
using Engine.Graphics;

namespace Client.Data
{
    public sealed class GameEnvironment : IDisposable
    {

        private GraphicsDeviceControl deviceControl;
        private Form gameWindow;
        private AppProfile profile;
        private Keyboard keyboard;
        private Mouse mouse;
        private LuaApiMaster luaApiMaster;
        private List<ManagerService> inClientServices;


        public GameEnvironment(string rootDirectory)
        {
            this.profile = new AppProfile(rootDirectory);

            this.inClientServices = new List<ManagerService>()
            {
                new AssetManager(),
                new AddonManager(),
                new NetworkManager(),
                new ConfigManager(),
                new RenderManager(),
            };

            GameSettings.ChangesApplied += this.GameSettings_ChangesApplied;
        }

        public Form CreateWindow()
        {
            if (this.gameWindow != null) { return this.gameWindow; }

            this.gameWindow = new Form()
            {
                Width = 1280,
                Height = 720,
                StartPosition = FormStartPosition.CenterScreen,
                FormBorderStyle = FormBorderStyle.Sizable,
                WindowState = FormWindowState.Normal,
            };

            this.deviceControl = new GraphicsDeviceControl();
            this.deviceControl.Dock = DockStyle.Fill;
            this.gameWindow.Controls.Add(this.deviceControl);

            return this.gameWindow;
        }


        public void InitializeServices()
        {
            ServiceProvider.Instance.AddService<AppProfile>(this.profile);
            this.inClientServices.ForEach(c => c.Initialize());

            this.luaApiMaster = new LuaApiMaster();
            this.keyboard = new Keyboard(this.deviceControl);
            this.mouse = new Mouse(this.deviceControl);

            ServiceProvider.Instance.AddService<Keyboard>(this.keyboard);
            ServiceProvider.Instance.AddService<Mouse>(this.mouse);
            ServiceProvider.Instance.AddService<LuaApiMaster>(this.luaApiMaster);

            this.keyboard.ReadKeyBindings(
                this.profile.GetRelativePath("KeyBindings.wtf"));
            this.luaApiMaster.ImportModules(
                this.profile.GetRelativePath("Interface\\Modules"));
            this.luaApiMaster.RegisterTypes();

#if DEBUG
            var scene = Environment.GetCommandLineArgs()
                        .Any(x => x.ToLower().SequenceEqual("--debug")) ?
                        (GfxScene)new GfxDebugScene() : new GfxMenu();

            this.deviceControl.LoadScene(scene);

#elif !DEBUG
            var scene = new GfxMenu();
            this.deviceControl.LoadScene(scene);
#endif
        }

        public void Dispose()
        {
            this.deviceControl?.Dispose();
            this.gameWindow?.Controls.Remove(this.deviceControl);
            this.deviceControl = null;

            this.inClientServices?.ForEach(c => c?.Dispose());
            this.inClientServices.Clear();
        }

        private void GameSettings_ChangesApplied(object sender, EventArgs e)
        {
            this.deviceControl.VSync = GameSettings.IsVSync;
            this.deviceControl.SetResolution(
                GameSettings.ResolutionX, GameSettings.ResolutionY);

            if (GameSettings.IsFullscreen)
            {
                this.gameWindow.FormBorderStyle = FormBorderStyle.None;
                this.gameWindow.WindowState = FormWindowState.Maximized;
            }
            else
            {
                this.gameWindow.FormBorderStyle = FormBorderStyle.Sizable;
                this.gameWindow.WindowState = FormWindowState.Normal;
            }
        }

    }
}
