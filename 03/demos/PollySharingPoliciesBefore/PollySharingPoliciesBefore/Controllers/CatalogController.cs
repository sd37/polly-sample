using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Polly.Retry;
using Polly;

namespace PollySharingPoliciesBefore.Controllers
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
              .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(retryAttempt));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var httpClient = GetHttpClient();
            string requestEndpoint = $"inventory/{id}";

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
