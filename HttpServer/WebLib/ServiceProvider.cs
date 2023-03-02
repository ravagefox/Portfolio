// Source: ServiceProvider
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

namespace WebLib
{
    /// <summary>
    /// Provides a directory for built-in services accesible from anywhere in
    /// the application.
    /// </summary>
    public sealed class ServiceProvider : Singleton<ServiceProvider>, IServiceProvider
    {

        private Dictionary<Type, object> _services;


        /// <summary>
        /// Adds the specified service to the current instance.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="instance"></param>
        public void AddService<T>(object instance)
        {
            this._services[typeof(T)] = instance;
        }

        /// <summary>
        /// Removes the specified service from the current instance.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void RemoveService<T>()
        {
            if (this._services.TryGetValue(typeof(T), out _))
            {
                _ = this._services.Remove(typeof(T));
            }
        }

        /// <summary>
        /// Returns the service that matches the specified type.
        /// </summary>
        /// <param name="serviceType"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public object GetService(Type serviceType)
        {
            return this._services.TryGetValue(serviceType, out var value)
                ? value
                : throw new KeyNotFoundException(
                "The service requested could not be located.",
                new Exception(serviceType.FullName));
        }

        /// <summary>
        /// Returns the service that matches the specified type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetService<T>()
        {
            return (T)this.GetService(typeof(T));
        }

        /// <summary>
        /// When overriden in a derrived class, initializes the current
        /// instance and its resources.
        /// </summary>
        protected override void Initialize()
        {
            this._services = new Dictionary<Type, object>();
        }
    }
}
