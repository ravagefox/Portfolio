using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics.CodeAnalysis;

namespace Perfekt.Core.Net.HttpService.Results
{
    public sealed class JObjectResult : IResult<JToken>
    {
        [AllowNull]
        public JToken Result { get; private set; }
        public bool IsSuccess { get; private set; }


        public T ConvertTo<T>()
        {
            if (Result == null)
            {
                throw new NullReferenceException(
                    "The result object cannot be null.");
            }

            var result = Result.Root.ToObject<T>();
            if (result == null)
            {
                throw new Exception(
                    "Unable to cast object result to another type.");
            }

            return result;
        }

        public static JObjectResult FromResponse(HttpResponseMessage msg)
        {
            var reader = new JsonTextReader(new StreamReader(msg.Content.ReadAsStream()));

            var json = JToken.ReadFrom(reader);
            var jsonResult = new JObjectResult()
            {
                IsSuccess = msg.IsSuccessStatusCode,
                Result = json,
            };

            reader?.Close();
            return jsonResult;
        }
    }
}
