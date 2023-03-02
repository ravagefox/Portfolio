using Perfekt.Core.Net.HttpService.Results;
using System.Net.Http.Headers;
using System.Security.Cryptography;

namespace Perfekt.Core.Net.HttpService
{
    public class PerfektWebClient : IDisposable
    {
        public TimeSpan DefaultTimeout { get; set; }

        public string? ExtensionUri { get; set; }

        #region Fields
        private string scheme;
        private string host;
        private ushort port;
        private HttpClient webClient;
        private Dictionary<string, IEnumerable<string>> headers;


        public event EventHandler Disposed;
        public event EventHandler ApplyCustomHeaders;

        #endregion

        #region Constructors

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public PerfektWebClient(string hostnameOrIp, string scheme, ushort port)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        {
            this.DefaultTimeout = TimeSpan.FromSeconds(5.0);
            this.ExtensionUri = string.Empty;

            this.host = hostnameOrIp;
            this.scheme = scheme;
            this.port = port;

            this.headers = new Dictionary<string, IEnumerable<string>>();
            this.webClient = new HttpClient();
        }

        #endregion


        #region Protected Virtual Methods

        protected virtual void OnDisposed(object sender, EventArgs e)
        {
            Disposed?.Invoke(sender, e);
        }

        protected virtual void OnApplyCustomHeaders(object sender, EventArgs e)
        {
            ApplyCustomHeaders?.Invoke(sender, e);
        }
        #endregion

        #region Public Methods
        public void SetHeader(string headerName, string value)
        {
            this.SetHeader(headerName, new[] { value });
        }

        public void SetHeader(string headerName, IEnumerable<string> values)
        {
            this.headers[headerName] = values;
        }

        public Uri BuildUri(string relativePath)
        {
            if (string.IsNullOrEmpty(relativePath)) { return GetBaseUri(); }
            else
            {
                var truePath = relativePath;
                if (truePath.StartsWith("/"))
                {
                    truePath = truePath.Substring(1);
                }

                var baseUri = GetBaseUri();
                return new Uri(baseUri, Uri.EscapeDataString(truePath));
            }
        }

        public Uri BuildUri(string relativePath, Dictionary<string, string> queries)
        {
            var queryPath = "?";

            for (var i = 0; i < queries.Count; i++)
            {
                var query = $"{queries.ElementAt(i).Key}={queries.ElementAt(i).Value}";
                if (i != queries.Count - 1)
                {
                    query += "&";
                }

                queryPath += query;
            }

            return new Uri(BuildUri(relativePath), Uri.EscapeDataString(queryPath));
        }

        public Uri GetBaseUri()
        {
            var uriString =
                $"{scheme}://{host}:{port}/";

            if (!string.IsNullOrEmpty(ExtensionUri))
            {
                var extUri = ExtensionUri;
                if (extUri.StartsWith("/"))
                {
                    extUri = extUri.Substring(1);
                }

                uriString += extUri;
            }

            return new Uri(Uri.EscapeDataString(uriString));
        }

        public void Dispose()
        {
            webClient?.Dispose();

            OnDisposed(this, EventArgs.Empty);

            GC.SuppressFinalize(this);
        }


        private IResult<TResult> Send<TResult>(Uri uriRequest, HttpMethod method, Stream stream, ResultFactory factory)
        {
            ThrowIfNull(uriRequest);
            ThrowIfNull(webClient);

            IResult<TResult> result = new NullResult<TResult>();

            using (var dStream = stream ?? new MemoryStream())
            using (var streamContent = new StreamContent(dStream))
            {
                dStream.Seek(0, SeekOrigin.Begin);
                SetDefaultHeaders(streamContent, dStream);

                var requestMessage = new HttpRequestMessage(method, uriRequest)
                {
                    Content = streamContent,
                };

                ApplyHeaders(requestMessage);

                var taskResult = Task.Run(async () => await webClient.SendAsync(requestMessage));
                if (taskResult.Wait(DefaultTimeout) &&
                    taskResult.Status == TaskStatus.RanToCompletion)
                {
                    result = factory.CreateRequest<TResult>(taskResult.Result);
                    if (!result.IsSuccess)
                    {
                        // TODO: implement failed debug trace
                        result = new FailedResult<TResult>(
                            taskResult.Result.ReasonPhrase ?? string.Empty,
                            (int)taskResult.Result.StatusCode);
                    }
                }

                headers.Clear();
                streamContent?.Dispose();
                dStream?.Close();
            }

            return result;
        }

