// Source: C_Character
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
using System.Linq;
using Client.Data.Managers;
using Client.Data.Net;
using Client.Data.Net.Packets.Character;
using Client.Data.Scenes;
using Engine.Core;
using Engine.Data;
using Engine.Diagnostics;
using Engine.Net;
using Engine.Net.Messaging;
using Fallen.Common;
using Fallen.Common.Dbx;
using MoonSharp;

namespace Client.API
{
    [MoonSharpUserData]
    public partial class C_Character : SystemApi
    {

        // -----------------------------------------------
        // Universal variables
        // -----------------------------------------------
        #region Fields and Properties
        private static AddonManager AddonManager =>
            ServiceProvider.Instance.GetService<AddonManager>();

        private CharacterInfo[] localChars = new CharacterInfo[0];
        private CharacterInfo[] worldChars = new CharacterInfo[0];
        private static bool hasRetrievedCharacters;

        #endregion

        // -----------------------------------------------
        // Client message handlers
        // -----------------------------------------------
        #region Handler Messages

        [WorldMessage(GameOpCodes.SMSG_CHAR_ENUM)]
        public static void SMSG_CHAR_ENUM(
            NetworkClient client,
            NetMessage netMsg)
        {
            using (var r = netMsg.PacketReader)
            {
                var isWorldChars = r.ReadBoolean();
                var charCount = r.ReadInt32();
                var chars = new CharacterInfo[charCount];
                for (var i = 0; i < chars.Length; i++)
                {
                    chars[i] = CharacterInfo.FromBlob(r.ReadBytes(r.ReadInt32()));
                }

                var c = GetInstance<C_Character>();
                if (isWorldChars) { c.worldChars = chars; }
                else { c.localChars = chars; }

                hasRetrievedCharacters = true;
            }
        }

        [WorldMessage(GameOpCodes.SMSG_CHAR_CREATE)]
        public static void SMSG_CHAR_CREATE(
            NetworkClient client,
            NetMessage netMsg)
        {
            if (netMsg.Succeeded)
            {
                AddonManager.LoadInterface("Interface\\GlueXML\\CharacterSelect.xml");
            }
            else
            {
                LogSystem.Instance.Error(netMsg.ErrorMessage, typeof(C_Character));
            }
        }

        [WorldMessage(GameOpCodes.SMSG_CHAR_DELETE)]
        public static void SMSG_CHAR_DELETE(
            NetworkClient client,
            NetMessage netMsg)
        {
            if (netMsg.Succeeded)
            {
                AddonManager.LoadInterface("Interface\\GlueXML\\CharacterSelect.xml");
            }
        }

        [WorldMessage(GameOpCodes.SMSG_ENTER_WORLD)]
        public static void SMSG_ENTER_WORLD(
            NetworkClient client,
            NetMessage netMsg)
        {
            using (var r = netMsg.PacketReader)
            {
                var sceneContainer = ServiceProvider.Instance.GetService<ISceneContainer>();
                if (sceneContainer.ActiveScene is GfxMenu menu)
                {
                    var gameWorld = new GfxGameWorld();
                    gameWorld.MapId = (MapId)r.ReadUInt32();

                    var objManager = gameWorld.ObjectManager;
                    var player = objManager.CreateDynamicWorldObject(
                        r.ReadUInt32(),
                        r.ReadString(),
                        WorldObjectType.Player);
                    objManager.Player = player;

                    var playerCount = r.ReadInt32();
                    for (var i = 0; i < playerCount; i++)
                    {
                        var playerObj = objManager.CreateDynamicWorldObject(
                            r.ReadUInt32(),
                            r.ReadString(),
                            WorldObjectType.Player);

                        objManager.GameObjects.Add(playerObj);
                    }

                    objManager.CreateNetworkLoop(r.ReadInt32());

                    sceneContainer.LoadScene(gameWorld);
                }
            }
        }

