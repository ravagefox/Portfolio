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

using System.IO;
using System.Linq;
using Engine.Core;
using Engine.Data;
using Wargame.Data.Scenes;

namespace Wargame.Data.IO.Map.Serializers
{
    public sealed class SceneSerializer : IObjectSerializer<Scene>
    {
        public void Serialize(Scene instance, MapWriter writer)
        {
            writer.Write(instance.Name);
            writer.Write(instance.Id.Id);

            writer.Write(instance.Actors.Count);
            foreach (var actor in instance.Actors)
            {
                if (actor is IObjectSerialization objectSerialization)
                {
                    writer.Write(actor.GetType().Name);

                    objectSerialization.Serialize(writer);
                    this.SerializeComponents(actor, writer);
                }
            }
        }

        private void SerializeComponents(GameObject actor, MapWriter writer)
        {
            var components = actor.AllComponents.OfType<IObjectSerialization>();
            writer.Write(components.Count());
            foreach (var component in components)
            {
                writer.Write(component.GetType().Name);
                component.Serialize(writer);
            }
        }

        public static Scene Deserialize(Stream instream)
        {
            using (var reader = new MapReader(instream))
            {
                var scene = new WorldScene();

                var worldScene = new WorldScene();
                worldScene.Name = reader.ReadString();
                worldScene.Id = new AssetId(reader.ReadGuid());

                var actorCount = reader.ReadInt32();
                for (var i = 0; i < actorCount; i++)
                {
                    var actor = reader.ReadGameObject();
                    if (actor != null)
                    {
                        worldScene.Actors.Add(actor);
                    }
                }

                return scene;
            }
        }
    }
}
