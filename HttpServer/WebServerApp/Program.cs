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

using WebLib;

namespace WebServerApp
{
    internal class Program
    {
        private const string ConfigArgumentPath = "/config";
        private const string InclusionsArgumentPath = "/inclusions";
        private const string ConsoleArgumentPath = "/console";

        private static void Main()
        {
            var consoleWindowManager = new ConsoleWindowManager();
            consoleWindowManager.HandleArgument(ReadArgument(ConsoleArgumentPath));

            var serverManager = new ServerManager(ReadArgument(ConfigArgumentPath));

            var server = ServiceProvider.Instance.GetService<WebServer>();

            var contextManager = new ContextManager(server);
            ServiceProvider.Instance.AddService<ContextManager>(contextManager);

            var inclusions = ReadArgument(InclusionsArgumentPath)?
                             .Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            contextManager.DiscoverApiContent(inclusions);
            serverManager.BeginHosting();

            while (server.IsRunning)
            {
            }

            server.Dispose();
        }

        private static string ReadArgument(string query)
        {
            var args = Environment.GetCommandLineArgs();
            if (args == null || args.Length == 0) { return null; }

            var lst = args.ToList();
            var comparer = StringComparer.OrdinalIgnoreCase;
            bool Compare(string s1, string s2)
            {
                return comparer.Compare(s1, s2) == 0;
            };

            if (lst.Any(s => Compare(s, query)))
            {
                var i = lst.First(s => Compare(s, query));
                var idx = lst.IndexOf(i) + 1;

                return lst[idx];
            }

            return null;
        }

    }
}
