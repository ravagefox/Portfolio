using System.Text;

namespace Perfekt.Core.IO
{
    public static class HttpStream
    {
        public static Stream CreateEmptyStream()
        {
            return new MemoryStream();
        }

        public static Stream CreateStream(Action<StreamWriter> contentWriter)
        {
            var ms = new MemoryStream();
            var sw = new StreamWriter(ms, Encoding.UTF8, leaveOpen: true);
            contentWriter?.Invoke(sw);

            sw?.Close();
            return ms;
        }

        public static FormUrlEncodedContent CreateUrlEncodedStream(params KeyValuePair<string, string>[] formData)
        {
            return new FormUrlEncodedContent(formData);
        }
    }
}
