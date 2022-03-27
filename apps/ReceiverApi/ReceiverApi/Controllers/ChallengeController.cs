using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReceiverApi.Core.Services;

namespace ReceiverApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChallengeController : ControllerBase
    {
        [AllowAnonymous]
        [HttpPost("")]
        public async Task<IActionResult> GetChallenge([FromServices] ChallengeService challengeService)
        {
            var challenge = await challengeService.GenerateChallenge();
            return Ok(challenge);
        }
    }
}