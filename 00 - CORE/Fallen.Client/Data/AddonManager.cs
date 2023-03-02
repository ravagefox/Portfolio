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
using Client.Data.IO;
using Engine.Assets;
using Engine.Data;
using Engine.Graphics.UI;
using MoonSharp;

namespace Client.Data
{
    public sealed class AddonManager
    {
        public AppProfile Profile =>
            ServiceProvider.Instance.GetService<AppProfile>();

        public IEnumerable<string> AddonsToLoad { get; set; }


        public AddonManager()
        {
            this.AddonsToLoad = Enumerable.Empty<string>();
        }

        public IEnumerable<UserInterface> LoadAddons()
        {
            if (this.AddonsToLoad.Any())
            {
                return this.GetAddons(this.AddonsToLoad.ToArray());
            }
            else { return Enumerable.Empty<UserInterface>(); }
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
    }
}
