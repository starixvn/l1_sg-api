using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SG.Api.Authentication;
using SG.Api.Models;
using SG.Core.Features.DailySummaries;
using SG.Core.Shared.Models;

namespace SG.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PlayerController : ControllerBase
    {
        private readonly IOperatorRequestAuthentication operatorRequestAuthentication;
        private readonly IDailySummaryService dailySummaryService;

        public PlayerController(IOperatorRequestAuthentication operatorRequestAuthentication, IDailySummaryService dailySummaryService)
        {
            this.operatorRequestAuthentication = operatorRequestAuthentication;
            this.dailySummaryService = dailySummaryService;
        }

        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> RetrieveEndRoundInfo([FromBody] DailySummaryRequest model)
        {
            if (ModelState.IsValid)
            {
                if (await operatorRequestAuthentication.Verify(model.Auth.OperatorId, Request))
                {
                    var result = await dailySummaryService.Get(model.SummarizedDate, model.Auth.OperatorId);
                    result.Seq = model.Seq;

                    return Ok(new SGResponse<DailySummaryResult>
                    {
                        TimeStamp = DateTimeOffset.UtcNow,
                        Result = result
                    });
                }
                return Unauthorized();
            }

            return BadRequest();
        }
    }
}