// Source: ApiHandler
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

namespace WebLib.Api
{
    /// <summary>
    /// Opens a controller to accept url requests.
    /// </summary>
    public sealed class ApiHandler
    {
        #region Fields

        private Dictionary<string, ApiContentNode> apiReferences;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance
        /// </summary>
        public ApiHandler()
        {
            this.apiReferences = new Dictionary<string, ApiContentNode>();
        }
        #endregion

        #region Methods
        /// <summary>
        /// Trys and retrieves the <see cref="ApiContentNode"/> if it's registered.
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public ApiContentNode GetNode(string url)
        {
            if (this.apiReferences.TryGetValue(url.ToLower(), out var apiNode))
            {
                return apiNode;
            }

            // TODO: return 404 not found.
            return null;
        }

        /// <summary>
        /// Attempts to import the <see cref="ApiContentNode"/> from within 
        /// the specified types.
        /// </summary>
        /// <param name="types"></param>
        public void ImportTypes(IEnumerable<Type> types)
        {
            foreach (var type in types
                .Where(t => t.IsSubclassOf(typeof(ApiContentNode)))
                .Where(t => !t.IsAbstract))
            {
                this.apiReferences[GetUri(type).ToLower()] =
                    (ApiContentNode)Activator.CreateInstance(type);
            }
        }

        private static string GetUri(Type type)
        {
            var url = $"/{type.FullName.Replace(".", "/")}";
            if (type.GetCustomAttribute<ApiReferenceAttribute>() is ApiReferenceAttribute attr)
            {
                if (!string.IsNullOrEmpty(attr.Uri))
                {
                    url = attr.Uri.ToString().StartsWith("/") ?
                          attr.Uri :
                          $"/{attr.Uri}";
                }
            }

            return url;
        }
        #endregion
    }
}
