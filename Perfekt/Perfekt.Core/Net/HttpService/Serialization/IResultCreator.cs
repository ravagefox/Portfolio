namespace Perfekt.Core.Net.HttpService.Serialization
{
    public interface IResultCreator
    {
        IResult<object> CreateResult(HttpResponseMessage response);
    }
}
