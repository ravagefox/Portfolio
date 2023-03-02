namespace Perfekt.Core.Net.HttpService.Results
{
#pragma warning disable CS8603 // Possible null reference return.
    internal class TimeoutResult<TResult> : IResult<TResult>
    {
        public bool IsSuccess => false;

        public TResult Result => default;

        public Type ResultType => typeof(TResult);

        public Exception Error => new Exception("The service timed out");

        public Uri UriRequest { get; }


        internal TimeoutResult(Uri uriRequest)
        {
            this.UriRequest = uriRequest;
        }
    }
}
