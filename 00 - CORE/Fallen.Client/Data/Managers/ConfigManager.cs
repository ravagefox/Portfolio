// Source: ConfigManager
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
using System.Reflection;
using Engine.Assets;
using Engine.Data;
using Engine.Data.IO;

namespace Client.Data.Managers
{
    public sealed class ConfigManager : ManagerService
    {
        private IEnumerable<Type> asmTypes;
        private IEnumerable<PropertyInfo> staticProperties;
        private WtfConfig defaultConfig;


        public override void Initialize()
        {
            ServiceProvider.Instance.AddService<ConfigManager>(this);

            this.asmTypes = this.GetTypes();
            this.staticProperties = this.GetStaticProperties(this.asmTypes);

            var profile = ServiceProvider.Instance.GetService<AppProfile>();
            this.defaultConfig = new WtfConfig(profile.GetRelativePath("Config.wtf"));
            this.InitializeSettings(this.defaultConfig);
        }



        public void InitializeSettings(WtfConfig config)
        {
            foreach (var property in this.staticProperties)
            {
                try
                {
                    var propertyConfigName = property.GetCustomAttribute<WtfConfigPropertyAttribute>().Name ??
                        property.Name;

                    if (property.PropertyType == typeof(bool))
                    {
                        var intValue = (int)config.GetValue(propertyConfigName, typeof(int));
                        property.SetValue(null, intValue == 1);
                    }
                    else
                    {
                        var value = config.GetValue(propertyConfigName, property.PropertyType);
                        property.SetValue(null, value);
                    }
                }
                catch (KeyNotFoundException)
                {
                }
            }
        }


        private IEnumerable<Type> GetTypes()
        {
            return typeof(ConfigManager).Assembly.GetExportedTypes();
        }

        private IEnumerable<PropertyInfo> GetStaticProperties(IEnumerable<Type> asmTypes)
        {
            foreach (var type in asmTypes)
            {
                var flags = BindingFlags.Public | BindingFlags.Static;
                var properties = type.GetProperties(flags)
                    .Where(property => property.GetCustomAttribute(typeof(WtfConfigPropertyAttribute)) != null);

                foreach (var property in properties) { yield return property; }
            }
        }
    }
}
