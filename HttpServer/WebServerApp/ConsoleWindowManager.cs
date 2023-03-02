// Source: ConsoleWindowManager
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

using System.Runtime.InteropServices;

namespace WebServerApp
{
    internal sealed class ConsoleWindowManager
    {

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);


        private const int SW_HIDE = 0;
        private const int SW_SHOW = 5;


        public ConsoleWindowManager()
        {
        }

        internal void HandleArgument(string value)
        {
            if (string.IsNullOrEmpty(value)) { return; }

            switch (value)
            {
                case "visible":
                    _ = ShowWindow(GetConsoleWindow(), SW_SHOW);
                    break;

                case "hidden":
                    _ = ShowWindow(GetConsoleWindow(), SW_HIDE);
                    break;
            }
        }
    }
}