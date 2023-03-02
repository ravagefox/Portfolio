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

using System.Threading;

namespace WebLib
{
    /// <summary>
    /// Provides a single reference based handler.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class Singleton<T>
        where T : Singleton<T>, new()
    {
        /// <summary>
        /// Gets the instance representation.
        /// </summary>
        public static T Instance => GetInstance();


        private static T _instance;
        private static int _refCount;


        /// <summary>
        /// When overriden in a derrived class, initializes the 
        /// basic instance and its variables.
        /// </summary>
        protected abstract void Initialize();


        private static T GetInstance()
        {
            if (Interlocked.Increment(ref _refCount) == 1)
            {
                _instance = new T();
                _instance.Initialize();
            }

            return _instance;
        }
    }
}