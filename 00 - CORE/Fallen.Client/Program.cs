// Source: Program
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
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using Client.Data;
using Engine.Assets;
using Engine.Core;
using Engine.Graphics;

namespace Client
{
    internal class Program
    {
        static Program()
        {
            if (!isSet)
            {
                AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;

                isSet = true;
                RootDirectory = "Data";
            }
        }


        private static readonly bool isSet;
        private static readonly string RootDirectory;
        private static Form mainWindow;
        private static GameEnvironment env;


        private static void Main(string[] args)
        {
            using (var splashScreen = new SplashScreen())
            {
                // TODO: change the splash screen to appropriate splash texture
                splashScreen.SplashScreenInitialize += SplashScreen_OnInitialize;
                splashScreen.BackgroundImage = Image.FromFile(
                    Process.GetCurrentProcess().ProcessName + ".png");
                splashScreen.ShowDialog();

                if (splashScreen.DialogResult == DialogResult.OK)
                {
                    Application.Run(mainWindow);
                    env?.Dispose();
                }
            }
        }



        #region Event Handlers

        private static void SplashScreen_OnInitialize(object sender, EventArgs e)
        {
            AppProfile.DoNotCreateDirectories();
            DebugDraw.DebugMode = true;
            env = new GameEnvironment(RootDirectory);

            mainWindow = env.CreateWindow();
            env.InitializeServices();
        }

        private static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            var name = args.Name.Substring(0, args.Name.IndexOf(','));
            var dirPath = Path.Combine(
                Environment.CurrentDirectory,
                RootDirectory,
                "Binaries");

            var dllFileInfo = new FileInfo(Path.Combine(dirPath, name + ".dll"));
            var exeFileInfo = new FileInfo(Path.Combine(dirPath, name + ".exe"));

            return dllFileInfo.Exists
                ? Assembly.UnsafeLoadFrom(dllFileInfo.FullName)
                : exeFileInfo.Exists ? Assembly.UnsafeLoadFrom(exeFileInfo.FullName) : null;
        }

        #endregion
    }
}
