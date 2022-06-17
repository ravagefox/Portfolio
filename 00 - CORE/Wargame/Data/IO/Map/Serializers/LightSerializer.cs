// Source: LightSerializer
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
using Wargame.Data.Gos;
using Wargame.Data.Graphics.DeferredContext;

namespace Wargame.Data.IO.Map.Serializers
{
    public sealed class LightSerializer : ISerializationObject
    {
        public List<PointLight> Lights { get; }

        private bool isReadonly;


        public LightSerializer(PointLightRenderSystem lightSystem)
        {
            this.Lights = lightSystem.GetLights().Select(l => (PointLight)l).ToList();
            this.isReadonly = false;
        }
        public LightSerializer()
        {
            this.Lights = new List<PointLight>();
            this.isReadonly = true;
        }



        public void Deserialize(MapReader reader)
        {
            if (!this.isReadonly)
            {
                throw new Exception("Can only serialize to the stream.");
            }

            var lightCount = reader.ReadInt32();
            for (var i = 0; i < lightCount; i++)
            {
                var pointLight = reader.ReadGameObject();
                if (pointLight != null)
                {
                    this.Lights.Add((PointLight)pointLight);
                }
            }
        }

        public void Serialize(MapWriter writer)
        {
            if (this.isReadonly)
            {
                throw new Exception("Can only deserialize from the stream.");
            }

            writer.Write(this.Lights.Count);
            foreach (var light in this.Lights)
            {
                writer.Write(light);
            }
        }
    }
}
