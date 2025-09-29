using System;
using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.Xunit2;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using SG.Core.Features.GameConfigurations;
using SG.Core.Features.MemberAuthentications;
using SG.Core.Features.Operators;
using SG.Core.Features.User;
using SG.Core.Shared.Configurations;
using SG.Core.Shared.Constants;
using Xunit;

namespace SG.Core.Tests.Features.MemberAuthentications
{
    public class MemberAuthServiceTests
    {
        private readonly IMemberAuthDataAccess memberAuthDataAccess;
        private readonly IOperatorDataService operatorDataService;
        private readonly ICurrencyGroupDataService currencyGroupDataService;
        private readonly IUserDataService userDataService;
        private readonly IConfigurationApiService configurationApiService;
        private readonly IAppSettings appSettings;
        private readonly Fixture fixture = new Fixture();

        private readonly MemberAuthService memberAuthService;

        public MemberAuthServiceTests()
        {
            memberAuthDataAccess = Substitute.For<IMemberAuthDataAccess>();
            operatorDataService = Substitute.For<IOperatorDataService>();
            currencyGroupDataService = Substitute.For<ICurrencyGroupDataService>();
            appSettings = Substitute.For<IAppSettings>();
            appSettings.MicrositeClientPatternDomain = "http://proxy.{host}";
            userDataService = Substitute.For<IUserDataService>();
            configurationApiService = Substitute.For<IConfigurationApiService>();

            memberAuthService = new MemberAuthService(memberAuthDataAccess, operatorDataService, currencyGroupDataService, userDataService, configurationApiService, appSettings);
        }

        [Theory, AutoData]
        public async Task SignIn_NotFoundClientUrl_ReturnsGameServerNotFoundCode(MemberAuth memberAuth)
        {
            // Arrange
            operatorDataService.GetCurrency(memberAuth.OperatorId, memberAuth.CurrencyName).Returns(new Currency());

            // Act
            var result = await memberAuthService.SignIn(memberAuth, null);

            // Assert
            Assert.IsType<SingleSignOnResult>(result);
            Assert.Equal(SingleSignOnResult.GameServerNotFoundCode, result.Code);
        }

        [Theory]
        [InlineData(AppTypes.MiniSabaClub)]
        [InlineData(AppTypes.SabaMobi)]
        [InlineData(100)]
        public async Task SignIn_Success_ReturnsSingleSignOnResult(byte appType)
        {
            // Arrange
            var memberAuth = fixture.Create<MemberAuth>();
            memberAuth.AppType = appType;

            operatorDataService.GetById(memberAuth.OperatorId).Returns(new Operator
            {
                ClubVersionUrl = "http://local.com/",
                MobiVersionUrl = "http://mobi.com/",
                MiniVersionUrl = "http://mini.com/"
            });

            // Act
            var result = await memberAuthService.SignIn(memberAuth, null);

            // Assert
            Assert.IsType<SingleSignOnResult>(result);
        }

        [Theory, AutoData]
        public async Task SignIn_WithGameId_ReturnsRedirectUrlContainsGameId(MemberAuth memberAuth)
        {
            // Arrange
            byte? gameTypeId = 1;
            operatorDataService.GetById(memberAuth.OperatorId).Returns(new Operator
            {
                ClubVersionUrl = "http://local.com/",
                MobiVersionUrl = "http://mobi.com/",
                MiniVersionUrl = "http://mini.com/"
            });
            operatorDataService.GetCurrency(memberAuth.OperatorId, memberAuth.CurrencyName).Returns(new Currency());
            currencyGroupDataService.GetById(Arg.Any<string>()).Returns(new CurrencyGroup() { Name = "Test" });
            configurationApiService.GetDetailGameConfiguration(1).Returns(new GameConfigurationResponse());


            // Act
            var result = await memberAuthService.SignIn(memberAuth, gameTypeId);

            // Assert
            Assert.IsType<SingleSignOnResult>(result);
            Assert.Contains($"gameId={gameTypeId.Value}", result.RedirectUrl);
        }

