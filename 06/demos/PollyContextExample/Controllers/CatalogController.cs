using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using Polly;
using Polly.Retry;

namespace PollyContextExample.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    public class CatalogController : Controller
    {
        readonly RetryPolicy<HttpResponseMessage> _httpRetryPolicy;
        private readonly HttpClient _httpClient;

        public CatalogController(RetryPolicy<HttpResponseMessage> httpRetryPolicy, HttpClient httpClient)
        {
            _httpRetryPolicy = httpRetryPolicy;
            _httpClient = httpClient;
        }
        
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            string requestEndpoint = $"inventory/{id}";

            var host = Request.Headers.FirstOrDefault(h => h.Key == "Host").Value;
            var userAgent = Request.Headers.FirstOrDefault(h => h.Key == "User-Agent").Value;

            IDictionary<string, object> contextDictionary = new Dictionary<string, object>
            {
                { "Host", host }, {"CatalogId", id}, {"User-Agent", userAgent}
            };

            Context context = new Context("CatalogContext", contextDictionary);

            HttpResponseMessage response = await _httpRetryPolicy.ExecuteAsync(
                () => _httpClient.GetAsync(requestEndpoint), context);

            if (response.IsSuccessStatusCode)
            {
                int itemsInStock = JsonConvert.DeserializeObject<int>(await response.Content.ReadAsStringAsync());
                return Ok(itemsInStock);
            }

            return StatusCode((int)response.StatusCode, response.Content.ReadAsStringAsync());
        }
    }
}
