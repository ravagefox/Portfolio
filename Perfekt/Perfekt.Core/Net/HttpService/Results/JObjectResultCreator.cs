using Perfekt.Core.Net.HttpService.Serialization;

namespace Perfekt.Core.Net.HttpService.Results
{
    internal class JObjectResultCreator : IResultCreator
    {
        public IResult<object> CreateResult(HttpResponseMessage response)
        {
            return JObjectResult.FromResponse(response);
        }
    }
}
