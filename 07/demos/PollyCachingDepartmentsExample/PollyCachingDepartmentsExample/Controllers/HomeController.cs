using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Polly;
using Polly.Caching;
using Polly.Registry;

namespace PollyCachingDepartmentsExample.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    public class HomeController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly CachePolicy<HttpResponseMessage> _cachePolicy;

        public HomeController(IPolicyRegistry<string> myRegistry, HttpClient httpClient)
        {
            _httpClient = httpClient;
            _cachePolicy = myRegistry.Get<CachePolicy<HttpResponseMessage>>("myPollyCache");
        }

        [HttpGet("")]
        public async Task<IActionResult> Get()
        {
            string requestEndpoint = $"departments";

            Context policyExecutionContext = new Context($"departments");

            HttpResponseMessage response = await _cachePolicy.ExecuteAsync(
                () => _httpClient.GetAsync(requestEndpoint), policyExecutionContext);

            if (response.IsSuccessStatusCode)
            {
                Dictionary<int, string> departments = JsonConvert.DeserializeObject<Dictionary<int, string>>(
                    await response.Content.ReadAsStringAsync());
                return Ok(departments);
            }

            return StatusCode((int)response.StatusCode, response.Content.ReadAsStringAsync());
        }

        [HttpGet("{departmentId}")]
        public async Task<IActionResult> Get(int departmentId)
        {
            string requestEndpoint = $"departments/{departmentId}";

            Context policyExecutionContext = new Context($"department-{departmentId}");

            HttpResponseMessage response = await _cachePolicy.ExecuteAsync(
                () => _httpClient.GetAsync(requestEndpoint), policyExecutionContext);

            if (response.IsSuccessStatusCode)
            {
                string departmentName = JsonConvert.DeserializeObject<string>(await response.Content.ReadAsStringAsync());
                return Ok(departmentName);
            }

            return StatusCode((int)response.StatusCode, response.Content.ReadAsStringAsync());
        }

      

    }
}