        [Theory, AutoData]
        public async Task SignIn_TestCurrency_ReturnsRedirectUrlContainsPartnerId(MemberAuth memberAuth)
        {
            // Arrange
            operatorDataService.GetById(memberAuth.OperatorId).Returns(new Operator
            {
                ClubVersionUrl = "http://local.com/",
                MobiVersionUrl = "http://mobi.com/",
                MiniVersionUrl = "http://mini.com/",
                TestCurrencyUrl = "http://test.com/"
            });
            operatorDataService.GetCurrency(memberAuth.OperatorId, memberAuth.CurrencyName).Returns(new Currency { IsTest = true });
            currencyGroupDataService.GetById(Arg.Any<string>()).Returns(new CurrencyGroup() { Name = "Test" });

            // Act
            var result = await memberAuthService.SignIn(memberAuth, null);

            // Assert
            Assert.IsType<SingleSignOnResult>(result);
            Assert.Contains($"partnerId={memberAuth.OperatorId}", result.RedirectUrl);
        }

        [Theory, AutoData]
        public async Task SignIn_ExistingToken_ShouldBeReUse(MemberAuth memberAuth)
        {
            // Arrange
            var token = "token";
            operatorDataService.GetById(memberAuth.OperatorId).Returns(new Operator
            {
                ClubVersionUrl = "http://local.com/",
                MobiVersionUrl = "http://mobi.com/",
                MiniVersionUrl = "http://mini.com/"
            });
            memberAuthDataAccess.GetLatest(memberAuth).Returns(new MemberAuth
            {
                Token = token,
                Time = DateTime.Now
            });
            operatorDataService.GetCurrency(memberAuth.OperatorId, memberAuth.CurrencyName).Returns(new Currency());
            appSettings.TokenExpiredSeconds.Returns(60);
            currencyGroupDataService.GetById(Arg.Any<string>()).Returns(new CurrencyGroup() { Name = "Test" });

            // Act
            var result = await memberAuthService.SignIn(memberAuth, null);

            // Assert
            Assert.IsType<SingleSignOnResult>(result);
            Assert.Contains($"token={token}", result.RedirectUrl);
        }

        [Theory, AutoData]
        public async Task SignIn_ExistingToken_ShouldContainsPartnerIdIfTestCurrency(MemberAuth memberAuth)
        {
            // Arrange
            var token = "token";
            var partnerId = "partnerId";
            operatorDataService.GetById(memberAuth.OperatorId).Returns(new Operator
            {
                ClubVersionUrl = "http://local.com/",
                MobiVersionUrl = "http://mobi.com/",
                MiniVersionUrl = "http://mini.com/",
                TestCurrencyUrl = "http://test.com/"
            });

            memberAuthDataAccess.GetLatest(memberAuth).Returns(new MemberAuth
            {
                Token = token,
                Time = DateTime.Now.AddSeconds(-30),
                OperatorId = partnerId,
                IsTestCurrency = true
            });
            operatorDataService.GetCurrency(memberAuth.OperatorId, memberAuth.CurrencyName).Returns(new Currency { IsTest = true });
            appSettings.TokenExpiredSeconds.Returns(60);
            currencyGroupDataService.GetById(Arg.Any<string>()).Returns(new CurrencyGroup() { Name = "Test" });

            // Act
            var result = await memberAuthService.SignIn(memberAuth, null);

            // Assert
            Assert.IsType<SingleSignOnResult>(result);
            Assert.Contains($"token={token}", result.RedirectUrl);
            Assert.Contains($"partnerId={partnerId}", result.RedirectUrl);
        }

        [Theory, AutoData]
        public async Task SignIn_ExpiredToken_ShouldGenerateNew(MemberAuth memberAuth)
        {
            // Arrange
            var token = "token";
            var partnerId = "partnerId";
            operatorDataService.GetById(memberAuth.OperatorId).Returns(new Operator
            {
                ClubVersionUrl = "http://local.com/",
                MobiVersionUrl = "http://mobi.com/",
                MiniVersionUrl = "http://mini.com/",
                TestCurrencyUrl = "http://test.com/"
            });

            memberAuthDataAccess.GetLatest(memberAuth).Returns(new MemberAuth
            {
                Token = token,
                Time = DateTime.Now.AddSeconds(-70),
                OperatorId = partnerId,
                IsTestCurrency = true
            });
            operatorDataService.GetCurrency(memberAuth.OperatorId, memberAuth.CurrencyName).Returns(new Currency { IsTest = true });
            appSettings.TokenExpiredSeconds.Returns(60);
            currencyGroupDataService.GetById(Arg.Any<string>()).Returns(new CurrencyGroup() { Name = "Test" });

            // Act
            var result = await memberAuthService.SignIn(memberAuth, null);

            // Assert
            Assert.IsType<SingleSignOnResult>(result);
            Assert.Contains($"token={memberAuth.Token}", result.RedirectUrl);
            Assert.Contains($"partnerId={memberAuth.OperatorId}", result.RedirectUrl);
        }

