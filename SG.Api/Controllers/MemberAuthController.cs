using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SG.Api.Authentication;
using SG.Api.Models;
using SG.Core.Features.MemberAuthentications;

namespace SG.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MemberAuthController : ControllerBase
    {
        private readonly IMemberAuthService memberAuthService;
        private readonly IOperatorRequestAuthentication operatorRequestAuthentication;

        public MemberAuthController(IMemberAuthService memberAuthService, IOperatorRequestAuthentication operatorRequestAuthentication)
        {
            this.memberAuthService = memberAuthService;
            this.operatorRequestAuthentication = operatorRequestAuthentication;
        }

        [HttpPost]
        [Route("[action]")]
        public async Task<ActionResult<SingleSignOnResult>> Update(UpdateMemberAuthRequest request)
        {
            if (await operatorRequestAuthentication.Verify(request.OperatorId, Request))
            {
                var result = await memberAuthService.UpdateMemberAuth(new UpdatememberAuth(request.Token, 
                    request.OperatorId, request.CurrencyGroupId, request.GameTypeId));

                return Ok(result);
            }
            return Ok(SingleSignOnResult.CreateErrorResponse(SingleSignOnResult.Unauthorize));
        }
    }
}