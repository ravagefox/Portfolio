// Source: C_Realmlist
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
using Client.Data.Managers;
using Client.Data.Net;
using Engine.Data;
using Engine.Linq;
using Engine.Net;
using Engine.Net.Messaging;
using Fallen.Common;
using Fallen.Common.Dbx;
using MoonSharp;

namespace Client.API
{
    [MoonSharpUserData]
    public partial class C_RealmList : SystemApi
    {
        // -----------------------------------------------
        // Universal variables
        // -----------------------------------------------
        #region Fields and Properties

        private static RealmEntry[] realms = new RealmEntry[0];
        private static bool isHandshaked;
        private static bool hasRetrievedRealms;
        private static string realmName;
        #endregion

        // -----------------------------------------------
        // Client message handlers
        // -----------------------------------------------
        #region Handler Messages
        [AuthMessage(AuthCodes.SMSG_REALM_RETRIEVE)]
        public static void SMSG_REALM_RETRIEVE(
            NetworkClient client,
            NetMessage msg)
        {
            if (!msg.Succeeded) { return; }

            using (var r = msg.PacketReader)
            {
                var realmCount = r.ReadInt32();
                realms = new RealmEntry[realmCount];

                for (var i = 0; i < realmCount; i++)
                {
                    realms[i] = RealmEntry.FromBlob(r.ReadBytes(r.ReadInt32()));
                }

                hasRetrievedRealms = true;
            }
        }

        [WorldMessage(GameOpCodes.SMSG_HANDSHAKE)]
        public static void SMSG_HANDSHAKE(
            NetworkClient client,
            NetMessage msg)
        {
            isHandshaked = msg.Succeeded;
        }
        #endregion

        // -----------------------------------------------
        // Public user methods
        // -----------------------------------------------
        #region Public API
        public static string GetRealmId()
        {
            return realmName;
        }

        public static void ConnectToRealm(string realmName)
        {
            C_RealmList.realmName = realmName;

            var entry = realms.FirstOrDefault(r => r.Name.ToLower().SequenceEqual(realmName.ToLower()));
            if (!Equals(entry, null))
            {
                var ip = entry.IP.ToIPv4();
                var netMgr = ServiceProvider.Instance.GetService<NetworkManager>();
                netMgr.ConnectToWorldServer(ip);

                if (!Wait("Connecting to Realm...", () => netMgr.WorldClient.AESCipher != null)) { return; }

                using (var handshakePacket = new UserPacket(GameOpCodes.CMSG_HANDSHAKE))
                {
                    handshakePacket.Enqueue(() =>
                    {
                        handshakePacket.Write(GetInstance<C_Login>().OAuth);
                    });

                    handshakePacket.Send(netMgr.WorldClient);
                }

                if (!Wait("Handshaking...", () => isHandshaked))
                {
                    GetInstance<C_Login>().DisconnectFromServer();
                }
                else
                {
                    var addonManager = ServiceProvider.Instance.GetService<AddonManager>();
                    addonManager.LoadInterface("Interface\\GlueXML\\CharacterSelect.xml");
                }
            }
        }

        [MoonSharpMetamethod]
        public static string GetLastKnownRealm()
        {
            return GetConfig().GetRawValue("lastrealm");
        }

        public static string[] GetRealms()
        {
            hasRetrievedRealms = false;
            using (var realmPacket = new UserPacket(AuthCodes.CMSG_REALM_RETRIEVE))
            {
                var netMgr = ServiceProvider.Instance.GetService<NetworkManager>();
                realmPacket.Send(netMgr.AuthClient);
            }

            return !Wait("Retrieving Realmlist...", () => hasRetrievedRealms) ?
                   (new string[0]) : realms.Select(r => r.Name).ToArray();
        }


        #endregion
    }
}
