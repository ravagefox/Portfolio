// Source: HostManager
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

namespace WebLib
{
    /// <summary>
    /// Provides a singleton instance for creating, and managing hostnames.
    /// </summary>
    public sealed class HostManager : Singleton<HostManager>
    {


        private Dictionary<string, WebServer> hostServers;



        /// <summary>
        /// When overriden in a derrived class, initializes the current
        /// instance and its resources.
        /// </summary>
        protected override void Initialize()
        {
            this.hostServers = new Dictionary<string, WebServer>();
        }


        /// <summary>
        /// Creates a new host with the specified hostnames.
        /// </summary>
        /// <param name="primaryHost"></param>
        /// <param name="hostnames"></param>
        public WebServer CreateHost(string primaryHost, string[] hostnames)
        {
            if (this.hostServers.ContainsKey(primaryHost))
            {
                throw new InvalidOperationException(
                    "An existing host with the primaryHost key already exists.");
            }

            var server = new WebServer();
            var hostNames = hostnames.Except(new[] { primaryHost }).Append(primaryHost).ToList();
            hostNames.ForEach(hostname => server.AddPrefix(hostname));

            this.hostServers[primaryHost] = server;
            return server;
        }

        /// <summary>
        /// Starts executing requests on the specified server.
        /// </summary>
        /// <param name="primaryHost"></param>
        public void Start(string primaryHost)
        {
            if (this.hostServers.TryGetValue(primaryHost, out var server))
            {
                server.Start();
            }
        }

        /// <summary>
        /// Stops the execution of the server.
        /// </summary>
        /// <param name="primaryHost"></param>
        public void Stop(string primaryHost)
        {
            if (this.hostServers.TryGetValue(primaryHost, out var server))
            {
                server.Stop();
            }
        }
    }
}