        [Fact]
        public async Task SignIn_ExistingRefAndReturnUrl_ShouldCreateSSOResult()
        {
            // Arrange
            var memberAuth = fixture.Create<MemberAuth>();
            memberAuth.Ref = "d_direct";
            memberAuth.ReturnUrl = "http://test.com/member";

            operatorDataService.GetById(memberAuth.OperatorId).Returns(new Operator
            {
                ClubVersionUrl = "http://local.com/",
                MobiVersionUrl = "http://mobi.com/",
                MiniVersionUrl = "http://mini.com/",
                TestCurrencyUrl = "http://test.com/"
            });
            operatorDataService.GetCurrency(memberAuth.OperatorId, memberAuth.CurrencyName).Returns(new Currency { IsTest = true });
            appSettings.TokenExpiredSeconds.Returns(60);
            currencyGroupDataService.GetById(Arg.Any<string>()).Returns(new CurrencyGroup() { Name = "Test" });

            var rurlEncoded = "aHR0cDovL3Rlc3QuY29tL21lbWJlcg==";

            // Act
            var result = await memberAuthService.SignIn(memberAuth, null);

            // Assert
            Assert.IsType<SingleSignOnResult>(result);
            Assert.Contains($"ref={memberAuth.Ref}", result.RedirectUrl);
            Assert.Contains($"rurl={rurlEncoded}", result.RedirectUrl);
        }

        [Theory, AutoData]
        public async Task UpdateMemberAuth_NotExistingToken_ReturnUnauthorize(UpdateMemberAuth request)
        {
            // Arrange
            memberAuthDataAccess.GetByToken(request.Token).ReturnsNull();

            // Act
            var actual = await memberAuthService.UpdateMemberAuth(request);

            // Assert
            Assert.Equal(SingleSignOnResult.Unauthorize, actual.Code);
        }

        [Theory, AutoData]
        public async Task UpdateMemberAuth_NotExistingOperator_ReturnGameServerNotFound(UpdateMemberAuth request)
        {
            // Arrange
            var memberAuth = fixture.Create<MemberAuth>();
            memberAuthDataAccess.GetByToken(Arg.Any<string>()).Returns(memberAuth);
            operatorDataService.GetById(request.OperatorId).ReturnsNull();

            // Act
            var actual = await memberAuthService.UpdateMemberAuth(request);

            // Assert
            Assert.Equal(SingleSignOnResult.GameServerNotFoundCode, actual.Code);
        }

        [Theory, AutoData]
        public async Task UpdateMemberAuth_NotRedirectUrl_ReturnGameServerNotFound(UpdateMemberAuth request)
        {
            // Arrange
            request.GameId = 2;
            var memberAuth = fixture.Create<MemberAuth>();
            memberAuth.AppType = 2;
            memberAuthDataAccess.GetByToken(Arg.Any<string>()).Returns(memberAuth);
            var @operator = fixture.Create<Operator>();
            @operator.MobiVersionUrl = "";
            operatorDataService.GetById(request.OperatorId).Returns(@operator);
            configurationApiService.GetDetailGameConfiguration(request.GameId.Value).ReturnsNull();

            // Act
            var actual = await memberAuthService.UpdateMemberAuth(request);

            // Assert
            Assert.Equal(SingleSignOnResult.GameServerNotFoundCode, actual.Code);
        }