        [WorldMessage(GameOpCodes.SMSG_LEAVE_WORLD)]
        public static void SMSG_LEAVE_WORLD(
            NetworkClient client,
            NetMessage netMsg)
        {
            GetInstance<C_Login>().DisconnectFromServer();
        }
        #endregion

        // -----------------------------------------------
        // Public user methods
        // -----------------------------------------------
        #region Public API

        public static void LeaveWorld()
        {
            var container = ServiceProvider.Instance.GetService<ISceneContainer>();
            if (container.ActiveScene is GfxGameWorld gameWorld)
            {
                using (var p = new UserPacket(GameOpCodes.CMSG_LEAVE_WORLD))
                {
                    var objManager = gameWorld.ObjectManager;

                    p.Enqueue(() =>
                    {
                        p.Write(objManager.Player.Id.HighId);
                        p.Write((uint)ObjUpdateFlags.Destroy);
                        p.Write((uint)WorldObjectType.Player);
                        p.Write((uint)gameWorld.MapId);
                    });

                    var netMgr = ServiceProvider.Instance.GetService<NetworkManager>();
                    p.Send(netMgr.WorldClient);
                }
            }
        }

        public static void EnterWorld(string charname)
        {
            using (var p = new UserPacket(GameOpCodes.CMSG_ENTER_WORLD))
            {
                p.Enqueue(() =>
                {
                    p.Write(GetInstance<C_Login>().HashedUsername);
                    p.Write(GetInstance<C_Character>().localChars.FirstOrDefault(c => c.Name.ToLower().SequenceEqual(charname.ToLower())).RefId);
                });

                var netMgr = ServiceProvider.Instance.GetService<NetworkManager>();
                p.Send(netMgr.WorldClient);
            }
        }

        public static bool DownloadCharacters(bool isWorldChars)
        {
            var netMgr = ServiceProvider.Instance.GetService<NetworkManager>();

            using (var p = new UserPacket(GameOpCodes.CMSG_CHAR_ENUM))
            {
                p.Enqueue(() =>
                {
                    p.Write(isWorldChars);
                    p.Write(GetInstance<C_Login>().HashedUsername);
                });

                p.Send(netMgr.WorldClient);
            }

            return Wait("Retrieving Character List...", () => hasRetrievedCharacters);
        }

        public static string[] GetCharacterList(bool isWorldChars)
        {
            return (isWorldChars ? GetInstance<C_Character>().worldChars : GetInstance<C_Character>().localChars)
                   .Select(c => c.Name)
                   .ToArray();
        }

        public static void Delete(string charname)
        {
            using (var p = new UserPacket(GameOpCodes.CMSG_CHAR_DELETE))
            {
                var netMgr = ServiceProvider.Instance.GetService<NetworkManager>();

                p.Enqueue(() =>
                {
                    var charInfo = GetInstance<C_Character>().localChars.FirstOrDefault(c => c.Name.ToLower().SequenceEqual(charname.ToLower()));

                    var blob = charInfo.ToArray();
                    p.Write(blob.Length);
                    p.Write(blob);
                });

                p.Send(netMgr.WorldClient);
            }
        }

        public static void Create(Table charInfoTable)
        {
            // TODO: incorporate appearance customization

            var charInfo = new CharacterInfo()
            {
                Name = charInfoTable.Get("name").CastToString(),
                Owner = GetInstance<C_Login>().HashedUsername,

                Class = (ClassId)Enum.Parse(typeof(ClassId), charInfoTable.Get("class").CastToString(), true),
                Gender = (GenderId)Enum.Parse(typeof(GenderId), charInfoTable.Get("gender").CastToString(), true),
                Race = (RaceId)Enum.Parse(typeof(RaceId), charInfoTable.Get("race").CastToString(), true),
            };

            using (var p = new CharacterCreatePacket())
            {
                var netMgr = ServiceProvider.Instance.GetService<NetworkManager>();

                p.CharacterInfo = charInfo;
                p.Send(netMgr.WorldClient);
            }
        }
        #endregion
    }
}
