// Source: AssetConvertHandler
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
using System.IO;
using System.Linq;
using Engine.Data.IO;
using Engine.Graphics.UI;

namespace Client.Data.IO.ConvertHandlers
{
    public partial class AssetConvertHandler
    {
        public static void ImportUserInterface(ConvertEventArgs e)
        {
            FontLoader.Size = 18.0f;
            var uiReader = new UIReader()
            {
                Font = FontLoader.Load("Fonts\\FRIZQT__.ttf"),
                UserInterface = new UserInterface(),
            };

            var controlLibrary = (ControlCollection)uiReader.Read(e.Input as Stream);
            controlLibrary.ToList().ForEach(c => uiReader.UserInterface.AddControl(c));

            e.Result = uiReader.UserInterface;
            uiReader.UserInterface.Initialize();
        }
    }
}
