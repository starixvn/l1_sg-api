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
using SG.Core.Shared.Utils;

namespace SG.Core.Features.MemberAuthentications
{
    public interface IMemberAuthService
    {
        Task<SingleSignOnResult> SignIn(MemberAuth memberAuth, int? gameTypeId);
        Task<SingleSignOnResult> UpdateMemberAuth(UpdateMemberAuth updateMemberAuthRequest);
    }

    public class SiteNames
    {
        public List<string> Sites { get; set; }
    }

    public class MemberAuthService : IMemberAuthService
    {
        private const string SABA_PLAY_OPERATOR_ID = "SabaPlay";
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

            if (@operator == null)
            {
                return SingleSignOnResult.CreateErrorResponse(SingleSignOnResult.GameServerNotFoundCode);
            }

            var isTesting = await IsTesting(currency.IsTest, memberAuth.OperatorId, memberAuth.PlayerId);
            var signInContext = await GetSignInContext(memberAuth, @operator, gameTypeId, isTesting);
            var gameClientUrl = signInContext.GameClientUrl;
            var redirectUrl = signInContext.RedirectUrl;
            var isOpenDirectly = signInContext.IsOpenDirect;

            if (signInContext.RedirectUrl == null)
            {
                return SingleSignOnResult.CreateErrorResponse(SingleSignOnResult.GameServerNotFoundCode);
            }

            var (token, isTestCurrency, partnerId) = await GetLatestMemberAuth(memberAuth, currency);

            var requiredURLParams = new RequiredURLParams
            {
                AppType = memberAuth.AppType,
                RedirectUrl = !isTesting ? redirectUrl.ToString() : redirectUrl.ToString().TrimEnd('/'),
                Token = token,
                Language = memberAuth.Language
            };

            var currencyGroup = await currencyGroupDataService.GetById(currency.GroupId);
            var siteNameOrAlias = GetSiteNameOrAlias(memberAuth.SiteName, @operator);
            var returnUrl = GetReturnUrl(gameClientUrl, memberAuth.Ref, memberAuth.ReturnUrl, isOpenDirectly);

            var optionalURLParams = new OptionalURLParams
            {
                gameTypeId = gameTypeId,
                returnUrl = returnUrl,
                refValue = memberAuth.Ref,
                operatorAlias = siteNameOrAlias,
                partnerId = isTestCurrency ? partnerId : string.Empty,
                currencyGroup = currencyGroup.Name,
                isStandalone = isOpenDirectly,
                lobbyUrl = isOpenDirectly ? gameClientUrl?.ToString()?.TrimEnd('/') : null
            };

            return CreateSSOResult(requiredURLParams, optionalURLParams);
        }

        public async Task<SingleSignOnResult> UpdateMemberAuth(UpdateMemberAuth updateMemberAuthRequest)
        {
            var memberAuth = await memberAuthDataAccess.GetByToken(updateMemberAuthRequest.Token);
            if (memberAuth == null)
            {
                return SingleSignOnResult.CreateErrorResponse(SingleSignOnResult.Unauthorize);
            }

            var @operator = await operatorDataService.GetById(updateMemberAuthRequest.OperatorId);
            if (@operator == null)
            {
                return SingleSignOnResult.CreateErrorResponse(SingleSignOnResult.GameServerNotFoundCode);
            }

            var gameId = updateMemberAuthRequest.GameId;
            var signInContext = await GetSignInContext(memberAuth, @operator, gameId, false);
            if (signInContext.RedirectUrl == null)
            {
                return SingleSignOnResult.CreateErrorResponse(SingleSignOnResult.GameServerNotFoundCode);
            }

            await memberAuthDataAccess.UpdateCurrencyGroup(updateMemberAuthRequest.Token, updateMemberAuthRequest.CurrencyGroupId);

            return SingleSignOnResult.CreateSuccessResponse(signInContext.RedirectUrl.ToString());
        }

        private async Task<SignInContext> GetSignInContext(MemberAuth memberAuth, Operator @operator, int? gameId, bool isTesting)
        {
            var appType = memberAuth.AppType;
            var gameConfiguration = await GetGameConfiguration(gameId);
            var isOpenDirectly = IsOpenGameDirectly(appType, gameConfiguration);
            var shouldUsePortraitOrientation = IsUsePortraitOrientation(memberAuth, gameConfiguration);
            var gameClientUrl = GetGameClientUrl(@operator, appType, shouldUsePortraitOrientation);
            var redirectUrl = GetRedirectedUrl(@operator, isTesting, isOpenDirectly, appType, shouldUsePortraitOrientation, gameId);
            return new SignInContext
            {
                IsOpenDirect = isOpenDirectly,
                GameClientUrl = gameClientUrl,
                RedirectUrl = redirectUrl
            };
        }

        private static bool IsOpenGameDirectly(byte appType, GameConfigurationResponse gameConfiguration)
        {
            return appType == AppTypes.SabaClub
                && gameConfiguration?.IsOpenDirectly == true 
                && gameConfiguration.IsMicrosite == true;
        }

