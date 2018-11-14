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

namespace PollyWrappingExampleReuse.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    [SuppressMessage("ReSharper", "PrivateFieldCanBeConvertedToLocalVariable")]
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

            HttpResponseMessage response = await  _policyHolder.TimeoutRetryAndFallbackWrap.ExecuteAsync(
                async token => await _httpClient.GetAsync(requestEndpoint, token), CancellationToken.None);

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
    }
}
