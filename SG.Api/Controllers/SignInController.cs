using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SG.Api.Authentication;
using SG.Api.Models;
using SG.Core.Features.Log;
using SG.Core.Features.MemberAuthentications;
using SG.Core.Features.Operators;
using SG.Core.Shared.Configurations;
using SG.Core.Shared.Constants;

namespace SG.Api.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("[controller]")]
    public class SignInController : ControllerBase
    {
        private readonly IMemberAuthService memberAuthService;
        private readonly IOperatorDataService operatorDataService;
        private readonly ILogService logService;
        private readonly IOperatorRequestAuthentication operatorRequestAuthentication;
        private readonly ISystemSetting systemSetting;

        public SignInController(IMemberAuthService memberAuthService, IOperatorDataService operatorDataService, ILogService logService,
            IOperatorRequestAuthentication operatorRequestAuthentication, ISystemSetting systemSetting)
        {
            this.memberAuthService = memberAuthService;
            this.operatorDataService = operatorDataService;
            this.logService = logService;
            this.operatorRequestAuthentication = operatorRequestAuthentication;
            this.systemSetting = systemSetting;
        }

        [HttpPost]
        [Route("index")]
        public async Task<IActionResult> Index(SignIn model)
        {
            var requestTime = DateTimeOffset.UtcNow;

            if (await operatorDataService.VerifyWithCurrency(model.Auth?.OperatorId, model.Currency)
                && await operatorRequestAuthentication.Verify(model.Auth?.OperatorId, Request))
            {
                var ip = model.IP.Split(',').FirstOrDefault()?.Trim();

                var result = await memberAuthService.SignIn(new MemberAuth
                {
                    Domain = model.Domain,
                    BrowserUserAgent = model.BrowserUserAgent,
                    CurrencyName = model.Currency.ToUpperInvariant(),
                    IP = ip,
                    PlayerId = model.PlayerId,
                    PlayerName = model.PlayerName,
                    Language = model.Language,
                    SiteId = model.SiteId,
                    OperatorId = model.Auth.OperatorId,
                    AppType = AppTypes.IsValid(model.AppType) ? model.AppType : AppTypes.SabaClub,
                    SiteName = model.SiteName,
                    OperatorToken = model.OperatorToken,
                    IsCash = IsVG2System(systemSetting.SystemType) && model.IsCash,
                    DisableJackpotGames = !IsVG2System(systemSetting.SystemType) ||
                                            systemSetting.DisableJackpotOperators.Any(x => x == model.Auth.OperatorId) || 
                                            model.DisableJackpotGames,
                    Ref = model.Ref,
                    ReturnUrl = model.ReturnUrl
                }, model.GameTypeId);

                await WriteSignInLog(requestTime, model, result);

                return Ok(new Models.SignInResult { RedirectUrl = result.RedirectUrl });
            }

            await WriteSignInLog(requestTime, model, SingleSignOnResult.CreateErrorResponse(SingleSignOnResult.Unauthorize));

            return Unauthorized();
        }

        private static bool IsVG2System(string systemType)
        {
            return systemType == SystemType.VG2;
        }

        private Task WriteSignInLog(DateTimeOffset requestTime, SignIn model, SingleSignOnResult result)
        {
            var log = new SBLog(model, result, requestTime, DateTimeOffset.UtcNow);

            return logService.LogSignInAsync(log);
        }
    }
}