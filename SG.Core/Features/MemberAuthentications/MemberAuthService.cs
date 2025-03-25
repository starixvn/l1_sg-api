using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SG.Core.Features.GameConfigurations;
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
        private readonly IConfigurationApiService configurationApiService;

        public MemberAuthService(
            IMemberAuthDataAccess memberAuthDataAccess,
            IOperatorDataService operatorDataService,
            ICurrencyGroupDataService currencyGroupDataService,
            IUserDataService userDataService,
            IConfigurationApiService configApiService,
            IAppSettings appSettings)
        {
            this.memberAuthDataAccess = memberAuthDataAccess;
            this.operatorDataService = operatorDataService;
            this.currencyGroupDataService = currencyGroupDataService;
            this.userDataService = userDataService;
            this.configurationApiService = configApiService;
            this.appSettings = appSettings;
        }

        public async Task<SingleSignOnResult> SignIn(MemberAuth memberAuth, int? gameTypeId)
        {
            var currency = await operatorDataService.GetCurrency(memberAuth.OperatorId, memberAuth.CurrencyName);

            if (currency == null)
            {
                return SingleSignOnResult.CreateErrorResponse(SingleSignOnResult.Unauthorize);
            }

            var @operator = await operatorDataService.GetById(memberAuth.OperatorId);

            if(@operator == null)
            {
                return SingleSignOnResult.CreateErrorResponse(SingleSignOnResult.GameServerNotFoundCode);
            }

            var isTesting = await IsTesting(currency.IsTest, memberAuth.OperatorId, memberAuth.PlayerId);
            var gameConfiguration = await GetGameConfiguration(gameTypeId);
            var isOpenDirectly = memberAuth.AppType == AppTypes.SabaClub 
                && gameConfiguration?.IsOpenDirectly == true 
                && gameConfiguration?.IsMicrosite == true;
            var gameClientUrl = GetGameClientUrl(@operator, memberAuth.AppType);
            var redirectUrl = GetRedirectedUrl(@operator, isTesting, isOpenDirectly, memberAuth.AppType, gameTypeId);

            if (string.IsNullOrEmpty(redirectUrl))
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
            var siteNameOrAlias = GetSiteNameOrAlias(memberAuth.SiteName, @operator);
            var currencyGroup = await currencyGroupDataService.GetById(currency.GroupId);
            var returnUrl = GetReturnUrl(gameClientUrl, memberAuth.Ref, memberAuth.ReturnUrl, isOpenDirectly);

            var requiredURLParams = new RequiredURLParams
            {
                AppType = memberAuth.AppType,
                RedirectUrl = redirectUrl,
                Token = token,
                Language = memberAuth.Language
            };

            var optionalURLParams = new OptionalURLParams
            {
                gameTypeId = gameTypeId,
                returnUrl = returnUrl,
                refValue = memberAuth.Ref,
                operatorAlias = siteNameOrAlias,
                partnerId = isTestCurrency ? partnerId : string.Empty,
                currencyGroup = currencyGroup.Name,
                isStandalone = !isTesting && isOpenDirectly,
                lobbyUrl = !isTesting && isOpenDirectly ? gameClientUrl : null,
            };

            return CreateSSOResult(requiredURLParams, optionalURLParams);
        }

        private static string GetRootDomain(string domain)
        {
            var uri = new Uri(domain);
            var parts = uri.Host.Split('.');
            if (parts.Length <= 2)
            {
                return uri.Host;
            }
            return string.Join('.', parts.Skip(1));
        }

        private static string GetReturnUrl(string gameClientUrl, string refValue, string returnUrlValue, bool isOpenDirectly)
        {
            if (string.IsNullOrEmpty(refValue) && string.IsNullOrEmpty(returnUrlValue) && isOpenDirectly)
            {
                return gameClientUrl;
            }
            return returnUrlValue;
        }

        private static string GetSiteNameOrAlias(string siteName, Operator @operator)
        {
            var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"sitename-config.json");
            var siteNames = LoadSiteNamesFromFile(filePath);
            if (siteName.ToUpperInvariant() == "TIP88")
            {
                return siteName;
            }
            return siteNames.Sites.Exists(site => site.ToUpperInvariant() == siteName.ToUpperInvariant()) ? "VG" : @operator.Alias;
        }

        private static SiteNames LoadSiteNamesFromFile(string filePath)
        {
            var json = File.ReadAllText(filePath);
            return JsonConvert.DeserializeObject<SiteNames>(json);
        }

        private async Task<bool> IsTesting(bool isTestCurrency, string operatorId, string playerId)
        {
            if (isTestCurrency) 
            {
                var user = await userDataService.GetByUsername(operatorId, playerId);
                return user?.IsInternal ?? false;
            }
            return false;
        }

        private static string GetGameClientUrl(Operator @operator, byte appType)
        {
            var clientUrl = appType switch
            {
                AppTypes.SabaClub => @operator.ClubVersionUrl,
                AppTypes.MiniSabaClub => @operator.MiniVersionUrl,
                AppTypes.SabaMobi => @operator.MobiVersionUrl,
                AppTypes.MegaKeno => @operator.MegaKenoUrl,
                AppTypes.MobileDirect => @operator.MobileDirectUrl,
                _ => @operator.ClubVersionUrl
            };
            return clientUrl?.Trim()?.TrimEnd('/');
        }

        private async Task<GameConfigurationResponse> GetGameConfiguration(int? gameId)
        {
            if(gameId > 0)
            {
                return await configurationApiService.GetDetailGameConfiguration(gameId.Value);
            }
            return null;
        }

        private string GetRedirectedUrl(Operator @operator, bool isTesting, bool isOpenDirectly, byte appType, int? gameId)
        {
            if (isTesting) {
                return @operator.TestCurrencyUrl.Trim().TrimEnd('/');
            }

            var gameClientUrl = GetGameClientUrl(@operator, appType);
            if (isOpenDirectly && !string.IsNullOrEmpty(gameClientUrl))
            {
                var url = appSettings.MicrositeClientPatternDomain.Replace("{host}", GetRootDomain(gameClientUrl));
                var uri = new Uri(url);
                return new Uri(uri, $"{uri.LocalPath.TrimEnd('/')}/site-{gameId}/").ToString();
            }

            return gameClientUrl;
        }

        private static SingleSignOnResult CreateSSOResult(RequiredURLParams requiredURLParams, OptionalURLParams optionalURLParams)
        {
            string redirectUrl = requiredURLParams.RedirectUrl;
            string token = requiredURLParams.Token;
            string language = requiredURLParams.Language;
            byte appType = requiredURLParams.AppType;

            var clientRedirectUrl = new StringBuilder();
            clientRedirectUrl.Append($"{redirectUrl}?token={token}&lang={language}&appType={appType}");

            int? gameTypeId = optionalURLParams.gameTypeId;
            string partnerId = optionalURLParams.partnerId;
            string refValue = optionalURLParams.refValue;
            string returnUrl = optionalURLParams.returnUrl;
            string operatorAlias = optionalURLParams.operatorAlias;
            string currencyGroup = optionalURLParams.currencyGroup;
            var lobbyUrl = optionalURLParams.lobbyUrl;

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

            if (optionalURLParams.isStandalone)
            {
                clientRedirectUrl.Append($"&isStandalone=true");
            }

            if (!string.IsNullOrEmpty(lobbyUrl))
            {
                var lobbyUrlEncoded = Convert.ToBase64String(Encoding.UTF8.GetBytes(lobbyUrl));
                clientRedirectUrl.Append($"&lurl={lobbyUrlEncoded}");

            }

            return SingleSignOnResult.CreateSuccessResponse(clientRedirectUrl.ToString(), token);
        }
    }
    class RequiredURLParams
    {
        public string Token { get; set; }
        public string Language { get; set; }
        public string RedirectUrl { get; set; }
        public byte AppType { get; set; }
    }

    class OptionalURLParams
    {
        public int? gameTypeId { get; set; }
        public string returnUrl { get; set; }
        public string refValue { get; set; }
        public string operatorAlias { get; set; }
        public string partnerId { get; set; }
        public string currencyGroup { get; set; }
        public bool isStandalone { get; set; }
        public string lobbyUrl { get; set; }
    }
}