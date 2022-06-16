// Source: MainWindow
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

using System.Windows.Forms;
using Engine.Graphics;
using Wargame.Data.Scenes;

namespace Wargame
{
    public partial class MainWindow : Form
    {
        public GraphicsDeviceControl GraphicsDeviceControl { get; }

        public MainWindow()
        {
            this.InitializeComponent();

            this.GraphicsDeviceControl = new GraphicsDeviceControl(this)
            {
                Dock = DockStyle.Fill
            };
        }
    }
}