        public IResult<TResult> Post<TResult>(Uri uriRequest, HttpContent content, ResultFactory factory)
        {
            var requestMessage = new HttpRequestMessage(HttpMethod.Post, uriRequest)
            {
                Content = content,
            };

            ApplyHeaders(requestMessage);

            IResult<TResult> result = new NullResult<TResult>();

            var taskResult = Task.Run(async () => await webClient.SendAsync(requestMessage));
            if (taskResult.Wait(DefaultTimeout) &&
                taskResult.Status == TaskStatus.RanToCompletion)
            {
                result = factory.CreateRequest<TResult>(taskResult.Result);
                if (!result.IsSuccess)
                {
                    // TODO: implement failed debug trace
                    result = new FailedResult<TResult>(
                        taskResult.Result.ReasonPhrase ?? string.Empty,
                        (int)taskResult.Result.StatusCode);
                }
            }

            headers.Clear();
            return result;
        }

        public IResult<TResult> Post<TResult>(Uri uriRequest, Stream dataStream, ResultFactory factory)
        {
            return Send<TResult>(uriRequest, HttpMethod.Post, dataStream, factory);
        }

        public IResult<TResult> Get<TResult>(Uri uriRequest, ResultFactory factory)
        {
            return Send<TResult>(uriRequest, HttpMethod.Get, new MemoryStream(), factory);
        }

        public IResult<TResult> Delete<TResult>(Uri uriRequest, Stream dataStream, ResultFactory factory)
        {
            return Send<TResult>(uriRequest, HttpMethod.Delete, dataStream, factory);
        }

        public IResult<TResult> Put<TResult>(Uri uriRequest, Stream dataStream, ResultFactory factory)
        {
            return Send<TResult>(uriRequest, HttpMethod.Put, dataStream, factory);
        }

        public IResult<TResult> Put<TResult>(Uri uriRequest, Stream dataStream)
        {
            return Put<TResult>(uriRequest, dataStream, ResultFactory.DefaultFactory);
        }

        public IResult<TResult> Get<TResult>(Uri uriRequest)
        {
            return Get<TResult>(uriRequest, ResultFactory.DefaultFactory);
        }

        public IResult<TResult> Post<TResult>(Uri uriRequest, Stream dataStream)
        {
            return Post<TResult>(uriRequest, dataStream, ResultFactory.DefaultFactory);
        }

        public IResult<TResult> Delete<TResult>(Uri uriRequest, Stream dataStream)
        {
            return Delete<TResult>(uriRequest, dataStream, ResultFactory.DefaultFactory);
        }

        #endregion

        #region Private Methods

        private void ApplyHeaders(HttpRequestMessage requestMessage)
        {
            OnApplyCustomHeaders(this, EventArgs.Empty);

            foreach (var headerPair in this.headers)
            {
                requestMessage.Headers.Add(headerPair.Key, headerPair.Value);
            }

        }

        private void ThrowIfNull(object obj)
        {
            if (obj is null) { throw new NullReferenceException(nameof(obj) + " cannot be null."); }
        }

        private static void SetDefaultHeaders(StreamContent streamContent, Stream dataStream)
        {
            streamContent.Headers.ContentLength = dataStream.Length;
            streamContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            streamContent.Headers.ContentMD5 = ComputeMd5Hash(dataStream);
        }

        private static byte[] ComputeMd5Hash(Stream dataStream)
        {
            if (!dataStream.CanSeek || !dataStream.CanRead)
            {
                throw new NotSupportedException(nameof(dataStream) + " cannot seek or read.");
            }

            using (var copyStream = new MemoryStream())
            {
                dataStream.CopyTo(copyStream);
                _ = dataStream.Seek(0, SeekOrigin.Begin);
                _ = copyStream.Seek(0, SeekOrigin.Begin);

                return MD5.HashData(copyStream.ToArray());
            }
        }

        #endregion
    }
}
