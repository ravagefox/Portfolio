// Source: C_Input
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

using System.Drawing;
using Client.Data.Managers;
using Engine.Core.Input;
using Engine.Data;
using Engine.Graphics;
using MoonSharp;

namespace Client.API
{
    [MoonSharpUserData]
    public partial class C_Input : SystemApi
    {
        // -----------------------------------------------
        // Universal variables
        // -----------------------------------------------
        #region Fields and Properties

        public Keyboard Keyboard => ServiceProvider.Instance.GetService<Keyboard>();
        public Mouse Mouse => ServiceProvider.Instance.GetService<Mouse>();

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

        public bool GetInput(string keyBinding)
        {
            return this.Keyboard.IsKeyDown(keyBinding);
        }

        [MoonSharpMetamethod]
        public static void CreateCursor(string curName, string assetPath, bool pngload)
        {
            var mouse = ServiceProvider.Instance.GetService<Mouse>();
            var assetManager = ServiceProvider.Instance.GetService<AssetManager>();

            if (assetManager.CanOpenStream(assetPath))
            {
                using (var stream = assetManager.OpenStream(assetPath))
                {
                    var bmp = pngload ?
                        (Bitmap)Image.FromStream(stream) :
                        Texture.Read(stream).GetBitmap();

                    var cursor = Mouse.CreateCursor(bmp);
                    mouse.RegisterCursor(curName, cursor);
                }
            }
        }

        [MoonSharpMetamethod]
        public static void SetCursor(string curname)
        {
            var mouse = ServiceProvider.Instance.GetService<Mouse>();
            mouse.SetCursor(curname);
        }

        #endregion
    }
}
