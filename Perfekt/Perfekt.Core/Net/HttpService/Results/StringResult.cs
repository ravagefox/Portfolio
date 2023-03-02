namespace Perfekt.Core.Net.HttpService.Results
{
    public struct StringResult : IResult<string>
    {
        public bool IsSuccess { get; private set; }

        public string Result { get; private set; }


        public static StringResult FromResponse(HttpResponseMessage msg)
        {
            return new StringResult()
            {
                IsSuccess = msg.IsSuccessStatusCode,
                Result = msg.Content.ReadAsStringAsync().GetAwaiter().GetResult(),
            };
        }
    }
}
