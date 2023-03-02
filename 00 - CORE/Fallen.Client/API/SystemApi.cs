// Source: SystemApi
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
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using Engine.Assets;
using Engine.Data;
using Engine.Data.IO;
using Engine.Data.Scripting.Lua;
using Engine.Diagnostics;
using Fallen.Common;
using MoonSharp;

namespace Client.API
{
    [MoonSharpUserData]
    public partial class SystemApi : UserLuaData
    {
        // -----------------------------------------------
        // Universal variables
        // -----------------------------------------------
        #region Fields and Properties

        #endregion

        // -----------------------------------------------
        // Client message handlers
        // -----------------------------------------------
        #region Handler Messages

        #endregion

        // -----------------------------------------------
        // Public user methods
        // -----------------------------------------------
        #region Public API
        [MoonSharpMetamethod]
        public static void QuitGame()
        {
            Application.Exit();
        }

        [MoonSharpMetamethod]
        public static string GetCopyrightInfo()
        {
            return $"Copyright {DateTime.Now.Year} Crexium Pty Ltd. All rights reserved.";
        }

        [MoonSharpMetamethod]
        public static string GetBuildInfo()
        {
            var ver = typeof(SystemApi).Assembly.GetName().Version;
            var buildTime = new DateTime(2000, 1, 1).Add(
                new TimeSpan(
                    (TimeSpan.TicksPerDay * ver.Build) +
                    (TimeSpan.TicksPerSecond * 2 * ver.Revision)));

            var isAlpha = ver.Major == 0 ? "Alpha" : "Release";
            var is64Bit = Environment.Is64BitProcess ? "x64" : "x86";

            return $"{ver.Major}.{ver.Minor}.{ver.Build} ({ver.Revision}) {isAlpha} {is64Bit}\r\n" +
                $"{(MonthFormat)buildTime.Month} {buildTime.Day} {buildTime.Year}";
        }

        [MoonSharpMetamethod]
        public static string GetSavedAccountName()
        {
            return GetConfig().GetRawValue("accountname");
        }

        [MoonSharpMetamethod]
        public static void SetSavedAccountName(string user)
        {
            var profile = ServiceProvider.Instance.GetService<AppProfile>();
            var cfgPath = profile.GetRelativePath("Config.wtf");

            var cfg = new WtfConfig(cfgPath);
            cfg.SetValue("accountname", user);

            using (var fs = File.Create(cfgPath))
            {
                cfg.Save(fs);
            }
        }
        #endregion

        // -----------------------------------------------
        // Private user methods
        // -----------------------------------------------
        #region Private API

        protected static WtfConfig GetConfig()
        {
            return new WtfConfig("Data\\Config.wtf");
        }

        protected static async Task<bool> WaitAsync(string msg, Func<Task<bool>> waitOp)
        {
            LogSystem.Instance.Debug(msg, typeof(SystemApi));
            var sw = Stopwatch.StartNew();
            var success = false;
            while (!success)
            {
                success = await waitOp();
                if (sw.Elapsed.TotalMilliseconds > 2000)
                {
                    sw.Stop();
                    break;
                }
            }

            return success;
        }

        protected static bool Wait(string msg, Func<bool> waitOp)
        {
            LogSystem.Instance.Debug(msg, typeof(SystemApi));
            var sw = Stopwatch.StartNew();
            var success = false;
            while (!success)
            {
                success = waitOp();
                if (sw.Elapsed.TotalMilliseconds > 2000)
                {
                    sw.Stop();
                    break;
                }
            }

            return success;
        }

        #endregion
    }
}
