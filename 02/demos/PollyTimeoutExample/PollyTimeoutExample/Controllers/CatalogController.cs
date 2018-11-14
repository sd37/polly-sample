using System;
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

namespace PollyTimeoutExample.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    public class CatalogController : Controller
    {
        readonly TimeoutPolicy _timeoutPolicy;
        readonly RetryPolicy<HttpResponseMessage> _httpRetryPolicy;
        readonly FallbackPolicy<HttpResponseMessage> _httpRequestFallbackPolicy;

        readonly int _cachedResult = 0;

        public CatalogController()
        {
            _timeoutPolicy = Policy.TimeoutAsync(1); // throws TimeoutRejectedException if timeout of 1 second is exceeded
             
            _httpRetryPolicy =
                Policy.HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
                    .Or<TimeoutRejectedException>()
                    .RetryAsync(3);

            _httpRequestFallbackPolicy = Policy.HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
                .Or<TimeoutRejectedException>()
                .FallbackAsync(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new ObjectContent(_cachedResult.GetType(), _cachedResult, new JsonMediaTypeFormatter())
                });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var httpClient = GetHttpClient();
            string requestEndpoint = $"inventory/{id}";

            HttpResponseMessage response =
                await
                _httpRequestFallbackPolicy.ExecuteAsync(() =>
                    _httpRetryPolicy.ExecuteAsync(() =>
                        _timeoutPolicy.ExecuteAsync(
                            async token => await httpClient.GetAsync(requestEndpoint, token), CancellationToken.None)));
            
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
