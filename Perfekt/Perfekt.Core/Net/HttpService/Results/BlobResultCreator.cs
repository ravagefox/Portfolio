using Perfekt.Core.Net.HttpService.Serialization;

namespace Perfekt.Core.Net.HttpService.Results
{
    internal class BlobResultCreator : IResultCreator
    {
        public IResult<object> CreateResult(HttpResponseMessage response)
        {
            return BlobResult.FromResponse(response);
        }
    }
}
