// Source: ServerManager
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

using System.Net;
using System.Reflection;
using WebLib;
using WebLib.Api;
using WebLib.IO;
using WebLib.WebResponses;

namespace WebServerApp
{
    /// <summary>
    /// An object which handles all the connections and requests
    /// made by <see cref="HttpListenerContext"/>
    /// </summary>
    public sealed class ContextManager
    {
        /// <summary>
        /// Gets the server that was used to create this 
        /// instance.
        /// </summary>
        public WebServer Server { get; }

        /// <summary>
        /// Gets the configuration used by the server.
        /// </summary>
        public WebConfiguration WebConfiguration => ServiceProvider.Instance.GetService<WebConfiguration>();



        private ApiHandler handler;


        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="server"></param>
        public ContextManager(WebServer server)
        {
            this.handler = new ApiHandler();
            this.Server = server;
            this.Server.HandleContext += this.OnHandleContext;
        }

        public void DiscoverApiContent(string[] inclusionsOnly)
        {
            var asmLocation = Assembly.GetEntryAssembly().Location;
            var path = Path.Combine(Path.GetDirectoryName(asmLocation), "Apps");

            var dirInfo = new DirectoryInfo(path);
            if (dirInfo.Exists)
            {
                var enumerationOptions = new EnumerationOptions()
                {
                    RecurseSubdirectories = true,
                    IgnoreInaccessible = true,
                    ReturnSpecialDirectories = false,
                };

                var fileInfos = dirInfo.EnumerateFiles("*.dll", enumerationOptions);
                if (inclusionsOnly != null && inclusionsOnly.Length > 0)
                {
                    bool CheckMatch(FileInfo n)
                    {
                        return inclusionsOnly.Any(i => n.Name.Contains(i));
                    };

                    fileInfos = fileInfos.Where(CheckMatch);
                }

                fileInfos
                    .ToList()
                    .ForEach(file =>
                    {
                        var asm = Assembly.UnsafeLoadFrom(file.FullName);
                        var exportedTypes = asm.GetExportedTypes();

                        if (exportedTypes.Any() && InvokeEntryPoint(exportedTypes))
                        {
                            Console.WriteLine($"Loaded Assembly {asm.GetName()}");
                            this.handler.ImportTypes(exportedTypes);
                        }
                    });
            }
        }

        private static bool InvokeEntryPoint(IEnumerable<Type> exportedTypes)
        {
            foreach (var type in exportedTypes)
            {
                var method = type.GetMethods()
                    .FirstOrDefault(m => m.GetCustomAttribute<ApiEntryAttribute>() != null);
                if (method != null)
                {
                    var parameters = method.GetParameters();

                    _ = parameters.Length > 0 && parameters.FirstOrDefault().ParameterType == typeof(object[])
                        ? method.Invoke(null,
                            new object[] { Environment.GetCommandLineArgs() })
                        : method.Invoke(null, Array.Empty<object>());

                    return true;
                }
            }

            return false;
        }


        private void OnHandleContext(object sender, HttpClientEventArgs e)
        {
            var url = e.Context.Request.Url;
            Console.WriteLine(url);

            try
            {
                if (this.RedirectToSsl(e)) { return; }
                if (this.IsUrlEmpty(e))
                {
                    if (!string.IsNullOrEmpty(this.WebConfiguration.RedirectUrl))
                    {
                        this.Redirect(e, this.WebConfiguration.RedirectUrl.Replace("\\", "/"));
                        return;
                    }
                }
                else
                {
                    if (!this.HandleFile(e))
                    {
                        if (this.handler.GetNode(url.LocalPath) is ApiContentNode node)
                        {
                            node.Invoke(e);
                        }
                    }
                }

                e.Context.Response.Close();
            }
            catch (ObjectDisposedException) { }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private bool IsUrlEmpty(HttpClientEventArgs e)
        {
            return e.Context.Request.Url.AbsolutePath.Length <= 1;
        }

        private bool RedirectToSsl(HttpClientEventArgs e)
        {
            if (this.WebConfiguration.IsSslEnabled)
            {
                if (e.Context.Request.Url.OriginalString.StartsWith("http://"))
                {
                    var redirectPath =
                        $"https://{e.Context.Request.Url.Host}{e.Context.Request.Url.AbsolutePath}";
                    this.Redirect(e, redirectPath);

                    return true;
                }
            }

            return false;
        }

        private bool HandleFile(HttpClientEventArgs e)
        {
            if (string.IsNullOrEmpty(Path.GetExtension(e.Context.Request.Url.AbsolutePath))) { return false; }

            using (var fileHandler = new HttpFileWebResponse(e.Context))
            {
                fileHandler.RequestUri = e.Context.Request.Url;

                return fileHandler.StatusCode == HttpStatusCode.OK;
            }
        }

        private void Redirect(HttpClientEventArgs e, string url)
        {
            e.Context.Response.Redirect(url);
            e.Context.Response.Close();
        }
    }
}