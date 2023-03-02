// Source: LoginPacket
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

using Fallen.Common;

namespace Client.Data.Net.Packets.Authentication
{
    internal class LoginPacket : UserPacket
    {
        public string Username { get; set; }

        public string Password { get; set; }


        public LoginPacket()
            : base(AuthCodes.CMSG_AUTH_LOGON_CHALLENGE)
        {
            this.Enqueue(() =>
            {
                this.Write(this.Username);
                this.Write(this.Password);
            });
        }
    }
}
