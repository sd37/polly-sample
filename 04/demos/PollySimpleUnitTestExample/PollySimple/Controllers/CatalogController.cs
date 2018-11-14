using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Polly;

namespace PollySimple.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    public class CatalogController : Controller
    {
        readonly IAsyncPolicy<HttpResponseMessage> _httpRetryPolicy;
        private readonly HttpClient _httpClient;

        public CatalogController(IAsyncPolicy<HttpResponseMessage> httpRetryPolicy, HttpClient httpClient)
        {
            _httpRetryPolicy = httpRetryPolicy;
            _httpClient = httpClient;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            string requestEndpoint = $"inventory/{id}";

            HttpResponseMessage response = await _httpRetryPolicy.ExecuteAsync(() => _httpClient.GetAsync(requestEndpoint));

            if (response.IsSuccessStatusCode)
            {
                int itemsInStock = JsonConvert.DeserializeObject<int>(await response.Content.ReadAsStringAsync());
                return Ok(itemsInStock);
            }

            return StatusCode((int)response.StatusCode, response.Content.ReadAsStringAsync());
        }
    }
}
