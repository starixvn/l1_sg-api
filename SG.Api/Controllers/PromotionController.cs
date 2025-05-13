using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SG.Api.Authentication;
using SG.Api.Models;
using SG.Core.Features.Log;
using SG.Core.Features.Promotions;
using SG.Core.Shared.Models;

namespace SG.Api.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("[controller]")]
    public class PromotionController : ControllerBase
    {
        private readonly IOperatorRequestAuthentication operatorRequestAuthentication;
        private readonly IPromotionService promotionService;
        private readonly ILogService logService;

        public PromotionController(
            IOperatorRequestAuthentication operatorRequestAuthentication,
            IPromotionService promotionService,
            ILogService logService)
        {
            this.operatorRequestAuthentication = operatorRequestAuthentication;
            this.promotionService = promotionService;
            this.logService = logService;
        }

        [HttpPost]
        [Route("GetPromotion")]
        public async Task<IActionResult> GetPromotion([FromBody] PromotionRequest model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var requestTime = DateTimeOffset.UtcNow;

            if (await operatorRequestAuthentication.Verify(model.Auth.OperatorId, Request))
            {
                var promotionResult = await promotionService.GetPromotionAsync(
                    model.PromotionCode,
                    model.Auth.OperatorId,
                    model.Seq);
                
                var response = new SGResponse<PromotionResult>
                {
                    TimeStamp = DateTimeOffset.UtcNow,
                    Result = promotionResult
                };

                await WritePromotionLog(requestTime, model, response.Result);

                return Ok(response);
            }

            return Unauthorized();
        }

        private Task WritePromotionLog(DateTimeOffset requestTime, PromotionRequest request, PromotionResult result)
        {
            var log = new SBLog(request, result, requestTime, DateTimeOffset.UtcNow);
            return logService.LogPromotionAsync(log);
        }
    }
} 