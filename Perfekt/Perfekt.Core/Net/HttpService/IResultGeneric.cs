namespace Perfekt.Core.Net.HttpService
{
    public interface IResult<out T>
    {
        bool IsSuccess { get; }
        T Result { get; }
    }
}
