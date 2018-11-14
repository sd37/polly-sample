using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using Polly;
using Polly.Timeout;
using Polly.Wrap;

namespace PollyWrappingExampleReuse
{
    public class PolicyHolder : IPolicyHolder
    {
        public IAsyncPolicy<HttpResponseMessage> TimeoutPolicy { get; set; }
        public IAsyncPolicy<HttpResponseMessage> HttpRetryPolicy { get; set; }
        public IAsyncPolicy<HttpResponseMessage> HttpRequestFallbackPolicy { get; set; }

        public PolicyWrap<HttpResponseMessage> TimeoutRetryAndFallbackWrap { get; set; }

        readonly int _cachedResult = 0;

        public PolicyHolder()
        {
            TimeoutPolicy = Policy.TimeoutAsync<HttpResponseMessage>(1); // throws TimeoutRejectedException if timeout of 1 second is exceeded

            HttpRetryPolicy =
                Policy.HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
                    .Or<TimeoutRejectedException>()
                    .RetryAsync(3);

            HttpRequestFallbackPolicy = Policy.HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
                .Or<TimeoutRejectedException>()
                .FallbackAsync(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new ObjectContent(_cachedResult.GetType(), _cachedResult, new JsonMediaTypeFormatter())
                });

            TimeoutRetryAndFallbackWrap = Policy.WrapAsync(HttpRequestFallbackPolicy, HttpRetryPolicy, TimeoutPolicy);
        }
    }
}
