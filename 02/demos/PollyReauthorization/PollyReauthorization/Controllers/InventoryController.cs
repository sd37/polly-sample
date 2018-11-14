using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;

namespace PollyExecutingDelegateOnRetry.Controllers
{
    [Produces("application/json")]
    [Route("api/Inventory")]
    public class InventoryController : Controller
    {
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            await Task.Delay(100); // simulate some data processing by delaying for 100 milliseconds 
      
            string authCode = Request.Cookies["Auth"];

            if (authCode == "GoodAuthCode")
            {
                return Ok(15);
            }
            else
            {
                return StatusCode((int)HttpStatusCode.Unauthorized, "Not authorized");
            }

        }
    }
}
