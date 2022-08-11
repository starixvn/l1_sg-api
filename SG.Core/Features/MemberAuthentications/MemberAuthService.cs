using System;
using System.Text;
using System.Threading.Tasks;
using SG.Core.Features.Operators;
using SG.Core.Shared.Configurations;
using SG.Core.Shared.Constants;

namespace SG.Core.Features.MemberAuthentications
{
    public interface IMemberAuthService
    {
        Task<SingleSignOnResult> SignIn(MemberAuth memberAuth, byte? gameTypeId);
    }

    public class MemberAuthService : IMemberAuthService
    {
        private readonly IMemberAuthDataAccess memberAuthDataAccess;
        private readonly IOperatorDataService operatorDataService;
        private readonly IAppSettings appSettings;

        public MemberAuthService(
            IMemberAuthDataAccess memberAuthDataAccess,
            IOperatorDataService operatorDataService,
            IAppSettings appSettings)
        {
            this.memberAuthDataAccess = memberAuthDataAccess;
            this.operatorDataService = operatorDataService;
            this.appSettings = appSettings;
        }

        public async Task<SingleSignOnResult> SignIn(MemberAuth memberAuth, byte? gameTypeId)
        {
            var currency = await operatorDataService.GetCurrency(memberAuth.OperatorId, memberAuth.CurrencyName);

            if (currency == null)
            {
                return SingleSignOnResult.CreateErrorResponse(SingleSignOnResult.Unauthorize);
            }

            var gameClientUrl = await GetRedirectedUrl(memberAuth, currency.IsTest);

            if (gameClientUrl == null)
            {
                return SingleSignOnResult.CreateErrorResponse(SingleSignOnResult.GameServerNotFoundCode);
            }

            var latestSignIn = await memberAuthDataAccess.GetLatest(memberAuth);
            var isGenerateNew = (latestSignIn == null || latestSignIn.Time.AddSeconds(appSettings.TokenExpiredSeconds) < DateTime.Now);

            if (isGenerateNew)
            {
                memberAuth.Token = Guid.NewGuid().ToString();
                memberAuth.Time = DateTime.Now;
                memberAuth.CurrencyCode = currency.Code;
                memberAuth.CurrencyGroupId = currency.GroupId;
                memberAuth.Multiplier = currency.Multiplier;
                memberAuth.IsTestCurrency = currency.IsTest;

                await memberAuthDataAccess.Create(memberAuth);
            }

            var (token, isTestCurrency, partnerId) = isGenerateNew ? memberAuth : latestSignIn;

            return CreateSSOResult(
                gameTypeId,
                token,
                memberAuth.Language,
                gameClientUrl.ToString().TrimEnd('/'),
                isTestCurrency ? partnerId : string.Empty);
        }

        private async Task<Uri> GetRedirectedUrl(MemberAuth memberAuth, bool isTestCurrency)
        {
            var partner = await operatorDataService.GetById(memberAuth.OperatorId);

            if (partner == null)
            {
                return null;
            }

            var redirectedUrl = isTestCurrency
                ? partner.TestCurrencyUrl
                : memberAuth.AppType switch
                {
                    AppTypes.SabaClub => partner.ClubVersionUrl,
                    AppTypes.MiniSabaClub => partner.MiniVersionUrl,
                    AppTypes.SabaMobi => partner.MobiVersionUrl,
                    _ => partner.ClubVersionUrl,
                };

            return string.IsNullOrWhiteSpace(redirectedUrl) ? null : new Uri(redirectedUrl);
        }

        private static SingleSignOnResult CreateSSOResult(byte? gameTypeId, string token, string language, string gameClientUrl, string partnerId = null)
        {
            var clientRedirectUrl = new StringBuilder();
            clientRedirectUrl.Append($"{gameClientUrl}?token={token}&lang={language}");

            if (gameTypeId.HasValue)
            {
                clientRedirectUrl.Append($"&gameId={gameTypeId}");
            }

            if (!string.IsNullOrWhiteSpace(partnerId))
            {
                clientRedirectUrl.Append($"&partnerId={partnerId}");
            }

            return SingleSignOnResult.CreateSuccessResponse(clientRedirectUrl.ToString());
        }
    }
}