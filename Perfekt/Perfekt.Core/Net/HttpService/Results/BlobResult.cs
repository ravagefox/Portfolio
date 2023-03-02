namespace Perfekt.Core.Net.HttpService.Results
{
    public struct BlobResult : IResult<byte[]>
    {
        public byte[] Result { get; private set; }

        public bool IsSuccess { get; private set; }


        public static BlobResult FromResponse(HttpResponseMessage msg)
        {
            return new BlobResult()
            {
                IsSuccess = msg.IsSuccessStatusCode,
                Result = GetBlob(msg.Content.ReadAsStream()),
            };
        }

        private static byte[] GetBlob(Stream stream)
        {
            using (var ms = new MemoryStream())
            {
                stream.CopyTo(ms);

                _ = ms.Seek(0, SeekOrigin.Begin);
                return ms.ToArray();
            }
        }
    }
}
