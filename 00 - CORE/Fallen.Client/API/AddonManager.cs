// Source: AddonManager
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
using System.IO;
using System.Linq;
using Client.Data;
using Client.Data.Scenes;
using Engine.Assets;
using Engine.Core;
using Engine.Data;
using Engine.Graphics.UI;
using MoonSharp;

namespace Client.API
{
    [MoonSharpUserData]
    public partial class AddonManager : SystemApi
    {
        // -----------------------------------------------
        // Universal variables
        // -----------------------------------------------
        #region Fields and Properties

        public AppProfile Profile =>
            ServiceProvider.Instance.GetService<AppProfile>();

        public IEnumerable<string> AddonsToLoad { get; set; } =
            Enumerable.Empty<string>();


        private static string reloadPath;

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
        public static void ReloadInterface()
        {
            if (!string.IsNullOrEmpty(reloadPath))
            {
                LoadInterface(reloadPath);
            }
        }

        [MoonSharpMetamethod]
        public static void LoadInterface(string path)
        {
            var sceneContainer = ServiceProvider.Instance.GetService<ISceneContainer>();
            if (sceneContainer.ActiveScene is GfxMenu menu)
            {
                reloadPath = path;

                menu.Interface?.Unload();
                menu.Interface = menu.Content.Load<UserInterface>(path);
            }
        }

        [MoonSharpMetamethod]
        public void SetAddons(Table addonNames)
        {
            this.AddonsToLoad = addonNames.Values
                .Where(name => name.IsNotNil())
                .Select(name => name.CastToString());
        }

        [MoonSharpMetamethod]
        public string[] EnumerateAddonNames()
        {
            var addonDir = this.Profile.GetRelativePath("Interface", "AddOns");
            var dirInfo = new DirectoryInfo(addonDir);

            var fileNames = new List<string>();
            foreach (var info in dirInfo.EnumerateDirectories())
            {
                var files = info.EnumerateFiles("*.xml");
                if (files.Any())
                {
                    var name = files.First(f => Path.GetExtension(f.FullName).ToLower().SequenceEqual(".xml"));
                    fileNames.Add(Path.GetFileNameWithoutExtension(name.FullName));
                }
            }

            return fileNames.ToArray();
        }


        public IEnumerable<UserInterface> LoadAddons()
        {
            return this.AddonsToLoad.Any() ?
                   this.GetAddons(this.AddonsToLoad.ToArray()) :
                   this.LoadAllAddons();
        }

        public IEnumerable<UserInterface> LoadAllAddons()
        {
            return this.GetAddons(this.EnumerateAddonNames());
        }

        public IEnumerable<UserInterface> GetAddons(params string[] enabledAddonNames)
        {
            var paths = enabledAddonNames.Select(name =>
            {
                return this.Profile.GetRelativePath("Interface", "AddOns", name, name + ".xml");
            });

            var lst = new List<UserInterface>();
            var content = ServiceProvider.Instance.GetService<AssetManager>();
            foreach (var path in paths)
            {
                lst.Add(content.Load<UserInterface>(path));
            }

            return lst;
        }
        #endregion
    }
}
