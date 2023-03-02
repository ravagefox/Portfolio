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
using Client.Data.Scenes;
using Engine.Assets;
using Engine.Core;
using Engine.Data;
using Engine.Graphics.UI;
using MoonSharp;

namespace Client.Data.Managers
{
    [MoonSharpUserData]
    public sealed class AddonManager : ManagerService
    {
        public IEnumerable<string> AddonsToLoad { get; set; }
            = Enumerable.Empty<string>();


        private string reloadPath;


        public override void Initialize()
        {
            ServiceProvider.Instance.AddService<AddonManager>(this);
        }


        public void SetAddons(Table addonNames)
        {
            this.AddonsToLoad = addonNames.Values
                .Where(name => name.IsNotNil())
                .Select(name => name.CastToString());
        }

        public void ReloadInterface()
        {
            var sceneContainer = ServiceProvider.Instance.GetService<ISceneContainer>();
            if (sceneContainer.ActiveScene is GfxMenu menu)
            {
                menu.UserInterface?.Unload();
                menu.UserInterface = menu.Content.Load<UserInterface>(this.reloadPath);
            }
        }

        public void LoadInterface(string path)
        {
            var sceneContainer = ServiceProvider.Instance.GetService<ISceneContainer>();
            if (sceneContainer.ActiveScene is GfxMenu menu)
            {
                this.reloadPath = path;
                menu.UserInterface?.Unload();
                menu.UserInterface = menu.Content.Load<UserInterface>(path);
            }
        }

        public IEnumerable<UserInterface> LoadAddons()
        {
            return this.AddonsToLoad.Any() ?
                   this.GetAddons(this.AddonsToLoad.ToArray()) :
                   this.LoadAllAddons();
        }


        private IEnumerable<string> EnumerateAddonNames()
        {
            var profile = ServiceProvider.Instance.GetService<AppProfile>();
            var addonDir = profile.GetRelativePath("Interface", "AddOns");
            var dirInfo = new DirectoryInfo(addonDir);

            foreach (var fInfo in dirInfo.EnumerateDirectories())
            {
                var files = fInfo.EnumerateFiles("*.xml");
                if (files.Any())
                {
                    yield return Path.GetFileNameWithoutExtension(files.First().FullName);
                }
            }
        }

        private IEnumerable<UserInterface> LoadAllAddons()
        {
            return this.GetAddons(this.EnumerateAddonNames().ToArray());
        }

        private IEnumerable<UserInterface> GetAddons(params string[] enabledAddonNames)
        {
            var content = ServiceProvider.Instance.GetService<AssetManager>();
            var profile = ServiceProvider.Instance.GetService<AppProfile>();
            var paths = enabledAddonNames.Select(name =>
            {
                return profile.GetRelativePath("Interface", "Addons", name, name + ".xml");
            });

            foreach (var path in paths)
            {
                yield return content.Load<UserInterface>(path);
            }
        }
    }
}
