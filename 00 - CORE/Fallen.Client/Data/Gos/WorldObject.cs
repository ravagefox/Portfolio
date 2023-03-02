// Source: WorldObject
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

using Client.Data.Gos.Components;
using Client.Data.IO;
using Engine.Core;
using Engine.Data;
using Fallen.Common;
using Microsoft.Xna.Framework.Graphics;

namespace Client.Data.Gos
{
    public abstract class WorldObject : GameObject
    {
        public GameID GameID { get; }

        public Transform Transform { get; }

        public AssetManager Content => ServiceProvider.Instance.GetService<AssetManager>();

        public GraphicsDevice GraphicsDevice => ServiceProvider.Instance.GetService<IGraphicsDeviceService>().GraphicsDevice;

        public MapId MapId { get; set; }
        public abstract WorldObjectType ObjectType { get; }



        public WorldObject(uint objId) : base()
        {
            this.GameID = new GameID(objId);
            this.Transform = new Transform();

            this.AddComponent(this.Transform);
        }

        public override void Initialize()
        {
            base.Initialize();
        }
    }
}
