using Microsoft.AspNetCore.Mvc;
using Sentry;
using SG.Api.Authentication;
using SG.Api.Extensions;
using SG.Core.Features.EndRoundInfo;
using SG.Core.Features.Log;
using SG.Core.Features.Operators;
using SG.Core.Features.ResultApiCommunicators;
using SG.Core.Shared.Configurations;
using SG.Core.Shared.Constants;
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
        private readonly ISystemSetting sytemSetting;

        public PlayerController(
            IOperatorRequestAuthentication operatorRequestAuthentication,
            IResultApiService resultApiService,
            IOperatorDataService operatorDataService,
            ILogService logService,
            ISystemSetting sytemSetting)
        {
            this.operatorRequestAuthentication = operatorRequestAuthentication;
            this.resultApiService = resultApiService;
            this.operatorDataService = operatorDataService;
            this.logService = logService;
            this.sytemSetting = sytemSetting;
        }

        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> RetrieveEndRoundInfo([FromBody] Models.EndRoundInfoRequest model)
        {
            SentrySdk.CaptureMessage(string.Format("RetrieveEndRoundInfo {0} {1}", Newtonsoft.Json.JsonConvert.SerializeObject(model), ModelState.IsValid), Sentry.Protocol.SentryLevel.Error);
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            if (await operatorRequestAuthentication.Verify(model.Auth.OperatorId, Request))
            {
                var @operator = await operatorDataService.GetById(model.Auth.OperatorId);

                var requestTime = DateTimeOffset.UtcNow;
                bool? freeBet = sytemSetting.SystemType == SystemType.VG2 ? model.FreeBet : null;
                var response = await resultApiService.RetrieveEndRoundInfoAsync(
                    new EndRoundInfoRequest(
                        model.GameRoundId,
                        model.GameTypeId,
                        StringFormat.FormatPlayerId(model.Auth.OperatorId, model.SiteId,model.PlayerId),
                        model.Auth.OperatorId,
                        @operator.SecretKey,
                        model.SiteId,
                        freeBet));

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