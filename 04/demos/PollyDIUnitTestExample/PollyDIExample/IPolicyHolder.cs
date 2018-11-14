using System.Net.Http;
using Polly;

namespace PollyDIExample
{
    public interface IPolicyHolder
    {
        IAsyncPolicy HttpClientTimeoutExceptionPolicy { get; set; }
        IAsyncPolicy<HttpResponseMessage> HttpRetryPolicy { get; set; }
    }
}