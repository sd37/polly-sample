using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Polly.Registry;
using Newtonsoft.Json;
using Polly;
using Polly.Retry;

namespace PollySharingPoliciesByRegistry.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    public class CatalogController : Controller
    {
        PolicyRegistry _policyRegistry;
        public CatalogController(PolicyRegistry policyRegistry)
        {
            _policyRegistry = policyRegistry;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var httpClient = GetHttpClient();
            string requestEndpoint = $"inventory/{id}";

            IAsyncPolicy<HttpResponseMessage> retryPolicy = _policyRegistry
                .Get<IAsyncPolicy<HttpResponseMessage>>("SimpleHttpWaitAndRetry");

            HttpResponseMessage response = await retryPolicy.ExecuteAsync(() => httpClient.GetAsync(requestEndpoint));

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
