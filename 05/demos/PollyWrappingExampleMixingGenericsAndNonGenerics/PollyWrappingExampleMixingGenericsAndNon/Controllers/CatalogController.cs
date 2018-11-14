using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Polly;
using Polly.Fallback;
using Polly.Retry;
using Polly.Timeout;
using Polly.Wrap;

namespace PollyWrappingExampleMixingGenericsAndNon.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    [SuppressMessage("ReSharper", "PrivateFieldCanBeConvertedToLocalVariable")]
    public class CatalogController : Controller
    {
        readonly int _cachedResult = 0;

        readonly TimeoutPolicy _timeoutPolicy;
        readonly RetryPolicy<HttpResponseMessage> _httpRetryPolicy;
        readonly FallbackPolicy<HttpResponseMessage> _httpRequestFallbackPolicy;
        private readonly PolicyWrap<HttpResponseMessage> _policyWrap;

        public CatalogController()
        { 
            _timeoutPolicy = Policy.TimeoutAsync(1, onTimeoutAsync:TimeoutDelegate); 

            _httpRetryPolicy =
                Policy.HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
                    .Or<TimeoutRejectedException>()
                    .RetryAsync(3, onRetry: HttpRetryPolicyDelegate);

            _httpRequestFallbackPolicy = Policy.HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
                .Or<TimeoutRejectedException>()
                .FallbackAsync(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new ObjectContent(_cachedResult.GetType(), _cachedResult, new JsonMediaTypeFormatter())
                }, onFallbackAsync:HttpRequestFallbackPolicyDelegate);

            _policyWrap = _httpRequestFallbackPolicy.WrapAsync(_httpRetryPolicy.WrapAsync(_timeoutPolicy)); 
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var httpClient = GetHttpClient();
            string requestEndpoint = $"inventory/{id}";
            
            HttpResponseMessage response = await _policyWrap.ExecuteAsync(
                async token => await httpClient.GetAsync(requestEndpoint, token), CancellationToken.None);

            if (response.IsSuccessStatusCode)
            {
                int itemsInStock = JsonConvert.DeserializeObject<int>(await response.Content.ReadAsStringAsync());
                return Ok(itemsInStock);
            }

            if (response.Content != null)
            {
                return StatusCode((int)response.StatusCode, response.Content.ReadAsStringAsync());
            }
            return StatusCode((int)response.StatusCode);
        }

        private Task TimeoutDelegate(Context context, TimeSpan timeSpan, Task arg3)
        {
            Debug.WriteLine("In OnTimeoutAsync");
            return Task.CompletedTask;
        }

        private void HttpRetryPolicyDelegate(DelegateResult<HttpResponseMessage> delegateResult, int i)
        {
            Debug.WriteLine("In HttpRetryPolicyDelegate");
        }

        private Task HttpRequestFallbackPolicyDelegate(DelegateResult<HttpResponseMessage> delegateResult, Context context)
        {
            Debug.WriteLine("In OnFallbackAsync");
            return Task.CompletedTask;
        }
        private HttpClient GetHttpClient()
        {
            var httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri(@"http://localhost:57696/api/");
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            return httpClient;
        }
    }
}
