// Source: UserInterface
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

using System.IO;
using Engine.Data.IO;
using Engine.Graphics.UI;

namespace Wargame.Data.IO.ContentReaders
{
    internal static partial class ConvertHandlers
    {
        public static void ImportUserInterface(ConvertEventArgs e)
        {
            var uiReader = new UIReader();
            uiReader.CallingFolder = Path.GetDirectoryName(e.AssetPath);
            FontLoader.Size = 18.0f;

            e.Result = uiReader.Read(e.Input as Stream);
        }

    }
}
