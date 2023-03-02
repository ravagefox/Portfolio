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
using System.Windows.Forms;
using Client.Data;
using Client.Data.Scenes;
using Engine.Assets;
using Engine.Core.Input;
using Engine.Data;
using Engine.Data.IO;
using Engine.Data.Scripting.Lua;
using Engine.Graphics;

namespace Client
{
    public sealed class GameEnvironment : IDisposable
    {
        private Form mainWindow;
        private GraphicsDeviceControl deviceControl;
        private AppProfile profile;
        private WtfConfig defaultConfig;
        private Keyboard keyboard;
        private Mouse mouse;
        private LuaApiMaster apiMaster;
        private AssetManager assetManager;
        private NetworkManager networkManager;


        public GameEnvironment(string rootDirectory)
        {
            this.profile = new AppProfile(rootDirectory);
            this.defaultConfig = new WtfConfig(this.profile.GetRelativePath("Config.wtf"));
            ConfigManager.Instance.InitializeSettings(this.defaultConfig);

            GameSettings.ChangesApplied += new EventHandler(this.OnChangesApplied);
        }

        private void OnChangesApplied(object sender, EventArgs e)
        {
            this.deviceControl.VSync = GameSettings.IsVSync;
            this.deviceControl.SetResolution(GameSettings.ResolutionX, GameSettings.ResolutionY);

            if (GameSettings.IsFullscreen)
            {
                this.mainWindow.FormBorderStyle = FormBorderStyle.None;
                this.mainWindow.WindowState = FormWindowState.Maximized;
            }
            else
            {
                this.mainWindow.FormBorderStyle = FormBorderStyle.Sizable;
                this.mainWindow.WindowState = FormWindowState.Normal;
            }
        }

        public Form CreateWindow()
        {
            this.mainWindow = new Form()
            {
                WindowState = FormWindowState.Normal,
                Width = 1366,
                Height = 768,
                StartPosition = FormStartPosition.CenterScreen,
                Text = typeof(GameEnvironment).Assembly.FullName,
            };

            this.deviceControl = new GraphicsDeviceControl()
            {
                Dock = DockStyle.Fill,
            };

            this.mainWindow.Controls.Add(this.deviceControl);
            return this.mainWindow;
        }

        public void InitializeServices()
        {
            this.keyboard = new Keyboard(this.deviceControl);
            this.mouse = new Mouse(this.deviceControl);
            this.assetManager = new AssetManager(this.profile);
            this.apiMaster = new LuaApiMaster();
            this.networkManager = new NetworkManager();

            ServiceProvider.Instance.AddService<Keyboard>(this.keyboard);
            ServiceProvider.Instance.AddService<Mouse>(this.mouse);
            ServiceProvider.Instance.AddService<LuaApiMaster>(this.apiMaster);
            ServiceProvider.Instance.AddService<AssetManager>(this.assetManager);
            ServiceProvider.Instance.AddService<AppProfile>(this.profile);
            ServiceProvider.Instance.AddService<NetworkManager>(this.networkManager);

            this.apiMaster.RegisterTypes();
            this.apiMaster.ImportModules(
                this.profile.GetRelativePath("Interface", "Modules"));
            this.keyboard.ReadKeyBindings(
                this.profile.GetRelativePath("Keybindings.wtf"));

            GameSettings.ApplyChanges();

            var gfxMenu = new GfxMenu();
            this.deviceControl.LoadScene(gfxMenu);
        }


        public void Dispose()
        {
            this.mainWindow?.Close();

            this.deviceControl?.Dispose();
            this.mainWindow?.Dispose();
            this.mouse?.Dispose();
            this.keyboard?.Dispose();
            this.apiMaster?.Unload();
            this.assetManager?.Unload();

            this.mainWindow = null;
            this.deviceControl = null;
            this.mouse = null;
            this.keyboard = null;
            this.apiMaster = null;
            this.assetManager = null;
        }
    }
}
