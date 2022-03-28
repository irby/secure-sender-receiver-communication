using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReceiverApi.Core.Services;

namespace ReceiverApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProcessController : ControllerBase
    {
        [AllowAnonymous]
        [HttpPost("")]
        public async Task<IActionResult> GetChallenge([FromServices] ProcessingService processingService)
        {
            await processingService.ProcessInboundMessageAsync(Request);
            return Ok();
        }
    }
}