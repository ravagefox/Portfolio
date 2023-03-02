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

using System.Linq;
using Client.Data.Gos;
using Client.Data.Gos.Components;
using Engine.Core;
using Engine.Net.Messaging;
using Fallen.Common;

namespace Client.Data
{
    public sealed class WorldObjectManager
    {
        public GameObjectCollection GameObjects { get; }

        private GameLoop networkLoop;


        public WorldObjectManager()
        {
            this.GameObjects = new GameObjectCollection();

            this.networkLoop = new GameLoop();
            this.networkLoop.Fps = 60;
            this.networkLoop.Loop += this.NetworkLoop_Loop;
            this.networkLoop.RunLoop();
        }

        private void NetworkLoop_Loop(Time time)
        {
            foreach (var obj in this.GameObjects.OfType<DynamicWorldObject>())
            {
                obj.NetworkUpdate(time);
            }
        }

        public void Unload()
        {
            foreach (GameObject obj in this.GameObjects)
            {
                obj.Unload();
            }

            this.networkLoop.Loop -= this.NetworkLoop_Loop;
            this.networkLoop.EndLoop(true);
            this.networkLoop = null;

            this.GameObjects.Clear();
        }

        public DynamicWorldObject CreateWorldObject(uint id)
        {
            var obj = new DynamicWorldObject(id);
            // TODO: download update information pretaining to object.

            var objRenderer = new ModelRenderer();
            objRenderer.LoadModel("Doodads\\Box.m2x");
            objRenderer.LoadTexture("Textures\\g1779.png");

            obj.Transform.Scale = 1.0f;
            obj.AddComponent(objRenderer);

            this.GameObjects.Add(obj);
            return obj;
        }

        internal void UpdateWorldObject(PacketReader r)
        {
            var objId = r.ReadUInt32();
            var objType = (WorldObjectType)r.ReadUInt32();

            if (this.GameObjects.OfType<DynamicWorldObject>().FirstOrDefault(x => x.GameID.HighId.CompareTo(objId) == 0) is DynamicWorldObject obj)
            {
                var updateFlags = (ObjUpdateFlags)r.ReadUInt32();
                switch (updateFlags)
                {
                    case ObjUpdateFlags.Movement:
                        {
                            obj.Transform.SetVelocity(r.ReadSingle(), r.ReadSingle(), r.ReadSingle());
                            obj.Transform.SetContinuousRotation(0.0f, r.ReadSingle(), 0.0f);
                        }
                        break;

                    case ObjUpdateFlags.Destroy:
                        {
                            this.GameObjects.Remove(obj);
                        }
                        break;
                }
            }
        }
    }
}
