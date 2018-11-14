using System;
using System.Net.Http;
using Polly;

namespace PollyDIExample
{
    public class PolicyHolder : IPolicyHolder
    {
        public IAsyncPolicy<HttpResponseMessage> HttpRetryPolicy { get;  set; }
        public IAsyncPolicy HttpClientTimeoutExceptionPolicy { get; set; }

        public PolicyHolder()
        {
            HttpRetryPolicy =
                Policy.HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
                    .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(retryAttempt), (response, timespan) =>
                    {
                        var result = response.Result;
                        // log the result
                    });

            HttpClientTimeoutExceptionPolicy = Policy.Handle<HttpRequestException>()
                .WaitAndRetryAsync(1, retryAttempt => TimeSpan.FromSeconds(retryAttempt),
                    onRetry: (exception, timespan) =>
                    {
                        string message = exception.Message;
                        // log the message.
                    }
                );
        }
    }
}
