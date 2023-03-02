// Source: MimeLookup
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
using System.Collections.Generic;
using System.Net.Mime;

namespace WebLib.IO
{
    /// <summary>
    /// Returns builtin <see cref="ContentType"/> objects which defines
    /// the media types.
    /// </summary>
    public static class MimeLookup
    {
#pragma warning disable
        // TEXT
        public static ContentType Html => new ContentType("text/html;charset=UTF-8");
        public static ContentType Javascript => new ContentType("text/javascript;charset=UTF-8");
        public static ContentType Xml => new ContentType("text/xml;charset=UTF-8");
        public static ContentType Style => new ContentType("text/css;charset=UTF-8");
        public static ContentType Plain => new ContentType("text/plain;charset=UTF-8");

        // VIDEO
        public static ContentType Mp4 => new ContentType("video/mp4;charset=UTF-8");
        public static ContentType Mpeg => new ContentType("video/mpeg;charset=UTF-8");

        // IMAGE
        public static ContentType Svg => new ContentType("image/svg+xml;charset=UTF-8");
        public static ContentType Png => new ContentType("image/png;charset=UTF-8");
        public static ContentType Jpg => new ContentType("image/jpeg;charset=UTF-8");
        public static ContentType Gif => new ContentType("image/gif;charset=UTF-8");

        // APPLICATION
        public static ContentType FormData => new ContentType("application/x-www-form-urlencoded;charset=UTF-8");
        public static ContentType Json => new ContentType("application/json;charset=UTF-8");
        public static ContentType Binary => new ContentType("application/octet-stream;charset=UTF-8");



        private static readonly Dictionary<string, ContentType> types =
            new Dictionary<string, ContentType>()
            {
                { ".xml", Xml },
                { ".html", Html },
                { ".htm", Html },
                { ".css", Style },
                { ".js", Javascript },
                { ".json", Json },
                { ".mp4", Mp4 },
                { ".mpeg", Mpeg },
                { ".mpg", Mpeg },
                { ".svg", Svg },
                { ".png", Png },
                { ".jpeg", Jpg },
                { ".gif", Gif },

                { "", Binary },
            };

        /// <summary>
        /// Returns the appropriate <see cref="ContentType"/> given by 
        /// the extension.
        /// </summary>
        /// <param name="ext"></param>
        /// <returns></returns>
        public static ContentType GetContentType(string ext)
        {
            if (!string.IsNullOrEmpty(ext))
            {
                if (!types.TryGetValue(ext, out var type))
                {
                    type = Plain;
                }

                return type;
            }

            return Plain;
        }
    }
}
