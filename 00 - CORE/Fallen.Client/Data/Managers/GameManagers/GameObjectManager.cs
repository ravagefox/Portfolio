// Source: GameObjectManager
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
using Client.Data.Gos;
using Client.Data.Scenes;
using Client.Extensions;
using Engine.Core;
using Engine.Data;
using Engine.Net;
using Engine.Net.Messaging;
using Fallen.Common;

namespace Client.Data.Managers.GameManagers
{
    public sealed class GameObjectManager : ManagerService
    {
        public GameObjectCollection GameObjects { get; private set; }
        public DynamicWorldObject Player { get; internal set; }

        private GameLoop networkLoop;


        public override void Initialize()
        {
            this.GameObjects = new GameObjectCollection();

            var netMgr = ServiceProvider.Instance.GetService<NetworkManager>();
            netMgr.HookWorldHandler(SMSG_CHAR_LOGIN);
            netMgr.HookWorldHandler(SMSG_CHAR_UPDATE);
        }

        #region Public Methods

        public DynamicWorldObject CreateDynamicWorldObject(uint objId, string name, WorldObjectType objType)
        {
            return new DynamicWorldObject(objId, objType)
            {
                Name = name,
            };
        }

        public void CreateNetworkLoop(int fps)
        {
            this.networkLoop = new GameLoop()
            {
                Fps = fps,
            };
            this.networkLoop.Loop += this.NetworkLoop_Loop;
        }

        public DynamicWorldObject GetGameObject(uint id)
        {
            return this.GameObjects
                   .OfType<DynamicWorldObject>()
                   .FirstOrDefault(f => f.GameId.HighId.CompareTo(id) == 0);
        }

        #endregion

        #region Private & Protected Methods

        protected override void Dispose(bool disposing)
        {
            var netMgr = ServiceProvider.Instance.GetService<NetworkManager>();
            netMgr.WorldPacketHandler.Unregister(SMSG_CHAR_LOGIN);
            netMgr.WorldPacketHandler.Unregister(SMSG_CHAR_UPDATE);

            base.Dispose(disposing);
        }

        #endregion

        #region Server Response Messages

        [WorldMessage(GameOpCodes.SMSG_CHAR_UPDATE)]
        private static void SMSG_CHAR_UPDATE(NetworkClient _, NetMessage packet)
        {
            var sceneContainer = ServiceProvider.Instance.GetService<ISceneContainer>();
            if (sceneContainer.ActiveScene is GfxGameWorld gameWorld)
            {
                using (var r = packet.PacketReader)
                {
                    var objId = r.ReadUInt32();
                    var objType = (WorldObjectType)r.ReadUInt32();
                    var updateFlags = (ObjUpdateFlags)r.ReadUInt32();

                    var obj = gameWorld.ObjectManager.GetGameObject(objId);
                    if (obj is DynamicWorldObject dynObj)
                    {
                        if (updateFlags == ObjUpdateFlags.Destroy)
                        {
                            gameWorld.ObjectManager.GameObjects.Remove(dynObj);
                        }
                        else if (updateFlags == ObjUpdateFlags.Movement)
                        {
                            var velocity = r.ReadVector4();
                            dynObj.Transform.SetVelocity(velocity.X, velocity.Y, velocity.Z);
                            dynObj.Transform.SetRotationMomentum(velocity.W);
                        }
                    }
                }
            }
        }

        [WorldMessage(GameOpCodes.SMSG_PLAYER_LOGIN)]
        private static void SMSG_CHAR_LOGIN(NetworkClient _, NetMessage packet)
        {
            var sceneContainer = ServiceProvider.Instance.GetService<ISceneContainer>();
            if (sceneContainer.ActiveScene is GfxGameWorld gameWorld)
            {
                using (var r = packet.PacketReader)
                {
                    var objUpdateFlags = (ObjUpdateFlags)r.ReadUInt32();
                    var objType = (WorldObjectType)r.ReadUInt32();
                    var mapId = (MapId)r.ReadUInt32();
                    var objId = r.ReadUInt32();
                    var name = r.ReadString();

                    if (objUpdateFlags == ObjUpdateFlags.Create)
                    {
                        var obj = gameWorld.ObjectManager.CreateDynamicWorldObject(objId, name, objType);

                        gameWorld.ObjectManager.GameObjects.Add(obj);
                    }
                }
            }
        }

        #endregion

        #region Event Handlers


        private void NetworkLoop_Loop(Time frameTime)
        {
            foreach (var obj in this.GameObjects.OfType<DynamicWorldObject>())
            {
                if (obj.Enabled) { obj.NetworkUpdate(frameTime); }
            }
        }


        #endregion

    }
}
