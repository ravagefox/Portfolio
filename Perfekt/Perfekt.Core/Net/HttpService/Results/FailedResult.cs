namespace Perfekt.Core.Net.HttpService.Results
{
    public sealed class FailedResult<TResult> : IResult<TResult>
    {
        public bool IsSuccess => false;

        public TResult Result
        {
            get
            {
                TResult? result = default;
#pragma warning disable CS8603 // Possible null reference return.
                return result;
#pragma warning restore CS8603 // Possible null reference return.
            }
        }


        public string Reason { get; }

        public int StatusCode { get; }


        public FailedResult(string reason, int statusCode)
        {
            this.Reason = reason;
            this.StatusCode = statusCode;
        }
    }
}
