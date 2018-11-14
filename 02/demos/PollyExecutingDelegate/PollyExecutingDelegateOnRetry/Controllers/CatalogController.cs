using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Polly;
using Polly.Retry;

namespace PollyExecutingDelegateOnRetry.Controllers
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
                    .RetryAsync(3, onRetry: (httpResponseMessage, retryCount) =>
                    {
                        if (httpResponseMessage.Result.StatusCode == HttpStatusCode.NotFound)
                        {
                            //log somewhere
                        }
                        else if(httpResponseMessage.Result.StatusCode == HttpStatusCode.Conflict)
                        {
                            //do something else
                        }
                    });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var httpClient = GetHttpClient();
            string requestEndpoint = $"doesNotExist/";

            HttpResponseMessage response = await _httpRetryPolicy.ExecuteAsync(() => httpClient.GetAsync(requestEndpoint));

            if (response.IsSuccessStatusCode)
            {
                int itemsInStock = JsonConvert.DeserializeObject<int>(await response.Content.ReadAsStringAsync());
                return Ok(itemsInStock);
            }

            return StatusCode((int)response.StatusCode, response.Content.ReadAsStringAsync());
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
