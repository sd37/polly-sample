using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace PollyCachingDepartmentsExample.Controllers
{
#pragma warning disable 1998
    [Produces("application/json")]
    [Route("api/[controller]")]
    public class DepartmentsController : Controller
    {
        private static readonly Dictionary<int, string> Departments;

        static DepartmentsController()
        {
            Departments = new Dictionary<int, string>
            {
                {1, "1-Arts"}, {2, "2-Books"}, {3, "3-Automotive"}, {4, "4-Beauty"},
                {5, "5-Cell phones"}, {6, "6-Computers"}, {7, "7-Electronics"}
            };
        }

        [HttpGet()]
        public async Task<IActionResult> Get()
        {
            return Ok(Departments);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            return Ok(Departments[id]);
        }
    }
}
