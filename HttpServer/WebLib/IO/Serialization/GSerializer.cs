// Source: GSerializer
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
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace WebLib.IO.Serialization
{
    /// <summary>
    /// Provides a simple serializer for XML formatting.
    /// </summary>
    public static class GSerializer
    {
        /// <summary>
        /// Returns the deserialized object from the specified stream.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static T Deserialize<T>(Stream source)
        {
            var xmlSerializer = new XmlSerializer(typeof(T));
            return (T)xmlSerializer.Deserialize(source);
        }

        /// <summary>
        /// Returns the serialized XML text of the specified object.
        /// </summary>
        /// <param name="graph"></param>
        /// <returns></returns>
        public static string Serialize(object graph)
        {
            var xmlSerializer = new XmlSerializer(graph.GetType());
            var encoding = Encoding.UTF8;
            var xmlSettings = new XmlWriterSettings()
            {
                Indent = true,
                IndentChars = "\t",
            };

            using (var sw = new StreamWriter(new MemoryStream()))
            {
                using (var xmlWriter = XmlWriter.Create(sw, xmlSettings))
                {
                    xmlSerializer.Serialize(xmlWriter, graph);

                    var bs = sw.BaseStream as MemoryStream;
                    _ = bs.Seek(0, SeekOrigin.Begin);

                    var data = bs.ToArray();
                    return encoding.GetString(data);
                }
            }
        }
    }
}
