using Microsoft.AspNetCore.Mvc;
using SG.Api.Authentication;
using SG.Core.Features.EndRoundInfo;
using SG.Core.Features.Log;
using SG.Core.Features.Operators;
using SG.Core.Features.ResultApiCommunicators;
using System;
using System.Threading.Tasks;

namespace SG.Api.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("[controller]")]
    public class PlayerController : ControllerBase
    {
        private readonly IOperatorRequestAuthentication operatorRequestAuthentication;
        private readonly IResultApiService resultApiService;
        private readonly IOperatorDataService operatorDataService;
        private readonly ILogService logService;

        public PlayerController(
            IOperatorRequestAuthentication operatorRequestAuthentication,
            IResultApiService resultApiService,
            IOperatorDataService operatorDataService,
            ILogService logService)
        {
            this.operatorRequestAuthentication = operatorRequestAuthentication;
            this.resultApiService = resultApiService;
            this.operatorDataService = operatorDataService;
            this.logService = logService;
        }

        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> RetrieveEndRoundInfo([FromBody] Models.EndRoundInfoRequest model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            if (await operatorRequestAuthentication.Verify(model.Auth.OperatorId, Request))
            {
                var @operator = await operatorDataService.GetById(model.Auth.OperatorId);

                var requestTime = DateTimeOffset.UtcNow;
                var response = await resultApiService.RetrieveEndRoundInfoAsync(
                    new EndRoundInfoRequest(
                        model.GameRoundId,
                        model.GameTypeId,
                        model.PlayerId,
                        model.SiteId,
                        model.Auth.OperatorId,
                        @operator.SecretKey,
                        model.FreeBet));

                response.Result.Seq = model.Seq;

                await WriteRetrieveEndRoundLog(requestTime, model, response.Result);

                return Ok(response);
            }

            return Unauthorized();
        }

        private Task WriteRetrieveEndRoundLog(DateTimeOffset requestTime, Models.EndRoundInfoRequest request, EndRoundInfoResult result)
        {
            var log = new SBLog(request, result, requestTime, DateTimeOffset.UtcNow);

            return logService.LogRetrieveEndRoundAsync(log);
        }
    }
}