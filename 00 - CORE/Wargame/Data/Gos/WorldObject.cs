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

using System;
using Engine.Core;
using Engine.Data;
using Wargame.Data.Gos.Components;
using Wargame.Data.IO.Map;

namespace Wargame.Data.Gos
{
    public class WorldObject : GameObject, ISerializationObject
    {
        public Transform Transform { get; private set; }


        public event EventHandler Serializing;
        public event EventHandler Deserializing;


        public WorldObject() : base()
        {
            this.Transform = new Transform();
        }

        public override void Initialize()
        {
            //this.AddComponent(this.Transform, false);

            base.Initialize();
        }


        protected virtual void OnSerialize(object sender, MapWriter writer)
        {
            writer.Write(this.Name);
            writer.Write(this.Id);

            writer.Write(this.Transform);

            Serializing?.Invoke(sender, EventArgs.Empty);
        }

        protected virtual void OnDeserialize(object sender, MapReader reader)
        {
            this.Name = reader.ReadString();
            this.Id = new AssetId(reader.ReadGuid());

            this.Transform = (Transform)reader.ReadGameComponent();
            this.AddComponent(this.Transform);

            Deserializing?.Invoke(sender, EventArgs.Empty);
        }



        public void Serialize(MapWriter writer)
        {
            this.OnSerialize(this, writer);
        }
        public void Deserialize(MapReader reader)
        {
            this.OnDeserialize(this, reader);
        }
    }
}
