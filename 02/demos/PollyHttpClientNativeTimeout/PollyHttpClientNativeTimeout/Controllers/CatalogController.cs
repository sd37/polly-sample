using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Polly;
using Polly.Retry;

namespace PollyHttpClientNativeTimeout.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    public class CatalogController : Controller
    {
        readonly RetryPolicy<HttpResponseMessage> _httpRetryPolicy;

        public CatalogController()
        {
            _httpRetryPolicy =
                Policy.HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
                    .Or<HttpRequestException>()
                    .RetryAsync(1, onRetry:OnRetry);
        }

        private void OnRetry(DelegateResult<HttpResponseMessage> delegateResult, int retryCount)
        {
            if (delegateResult.Exception is HttpRequestException)
            {
                if (delegateResult.Exception.GetBaseException().Message == "The operation timed out")
                {
                    // log something about the timeout       
                }
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var httpClient = GetHttpClient();
            string requestEndpoint = $"inventory/{id}";

            //HttpResponseMessage response = await _httpRetryPolicy.ExecuteAsync(() => httpClient.GetAsync(requestEndpoint));
            HttpResponseMessage response = await httpClient.GetAsync(requestEndpoint);

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
            httpClient.BaseAddress = new Uri(@"http://10.255.255.1/someUnreachableEndpoint/"); //unreachable address, will cause a timeout
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            return httpClient;
        }
    }
}
