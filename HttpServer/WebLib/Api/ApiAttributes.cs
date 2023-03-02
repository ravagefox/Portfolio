// Source: ApiAttributes
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

namespace WebLib.Api
{
    /// <summary>
    /// Exposes the main entry point for a given assembly that would 
    /// be registered by the web server.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public sealed class ApiEntryAttribute : Attribute
    {
    }

    /// <summary>
    /// Exposes a method to be dynamically invoked by the client POST event.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
    public sealed class ApiMethodAttribute : Attribute
    {
        /// <summary>
        /// Gets or sets the name that would bind
        /// the method to.
        /// </summary>
        public string Name { get; set; }
    }

    /// <summary>
    /// Exposes a class to the server listener and utilises it as a resource
    /// path.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class,
                    AllowMultiple = true,
                    Inherited = false)]
    public sealed class ApiReferenceAttribute : Attribute
    {
        /// <summary>
        /// Gets or sets the request uri that the 
        /// <see cref="System.Net.HttpListenerContext"/> would request.
        /// </summary>
        public string Uri { get; set; }
    }
}
