using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace ReceiverApi.Core.Services
{
    public class ProcessingService
    {
        private readonly ChallengeService _challengeService;
        
        public ProcessingService(ChallengeService challengeService)
        {
            _challengeService = challengeService;
        }

        public async Task ProcessInboundMessageAsync(HttpRequest req)
        {
            var token = req.Headers["Authorization"].ToString();
            var clientId = req.Query["clientId"];
            var signedMessage = req.Query["signedMessage"];
            if (string.IsNullOrWhiteSpace(token))
            {
                throw new Exception();
            }

            token = token.Split(" ").Last();

            if (!(await _challengeService.ValidateClientMessage(clientId, signedMessage, token)))
            {
                throw new Exception();
            }
        }
    }
}