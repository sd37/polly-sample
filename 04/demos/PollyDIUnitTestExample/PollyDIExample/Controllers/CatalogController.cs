using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Polly;
using Polly.Registry;

namespace PollyDIExample.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    public class CatalogController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly IPolicyHolder _policyHolder;

        public CatalogController(IPolicyHolder policyHolder, HttpClient httpClient)
        {
            _policyHolder = policyHolder;
            _httpClient = httpClient;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            string requestEndpoint = $"inventory/{id}";

            HttpResponseMessage response =
                await _policyHolder.HttpRetryPolicy.ExecuteAsync(
                    () => _policyHolder.HttpClientTimeoutExceptionPolicy.ExecuteAsync(
                        async token => await _httpClient.GetAsync(requestEndpoint, token), CancellationToken.None));

            if (response.IsSuccessStatusCode)
            {
                int itemsInStock = JsonConvert.DeserializeObject<int>(await response.Content.ReadAsStringAsync());
                return Ok(itemsInStock);
            }

            return StatusCode((int)response.StatusCode, response.Content.ReadAsStringAsync());
        }
    }
}