        [Theory]
        [InlineData("Operator1", "", false, false, 1, "http://local.com")]
        [InlineData("Operator1", "", false, false, 2, "http://mobi.com")]
        [InlineData("Operator1", "", false, false, 3, "http://mini.com")]
        [InlineData("Operator1", "", false, false, 7, "http://direct.com")]
        [InlineData("Operator1", "", true, false, 1, "http://proxy.local.com/site-")]
        [InlineData("Operator1", "", true, true, 1, "http://proxy.local.com/site-")]
        [InlineData("SabaPlay", "", false, true, 1, "http://local.com")]
        [InlineData("SabaPlay", "iphone", true, true, 1, "http://direct.com")]
        public async Task UpdateMemberAuth_ValidData_ReturnExpectedUrl(
            string operatorId,
            string userAgent,
            bool isOpenDirectly,
            bool isSupportPortraitMode,
            byte appType,
            string expectedUrl)
        {
            // Arrange
            var request = fixture.Create<UpdateMemberAuth>();
            request.OperatorId = operatorId;
            request.GameId = 2;
            var memberAuth = fixture.Create<MemberAuth>();
            memberAuth.AppType = appType;
            memberAuth.BrowserUserAgent = userAgent;
            memberAuth.OperatorId = operatorId;
            memberAuthDataAccess.GetByToken(Arg.Any<string>()).Returns(memberAuth);
            operatorDataService.GetById(request.OperatorId).Returns(new Operator
            {
                ClubVersionUrl = "http://local.com/",
                MobiVersionUrl = "http://mobi.com/",
                MiniVersionUrl = "http://mini.com/",
                MobileDirectUrl = "http://direct.com/"
            });
            configurationApiService.GetDetailGameConfiguration(request.GameId.Value).Returns(new GameConfigurationResponse
            {
                IsOpenDirectly = isOpenDirectly,
                IsSupportedPortraitMode = isSupportPortraitMode,
                IsMicrosite = true,
            });

            // Act
            var actual = await memberAuthService.UpdateMemberAuth(request);

            // Assert
            Assert.Contains(expectedUrl, actual.RedirectUrl);
        }

