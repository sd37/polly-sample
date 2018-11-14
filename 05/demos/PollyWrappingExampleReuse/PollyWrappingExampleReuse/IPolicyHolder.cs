using System.Net.Http;
using Polly;
using Polly.Wrap;

namespace PollyWrappingExampleReuse
{
    public interface IPolicyHolder
    {
        IAsyncPolicy<HttpResponseMessage> TimeoutPolicy { get; set; }
        IAsyncPolicy<HttpResponseMessage> HttpRetryPolicy { get; set; }
        IAsyncPolicy<HttpResponseMessage> HttpRequestFallbackPolicy { get; set; }

        PolicyWrap<HttpResponseMessage> TimeoutRetryAndFallbackWrap { get; set; }
    }
}