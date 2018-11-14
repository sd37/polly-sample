using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace PollyWrappingExampleMixingGenericsAndNon.Controllers
{
    [Produces("application/json")]
    [Route("api/Inventory")]
    public class InventoryController : Controller
    {
        static int _requestCount = 0;

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            _requestCount++;

            if (_requestCount % 6 != 0)
            {
                //return StatusCode((int)HttpStatusCode.InternalServerError, "Something went wrong");

                await Task.Delay(10000); // simulate some data processing by delaying for 10 seconds
            }

            return Ok(15);
        }
    }
}
