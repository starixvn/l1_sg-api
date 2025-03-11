using System;
using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.Xunit2;
using NSubstitute;
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
                ClubVersionUrl = "http://local/",
                MobiVersionUrl = "http://mobi/",
                MiniVersionUrl = "http://mini/"
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
                ClubVersionUrl = "http://local/",
                MobiVersionUrl = "http://mobi/",
                MiniVersionUrl = "http://mini/"
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
                ClubVersionUrl = "http://local/",
                MobiVersionUrl = "http://mobi/",
                MiniVersionUrl = "http://mini/",
                TestCurrencyUrl = "http://test/"
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
                ClubVersionUrl = "http://local/",
                MobiVersionUrl = "http://mobi/",
                MiniVersionUrl = "http://mini/"
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
                ClubVersionUrl = "http://local/",
                MobiVersionUrl = "http://mobi/",
                MiniVersionUrl = "http://mini/",
                TestCurrencyUrl = "http://test/"
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
                ClubVersionUrl = "http://local/",
                MobiVersionUrl = "http://mobi/",
                MiniVersionUrl = "http://mini/",
                TestCurrencyUrl = "http://test/"
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
                ClubVersionUrl = "http://local/",
                MobiVersionUrl = "http://mobi/",
                MiniVersionUrl = "http://mini/",
                TestCurrencyUrl = "http://test/"
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
    }
}