        private static bool IsUsePortraitOrientation(MemberAuth memberAuth, GameConfigurationResponse gameConfiguration)
        {
            var deviceType = DeviceTypeHelper.GetDeviceType(memberAuth.BrowserUserAgent);
            return memberAuth.AppType == AppTypes.SabaClub
                && memberAuth.OperatorId == SABA_PLAY_OPERATOR_ID
                && gameConfiguration?.IsSupportedPortraitMode == true
                && deviceType.Equals(DeviceType.Mobile);

        }

        private async Task<MemberAuth> GetLatestMemberAuth(MemberAuth oldMemberAuth, Currency currency)
        {
            var latestSignIn = await memberAuthDataAccess.GetLatest(oldMemberAuth);
            var isGenerateNew = (latestSignIn == null || latestSignIn.Time.AddSeconds(appSettings.TokenExpiredSeconds) < DateTimeOffset.Now);

            if (isGenerateNew)
            {
                oldMemberAuth.Token = Guid.NewGuid().ToString();
                oldMemberAuth.Time = DateTimeOffset.Now;
                oldMemberAuth.CurrencyCode = currency.Code;
                oldMemberAuth.CurrencyGroupId = currency.GroupId;
                oldMemberAuth.Multiplier = currency.Multiplier;
                oldMemberAuth.IsTestCurrency = currency.IsTest;

                await memberAuthDataAccess.Create(oldMemberAuth);
            }

            return isGenerateNew ? oldMemberAuth : latestSignIn;
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

        private static Uri GetReturnUrl(Uri gameClientUrl, string refValue, string returnUrlValue, bool isOpenDirectly)
        {
            if (string.IsNullOrEmpty(refValue) && string.IsNullOrEmpty(returnUrlValue) && isOpenDirectly)
            {
                return gameClientUrl;
            }
            Uri uri;
            return !string.IsNullOrWhiteSpace(returnUrlValue) && Uri.TryCreate(returnUrlValue, UriKind.RelativeOrAbsolute, out uri)
            ? uri
            : null;
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

        private static Uri GetGameClientUrl(Operator @operator, byte appType, bool shouldUsePortraitOrientation)
        {
            var clientUrl = appType switch
            {
                AppTypes.SabaClub => shouldUsePortraitOrientation ? @operator.MobileDirectUrl : @operator.ClubVersionUrl,
                AppTypes.MiniSabaClub => @operator.MiniVersionUrl,
                AppTypes.SabaMobi => @operator.MobiVersionUrl,
                AppTypes.MegaKeno => @operator.MegaKenoUrl,
                AppTypes.DesktopLivePage => @operator.DesktopLiveUrl,
                AppTypes.MobileDirect => @operator.MobileDirectUrl,
                AppTypes.DesktopTodayPage => @operator.DesktopTodayUrl,
                AppTypes.LivestreamingPlatform => @operator.LivestreamingPlatformUrl,
                _ => @operator.ClubVersionUrl
            };
            return !string.IsNullOrEmpty(clientUrl) ? new Uri(clientUrl) : null;
        }

        private async Task<GameConfigurationResponse> GetGameConfiguration(int? gameId)
        {
            if (gameId > 0)
            {
                return await configurationApiService.GetDetailGameConfiguration(gameId.Value);
            }
            return null;
        }

        private Uri GetRedirectedUrl(Operator @operator, bool isTesting, bool isOpenDirectly, byte appType, bool shouldUsePortraitOrientation, int? gameId)
        {
            if (isTesting)
            {
                return new Uri(@operator.TestCurrencyUrl);
            }

            var gameClientUrl = GetGameClientUrl(@operator, appType, shouldUsePortraitOrientation);
            if (shouldUsePortraitOrientation)
            {
                return gameClientUrl;
            }
            if (isOpenDirectly && gameClientUrl != null)
            {
                var url = appSettings.MicrositeClientPatternDomain.Replace("{host}", GetRootDomain(gameClientUrl.ToString()));
                var uri = new Uri(url);
                return new Uri(uri, $"{uri.LocalPath.TrimEnd('/')}/site-{gameId}/");
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
            var returnUrl = optionalURLParams.returnUrl;
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

            if (returnUrl != null)
            {
                var returnUrlEncoded = Convert.ToBase64String(Encoding.UTF8.GetBytes(returnUrl.ToString()));

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

    class SignInContext
    {
        public bool IsOpenDirect { get; set; }
        public Uri GameClientUrl { get; set; }
        public Uri RedirectUrl { get; set; }
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
        public Uri returnUrl { get; set; }
        public string refValue { get; set; }
        public string operatorAlias { get; set; }
        public string partnerId { get; set; }
        public string currencyGroup { get; set; }
        public bool isStandalone { get; set; }
        public string lobbyUrl { get; set; }
    }
}