
using Perfekt.Core.Net.HttpService.Serialization;

namespace Perfekt.Core.Net.HttpService.Results
{
    internal class StringResultCreator : IResultCreator
    {
        public IResult<object> CreateResult(HttpResponseMessage response)
        {
            return StringResult.FromResponse(response);
        }
    }
}
