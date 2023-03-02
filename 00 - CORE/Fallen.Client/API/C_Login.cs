// Source: C_Login
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

using System.Linq;
using System.Net;
using System.Security.Cryptography;
using Client.Data.Managers;
using Client.Data.Net;
using Client.Data.Net.Packets.Authentication;
using Client.Data.Scenes;
using Engine.Core;
using Engine.Data;
using Engine.Diagnostics;
using Engine.Linq;
using Engine.Net;
using Engine.Net.Messaging;
using Fallen.Common;
using MoonSharp;

namespace Client.API
{
    [MoonSharpUserData]
    public partial class C_Login : SystemApi
    {
        // -----------------------------------------------
        // Universal variables
        // -----------------------------------------------
        #region Fields and Properties
        public static AddonManager AddonManager =>
            ServiceProvider.Instance.GetService<AddonManager>();

        public string Username { get; set; }
        public string Password { get; set; }

        public string HashedUsername => this.Username.GetHash<SHA256>();
        public string HashedPassword => this.Password.GetHash<SHA256>();

        public string OAuth { get; private set; }


        private static bool isAuthConnected;
        private static bool isAuthenticated;
        #endregion

        // -----------------------------------------------
        // Client message handlers
        // -----------------------------------------------
        #region Handler Messages

        [AuthMessage(AuthCodes.SMSG_AUTH_LOGON_CHALLENGE)]
        public static void SMSG_AUTH_LOGON_CHALLENGE(
            NetworkClient client,
            NetMessage msg)
        {
            using (var p = msg.PacketReader)
            {
                if (msg.Succeeded)
                {
                    GetInstance<C_Login>().OAuth = p.ReadString();
                    isAuthenticated = true;
                }
            }
        }

        [AuthMessage(AuthCodes.SMSG_AUTH_LOGOUT)]
        public static void SMSG_AUTH_LOGOUT(
            NetworkClient client,
            NetMessage msg)
        {
            using (var p = msg.PacketReader)
            {
                var netMgr = ServiceProvider.Instance.GetService<NetworkManager>();
                netMgr.DisconnectClients();
            }
        }
        #endregion

        // -----------------------------------------------
        // Public user methods
        // -----------------------------------------------
        #region Public API

        public void Login(string user, string pass)
        {
            this.Username = user;
            this.Password = pass;

            var netMgr = ServiceProvider.Instance.GetService<NetworkManager>();
            var realmList = GetConfig().GetRawValue("realmlist");

            var dns = Dns.GetHostEntry(realmList);
            if (dns != null)
            {
                var ipAddress = dns.AddressList.FirstOrDefault(i => i.IsIPv4());
                if (ipAddress != null)
                {
                    netMgr.DisconnectClients();
                    netMgr.ConnectToAuthService(ipAddress);
                }
            }
            if (!Wait("Connecting...", () => netMgr.AuthClient.AESCipher != null)) { return; }


            using (var loginPacket = new LoginPacket())
            {
                loginPacket.Username = this.Username;
                loginPacket.Password = this.HashedPassword;

                loginPacket.Send(netMgr.AuthClient);
            }

            if (!Wait("Authenticating...", () => isAuthenticated))
            {
                LogSystem.Instance.Info("Failed to authenticate", typeof(C_Login));
            }
            else { AddonManager.LoadInterface("Interface\\GlueXML\\Realmlist.xml"); }
        }

        public void Logout()
        {
            var netMgr = ServiceProvider.Instance.GetService<NetworkManager>();
            if (netMgr.AuthClient == null) { return; }

            using (var logoutPacket = new UserPacket(AuthCodes.CMSG_AUTH_LOGOUT))
            {
                logoutPacket.Send(netMgr.AuthClient);
            }
        }

        public void DisconnectFromServer()
        {
            this.Logout();

            var netMgr = ServiceProvider.Instance.GetService<NetworkManager>();
            netMgr.DisconnectClients();

            var container = ServiceProvider.Instance.GetService<ISceneContainer>();
            var scene = container.ActiveScene;
            if (scene is GfxMenu menu)
            {
                AddonManager.LoadInterface("Interface\\GlueXML\\AccountLogin.xml");
            }
            else
            {
                menu = new GfxMenu();
                container.LoadScene(menu);
            }
        }

        #endregion
    }
}
