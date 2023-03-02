// Source: C_Audio
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

using Client.Data;
using Engine.Core.Audio;
using MoonSharp;

namespace Client.API
{
    [MoonSharpUserData]
    public partial class C_Audio : SystemApi
    {
        // -----------------------------------------------
        // Universal variables
        // -----------------------------------------------
        #region Fields and Properties

        #endregion

        // -----------------------------------------------
        // Client message handlers
        // -----------------------------------------------
        #region Handler Messages

        #endregion

        // -----------------------------------------------
        // Public user methods
        // -----------------------------------------------
        #region Public API

        [MoonSharpMetamethod]
        public static void PlaySound(string path)
        {
            ImmediateSoundEngine.Play(path, GetFxVolume());
        }

        [MoonSharpMetamethod]
        public static float GetFxVolume()
        {
            return GameSettings.SfxVol;
        }
        #endregion
    }
}
