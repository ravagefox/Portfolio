namespace Perfekt.Core.Net.HttpService.Results
{
    public sealed class NullResult<TResult> : IResult<TResult>
    {
        public bool IsSuccess => false;

        public TResult Result => throw new NullReferenceException();
    }
}
