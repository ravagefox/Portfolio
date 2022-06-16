using System;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using Engine.Assets;
using Engine.Core;
using Engine.Core.Input;
using Engine.Data;
using Engine.Data.Scripting.Lua;
using Engine.Diagnostics;
using Engine.Graphics;
using Wargame.Data;
using Wargame.Data.Scenes;
using Wargame.Properties;

namespace Wargame
{
    internal static class Program
    {
        static Program()
        {
            if (!isSet)
            {
                isSet = true;

                AppDomain.CurrentDomain.AssemblyResolve += OnAssemblyResolve;
                AppDomain.CurrentDomain.ProcessExit += OnProcessExit;
            }
        }

        private static bool isSet = false;
        private static MainWindow mainWindow;
        private const string DefaultRootDirectory = "Data";


        [STAThread]
        private static void Main(string[] args)
        {
            Application.SetCompatibleTextRenderingDefault(false);
            using (var splashScreen = new SplashScreen())
            {
                splashScreen.SplashScreenInitialize += SplashScreen_SplashScreenInitialize;
                splashScreen.BackgroundImage = Resources.CrexiumSplash;
                splashScreen.ShowDialog();

                if (splashScreen.DialogResult == DialogResult.OK)
                {
                    Application.Run(mainWindow);
                }
            }
        }

        private static void CurrentDomain_ProcessExit(object sender, System.EventArgs e)
        {
            LogSystem.Instance.CaptureLog();
        }

        private static void SplashScreen_SplashScreenInitialize(object sender, System.EventArgs e)
        {
            AppProfile.DoNotCreateDirectories();
            mainWindow = new MainWindow();

            var profile = new AppProfile("Data");
            var luaScriptEngine = new LuaApiMaster();
            var assetManager = new AssetManager(profile);
            var keyboard = new Keyboard(mainWindow.GraphicsDeviceControl);
            var mouse = new Mouse(mainWindow.GraphicsDeviceControl);

            ServiceProvider.Instance.AddService<AppProfile>(profile);
            ServiceProvider.Instance.AddService<LuaApiMaster>(luaScriptEngine);
            ServiceProvider.Instance.AddService<AssetManager>(assetManager);
            ServiceProvider.Instance.AddService<ILoader>(assetManager);
            ServiceProvider.Instance.AddService<IStreamContainer>(assetManager);
            ServiceProvider.Instance.AddService<Keyboard>(keyboard);
            ServiceProvider.Instance.AddService<Mouse>(mouse);
            ServiceProvider.Instance.AddService<MainWindow>(mainWindow);

            luaScriptEngine.RegisterTypes();
            keyboard.ReadKeyBindings("Data\\KeyBindings.wtf");

            mainWindow.GraphicsDeviceControl.LoadScene(new WorldScene());
            DebugDraw.DebugMode = true;
        }

        #region System Handlers

        private static Assembly OnAssemblyResolve(object sender, ResolveEventArgs args)
        {
            var name = args.Name.Substring(0, args.Name.IndexOf(','));
            var dirPath = Path.Combine(Environment.CurrentDirectory, DefaultRootDirectory, "Binaries");

            var dllFileInfo = new FileInfo(Path.Combine(dirPath, name + ".dll"));
            var exeFileInfo = new FileInfo(Path.Combine(dirPath, name + ".exe"));

            if (dllFileInfo.Exists) { return Assembly.UnsafeLoadFrom(dllFileInfo.FullName); }
            return exeFileInfo.Exists ? Assembly.UnsafeLoadFrom(exeFileInfo.FullName) : null;
        }

        private static void OnProcessExit(object sender, EventArgs e)
        {
            LogSystem.Instance.CaptureLog();
        }

        private static void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            LogSystem.Instance.Error(e.ExceptionObject as Exception, typeof(Program));
            LogSystem.Instance.CaptureLog();
        }

        #endregion

    }
}
