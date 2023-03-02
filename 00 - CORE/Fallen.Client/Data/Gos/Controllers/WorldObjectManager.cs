// Source: WorldObjectManager
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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Engine.Core;
using Engine.Net.Messaging;
using Fallen.Common;

namespace Client.Data.Gos.Controllers
{
    public sealed class WorldObjectManager : ControllerBase<WorldObject>
    {
        public GameObjectCollection GameObjects { get; }


        private readonly Dictionary<WorldObjectType, Func<uint, WorldObject>> CreateObjects = new Dictionary<WorldObjectType, Func<uint, WorldObject>>()
        {
            { WorldObjectType.Player, (id) => new PlayerObject(id) },
        };


        public WorldObjectManager(GameObjectCollection gameObjects)
        {
            this.GameObjects = gameObjects;
        }

        public WorldObject InvokeObjectUpdateOrCreate(NetMessage msg)
        {
            var updateFlags = (ObjUpdateFlags)msg.PacketReader.ReadUInt32();
            if (updateFlags == ObjUpdateFlags.Create)
            {
                var objType = (WorldObjectType)msg.PacketReader.ReadUInt32();
                if (this.CreateObjects.TryGetValue(objType, out var func))
                {
                    var id = msg.PacketReader.ReadUInt32();
                    var newObj = func(id);
                    this.GameObjects.Add(newObj);

                    this.InvokeNetworkReceive(newObj, msg, updateFlags);
                    return newObj;
                }
            }
            else
            {
                var id = msg.PacketReader.ReadUInt32();
                var existingObj = this.GameObjects.OfType<WorldObject>().FirstOrDefault(x => x.GameID.HighId.CompareTo(id) == 0);
                if (existingObj != null)
                {
                    this.InvokeNetworkReceive(existingObj, msg, updateFlags);
                    return existingObj;
                }
            }

            return null;
        }

        private void InvokeNetworkReceive(WorldObject obj, NetMessage msg, ObjUpdateFlags updateFlags)
        {
            var flags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance;

            var recMethod = obj.GetType().GetMethod("OnNetworkReceive", flags);
            if (recMethod != null)
            {
                recMethod.Invoke(obj, new object[] { msg, updateFlags });
            }
        }
    }
}
