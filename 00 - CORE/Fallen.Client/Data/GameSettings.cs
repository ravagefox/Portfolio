// Source: GameSettings
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
using Engine.Data.IO;

namespace Client.Data
{
    public static class GameSettings
    {
        #region Properties

        #region GRAPHICS
        [WtfConfigProperty(Name = "vsyncenabled")]
        public static bool IsVSync
        {
            get => m_vsync;
            set => m_vsync = AppendChange(m_vsync, value);
        }

        [WtfConfigProperty(Name = "resx")]
        public static int ResolutionX
        {
            get => m_resx;
            set => m_resx = AppendChange(m_resx, value);
        }

        [WtfConfigProperty(Name = "resy")]
        public static int ResolutionY
        {
            get => m_resy;
            set => m_resy = AppendChange(m_resy, value);
        }

        [WtfConfigProperty(Name = "viewDistance")]
        public static float ViewDistance
        {
            get => m_viewDist;
            set => m_viewDist = AppendChange(m_viewDist, value);
        }

        [WtfConfigProperty(Name = "isfullscreen")]
        public static bool IsFullscreen
        {
            get => m_fullscreen;
            set => m_fullscreen = AppendChange(m_fullscreen, value);
        }

        #endregion
        #region NETWORK

        [WtfConfigProperty(Name = "realmlist")]
        public static string Realmlist { get; set; }
        #endregion
        #region INPUT
        [WtfConfigProperty(Name = "mouseSensitivity")]
        public static float MouseSensitivity
        {
            get => m_sensitivity;
            set => m_sensitivity = AppendChange(m_sensitivity, value);
        }
        #endregion
        #region AUDIO

        [WtfConfigProperty(Name = "sfxvol")]
        public static float SfxVol
        {
            get => m_sfxvol;
            set => m_sfxvol = AppendChange(m_sfxvol, value);
        }
        #endregion

        #endregion

        #region Fields
        public static event EventHandler ChangesApplied;


        private static bool pendingChanges;
        private static float m_viewDist;
        private static int m_resy;
        private static int m_resx;
        private static bool m_vsync;
        private static bool m_fullscreen;
        private static float m_sensitivity;
        private static float m_sfxvol;
        #endregion

        #region Methods
        private static T AppendChange<T>(T field, T newValue)
        {
            if ((object)field != (object)newValue)
            {
                pendingChanges = true;
                return newValue;
            }

            return field;
        }

        public static void ApplyChanges()
        {
            if (pendingChanges)
            {
                ChangesApplied?.Invoke(null, EventArgs.Empty); pendingChanges = false;
            }
        }
        #endregion
    }
}
