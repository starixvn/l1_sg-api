using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Sentry;
using SG.Api.Authentication;
using SG.Api.Models;
using SG.Core.Features.ApiCommunicators;
using SG.Core.Features.DailySummaries;
using SG.Core.Features.Operators;
using SG.Core.Shared.Configurations;
using SG.Core.Shared.Models;
using SG.Core.Shared.Utils;

namespace SG.Api.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("[controller]")]
    public class IntegrityController : ControllerBase
    {
        private readonly IOperatorRequestAuthentication operatorRequestAuthentication;
        private readonly IDailySummaryService dailySummaryService;
        private readonly ISGApi sgApi;
        private readonly IAppSettings appSettings;
        private readonly IOperatorDataService operatorDataService;
        private readonly ISentryClient sentryClient;

        public IntegrityController(
            IOperatorRequestAuthentication operatorRequestAuthentication, 
            IDailySummaryService dailySummaryService, 
            ISGApi sgApi, 
            IAppSettings appSettings, 
            IOperatorDataService operatorDataService,
            ISentryClient sentryClient)
        {
            this.operatorRequestAuthentication = operatorRequestAuthentication;
            this.dailySummaryService = dailySummaryService;
            this.sgApi = sgApi;
            this.appSettings = appSettings;
            this.operatorDataService = operatorDataService;
            this.sentryClient = sentryClient;
        }

        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> DailySummarize([FromBody] DailySummaryRequest model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            if (await operatorRequestAuthentication.Verify(model.Auth.OperatorId, Request))
            {
                if(this.appSettings.DailySummaryApiSettings.EnableCloud && !this.appSettings.DailySummaryApiSettings.IsCloud)
                {
                    return await GetCloudData(model);
                }

                return await GetInternalData(model);
            }

            return Unauthorized();
        }

        private async Task<IActionResult> GetCloudData(DailySummaryRequest model)
        {
            try
            {
                var @operator = await operatorDataService.GetById(model.Auth.OperatorId);

                if (@operator != null)
                {
                    var accessKey = HMACUtils.GenerateAccessKey(@operator.SecretKey, model);
                    var result = await sgApi.DailySummarize(accessKey, model);

                    return Ok(result);
                }
            }
            catch(Exception ex)
            {
                this.sentryClient.CaptureException(ex);
            }

            return await GetInternalData(model);
        }



        private async Task<IActionResult> GetInternalData(DailySummaryRequest model)
        {
            var result = await dailySummaryService.Get(model.SummarizedDate, model.Auth.OperatorId);
            result.Seq = model.Seq;

            var response = new SGResponse<DailySummaryResult>
            {
                TimeStamp = DateTimeOffset.UtcNow,
                Result = result
            };

            return Ok(response);
        }
    }
}