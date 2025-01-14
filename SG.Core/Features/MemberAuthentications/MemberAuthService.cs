using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SG.Core.Features.Operators;
using SG.Core.Features.User;
using SG.Core.Shared.Configurations;
using SG.Core.Shared.Constants;

namespace SG.Core.Features.MemberAuthentications
{
    public interface IMemberAuthService
    {
        Task<SingleSignOnResult> SignIn(MemberAuth memberAuth, int? gameTypeId);
    }

    public class SiteNames
    {
        public List<string> Sites { get; set; }
    }

    public class MemberAuthService : IMemberAuthService
    {
        private readonly IMemberAuthDataAccess memberAuthDataAccess;
        private readonly IOperatorDataService operatorDataService;
        private readonly ICurrencyGroupDataService currencyGroupDataService;
        private readonly IAppSettings appSettings;
        private readonly IUserDataService userDataService;

        public MemberAuthService(
            IMemberAuthDataAccess memberAuthDataAccess,
            IOperatorDataService operatorDataService,
            ICurrencyGroupDataService currencyGroupDataService,
            IUserDataService userDataService,
            IAppSettings appSettings)
        {
            this.memberAuthDataAccess = memberAuthDataAccess;
            this.operatorDataService = operatorDataService;
            this.currencyGroupDataService = currencyGroupDataService;
            this.userDataService = userDataService;
            this.appSettings = appSettings;
        }

        public async Task<SingleSignOnResult> SignIn(MemberAuth memberAuth, int? gameTypeId)
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
            var isGenerateNew = (latestSignIn == null || latestSignIn.Time.AddSeconds(appSettings.TokenExpiredSeconds) < DateTimeOffset.Now);

            if (isGenerateNew)
            {
                memberAuth.Token = Guid.NewGuid().ToString();
                memberAuth.Time = DateTimeOffset.Now;
                memberAuth.CurrencyCode = currency.Code;
                memberAuth.CurrencyGroupId = currency.GroupId;
                memberAuth.Multiplier = currency.Multiplier;
                memberAuth.IsTestCurrency = currency.IsTest;

                await memberAuthDataAccess.Create(memberAuth);
            }

            var (token, isTestCurrency, partnerId) = isGenerateNew ? memberAuth : latestSignIn;
            var @operator = await operatorDataService.GetById(memberAuth.OperatorId);
            var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"sitename-config.json");
            var siteNames = LoadSiteNamesFromFile(filePath);
            var siteNameOrAlias = GetSiteNameOrAlias(memberAuth.SiteName, siteNames, @operator);
            var currencyGroup = await currencyGroupDataService.GetById(currency.GroupId);

            var requiredURLParams = new RequiredURLParams
            {
                AppType = memberAuth.AppType,
                GameClientUrl = gameClientUrl.ToString().TrimEnd('/'),
                Token = token,
                Language = memberAuth.Language
            };

            var optionalURLParams = new OptionalURLParams
            {
                gameTypeId = gameTypeId,
                returnUrl = memberAuth.ReturnUrl,
                refValue = memberAuth.Ref,
                operatorAlias = siteNameOrAlias,
                partnerId = isTestCurrency ? partnerId : string.Empty,
                currencyGroup = currencyGroup.Name
            };

            return CreateSSOResult(
                ref requiredURLParams,
                ref optionalURLParams
               );
        }

        private string GetSiteNameOrAlias(string siteName, SiteNames siteNames, Operator @operator)
        {
            if (siteName.ToUpperInvariant() == "TIP88")
            {
                return siteName;
            }
            return siteNames.Sites.Any(site => site.ToUpperInvariant() == siteName.ToUpperInvariant()) ? "VG" : @operator.Alias;
        }

        private SiteNames LoadSiteNamesFromFile(string filePath)
        {
            var json = File.ReadAllText(filePath);
            return JsonConvert.DeserializeObject<SiteNames>(json);
        }

        private async Task<Uri> GetRedirectedUrl(MemberAuth memberAuth, bool isTestCurrency)
        {
            var partner = await operatorDataService.GetById(memberAuth.OperatorId);

            if (partner == null)
            {
                return null;
            }

            var user = await userDataService.GetByUsername(memberAuth.OperatorId, memberAuth.PlayerId);
            var isInternal = user?.IsInternal ?? false;
            if (isTestCurrency && isInternal && !string.IsNullOrWhiteSpace(partner.TestCurrencyUrl))
            {
                return new Uri(partner.TestCurrencyUrl);
            }

            var redirectedUrl = memberAuth.AppType switch
            {
                AppTypes.SabaClub => partner.ClubVersionUrl,
                AppTypes.MiniSabaClub => partner.MiniVersionUrl,
                AppTypes.SabaMobi => partner.MobiVersionUrl,
                AppTypes.MegaKeno => partner.MegaKenoUrl,
                AppTypes.MobileDirect => partner.MobileDirectUrl,
                _ => partner.ClubVersionUrl
            };

            return string.IsNullOrWhiteSpace(redirectedUrl) ? null : new Uri(redirectedUrl);
        }

        private static SingleSignOnResult CreateSSOResult(ref RequiredURLParams requiredURLParams, ref OptionalURLParams optionalURLParams)
        {
            string gameClientUrl = requiredURLParams.GameClientUrl;
            string token = requiredURLParams.Token;
            string language = requiredURLParams.Language;
            byte appType = requiredURLParams.AppType;

            var clientRedirectUrl = new StringBuilder();
            clientRedirectUrl.Append($"{gameClientUrl}?token={token}&lang={language}&appType={appType}");

            int? gameTypeId = optionalURLParams.gameTypeId;
            string partnerId = optionalURLParams.partnerId;
            string refValue = optionalURLParams.refValue;
            string returnUrl = optionalURLParams.returnUrl;
            string operatorAlias = optionalURLParams.operatorAlias;
            string currencyGroup = optionalURLParams.currencyGroup;

            if (gameTypeId.HasValue)
            {
                clientRedirectUrl.Append($"&gameId={gameTypeId}");
            }

            if (!string.IsNullOrWhiteSpace(partnerId))
            {
                clientRedirectUrl.Append($"&partnerId={partnerId}");
            }

            if (!string.IsNullOrWhiteSpace(refValue))
            {
                clientRedirectUrl.Append($"&ref={refValue}");
            }

            if (!string.IsNullOrWhiteSpace(returnUrl))
            {
                var returnUrlEncoded = Convert.ToBase64String(Encoding.UTF8.GetBytes(returnUrl));

                clientRedirectUrl.Append($"&rurl={returnUrlEncoded}");
            }

            if (!string.IsNullOrWhiteSpace(operatorAlias))
            {
                clientRedirectUrl.Append($"&op={operatorAlias}");
            }

            if (!string.IsNullOrWhiteSpace(currencyGroup))
            {
                clientRedirectUrl.Append($"&cg={currencyGroup}");
            }

            return SingleSignOnResult.CreateSuccessResponse(clientRedirectUrl.ToString(), token);
        }

        private struct RequiredURLParams
        {
            public string Token;
            public string Language;
            public string GameClientUrl;
            public byte AppType;
        }

        private struct OptionalURLParams
        {
            public int? gameTypeId;
            public string returnUrl;
            public string refValue;
            public string operatorAlias;
            public string partnerId;
            public string currencyGroup;
        }
    }
}