        [Fact]
        public async Task BuildRequiredUrlParams_OriginalGamesApptype_WithUnknownDevice_ReturnUrlHasGamePath()
        {
            // Arrange
            var memberAuth = new MemberAuth
            {
                AppType = AppTypes.OriginalGames,
                Language = "EN",
                Token = "test-token"
            };
            var redirectUrl = new Uri("http://example.com/");
            var token = "test-token";
            var gameTypeId = 5;
            var isTesting = false;

            // Act - Using reflection to test the private async method
            var method = typeof(MemberAuthService).GetMethod("BuildRequiredUrlParams",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var task = method.Invoke(memberAuthService, new object[] { memberAuth, redirectUrl, token, gameTypeId, isTesting });

            // Get the result using reflection to avoid RequiredURLParams access issue
            var resultTask = (Task)task;
            await resultTask;
            var resultProperty = resultTask.GetType().GetProperty("Result");
            var resultObject = resultProperty.GetValue(resultTask);
            var redirectUrlProperty = resultObject.GetType().GetProperty("RedirectUrl");
            var redirectUrlValue = (string)redirectUrlProperty.GetValue(resultObject);

            // Assert
            Assert.Contains("game-5/", redirectUrlValue);
        }

        [Fact]
        public async Task BuildRequiredUrlParams_OriginalGamesApptype_WithMobileDevice_ReturnUrlHasMobileGamePath()
        {
            // Arrange
            var memberAuth = new MemberAuth
            {
                AppType = AppTypes.OriginalGames,
                Language = "EN",
                Token = "test-token",
                BrowserUserAgent = "Mozilla/5.0 (iPhone; CPU iPhone OS 14_0 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Mobile/15E148"
            };
            var redirectUrl = new Uri("http://example.com/");
            var token = "test-token";
            var gameTypeId = 5;
            var isTesting = false;

            // Act - Using reflection to test the private async method
            var method = typeof(MemberAuthService).GetMethod("BuildRequiredUrlParams",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var task = method.Invoke(memberAuthService, new object[] { memberAuth, redirectUrl, token, gameTypeId, isTesting });

            // Get the result using reflection to avoid RequiredURLParams access issue
            var resultTask = (Task)task;
            await resultTask;
            var resultProperty = resultTask.GetType().GetProperty("Result");
            var resultObject = resultProperty.GetValue(resultTask);
            var redirectUrlProperty = resultObject.GetType().GetProperty("RedirectUrl");
            var redirectUrlValue = (string)redirectUrlProperty.GetValue(resultObject);

            // Assert
            Assert.Contains("game-5-m/", redirectUrlValue);
        }

        [Fact]
        public async Task BuildRequiredUrlParams_OriginalGamesApptype_WithDesktopDevice_ReturnUrlHasDesktopGamePath()
        {
            // Arrange
            var memberAuth = new MemberAuth
            {
                AppType = AppTypes.OriginalGames,
                Language = "EN",
                Token = "test-token",
                BrowserUserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36"
            };
            var redirectUrl = new Uri("http://example.com/");
            var token = "test-token";
            var gameTypeId = 5;
            var isTesting = false;

            // Act - Using reflection to test the private async method
            var method = typeof(MemberAuthService).GetMethod("BuildRequiredUrlParams",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var task = method.Invoke(memberAuthService, new object[] { memberAuth, redirectUrl, token, gameTypeId, isTesting });

            // Get the result using reflection to avoid RequiredURLParams access issue
            var resultTask = (Task)task;
            await resultTask;
            var resultProperty = resultTask.GetType().GetProperty("Result");
            var resultObject = resultProperty.GetValue(resultTask);
            var redirectUrlProperty = resultObject.GetType().GetProperty("RedirectUrl");
            var redirectUrlValue = (string)redirectUrlProperty.GetValue(resultObject);

            // Assert
            Assert.Contains("game-5-d/", redirectUrlValue);
        }

        [Fact]
        public async Task BuildRequiredUrlParams_NotOriginalGamesApptype_ReturnUrlHasNoGamePath()
        {
            // Arrange
            var memberAuth = new MemberAuth
            {
                AppType = AppTypes.SabaClub,
                Language = "EN",
                Token = "test-token"
            };
            var redirectUrl = new Uri("http://example.com/");
            var token = "test-token";
            var gameTypeId = 5;
            var isTesting = false;

            // Act - Using reflection to test the private async method
            var method = typeof(MemberAuthService).GetMethod("BuildRequiredUrlParams",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var task = method.Invoke(memberAuthService, new object[] { memberAuth, redirectUrl, token, gameTypeId, isTesting });

            // Get the result using reflection to avoid RequiredURLParams access issue
            var resultTask = (Task)task;
            await resultTask;
            var resultProperty = resultTask.GetType().GetProperty("Result");
            var resultObject = resultProperty.GetValue(resultTask);
            var redirectUrlProperty = resultObject.GetType().GetProperty("RedirectUrl");
            var redirectUrlValue = (string)redirectUrlProperty.GetValue(resultObject);

            // Assert
            Assert.DoesNotContain("game-", redirectUrlValue);
            Assert.Contains("http://example.com/", redirectUrlValue);
        }

        [Fact]
        public async Task BuildRequiredUrlParams_IsTesting_TrimsTrailingSlash()
        {
            // Arrange
            var memberAuth = new MemberAuth
            {
                AppType = AppTypes.SabaClub,
                Language = "EN",
                Token = "test-token"
            };
            var redirectUrl = new Uri("http://example.com/");
            var token = "test-token";
            var gameTypeId = 5;
            var isTesting = true;

            // Act - Using reflection to test the private async method
            var method = typeof(MemberAuthService).GetMethod("BuildRequiredUrlParams",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var task = method.Invoke(memberAuthService, new object[] { memberAuth, redirectUrl, token, gameTypeId, isTesting });

            // Get the result using reflection to avoid RequiredURLParams access issue
            var resultTask = (Task)task;
            await resultTask;
            var resultProperty = resultTask.GetType().GetProperty("Result");
            var resultObject = resultProperty.GetValue(resultTask);
            var redirectUrlProperty = resultObject.GetType().GetProperty("RedirectUrl");
            var redirectUrlValue = (string)redirectUrlProperty.GetValue(resultObject);

            // Assert
            Assert.Equal("http://example.com", redirectUrlValue);
        }
    }
}