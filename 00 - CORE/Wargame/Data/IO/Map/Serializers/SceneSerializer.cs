// Source: SceneSerializer
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

namespace Wargame.Data.IO.Map.Serializers
{
    public sealed class SceneSerializer : ISerializationObject
    {
        public GameObjectCollection Actors { get; }


        private bool readonlyMode;


        public SceneSerializer()
        {
            this.readonlyMode = true;
            this.Actors = new GameObjectCollection();
        }

        public SceneSerializer(GameObjectCollection actors)
        {
            this.readonlyMode = false;
            this.Actors = actors;
        }


        public void Deserialize(MapReader reader)
        {
            if (!this.readonlyMode)
            {
                throw new Exception("Can only serialize to the stream.");
            }

            var actorCount = reader.ReadInt32();
            for (var i = 0; i < actorCount; i++)
            {
                var gameObject = reader.ReadGameObject();
                if (gameObject != null)
                {
                    this.Actors.Add(gameObject);
                }
            }
        }

        public void Serialize(MapWriter writer)
        {
            if (this.readonlyMode) { throw new Exception("Can only deserialize from the stream."); }

            writer.Write(this.Actors.Count);
            foreach (var actor in this.Actors)
            {
                writer.Write(actor);
            }
        }
    }
}
