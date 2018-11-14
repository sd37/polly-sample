using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace PollyHttpClientFactoryExampleCore21.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CatalogController : ControllerBase
    {

        private readonly IHttpClientFactory _httpClientFactory;

        public CatalogController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult> Get(int id)
        {
            string requestEndpoint = $"inventory/{id}";

            var httpClient = _httpClientFactory.CreateClient("RemoteServer");

            HttpResponseMessage response = await httpClient.GetAsync(requestEndpoint);
            
            if (response.IsSuccessStatusCode)
            { 
                int itemsInStock = await response.Content.ReadAsAsync<int>();
                return Ok(itemsInStock);
            }

            return StatusCode((int)response.StatusCode, response.Content.ReadAsStringAsync());
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            string requestEndpoint = $"inventory/{id}";

            var httpClient = _httpClientFactory.CreateClient("RemoteServer");

            HttpResponseMessage response = await httpClient.DeleteAsync(requestEndpoint);

            if (response.IsSuccessStatusCode)
            {
                return Ok();
            }

            return StatusCode((int)response.StatusCode, response.Content.ReadAsStringAsync());
        }
    }
}
