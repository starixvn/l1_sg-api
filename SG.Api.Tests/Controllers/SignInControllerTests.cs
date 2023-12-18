using System.Threading.Tasks;
using AutoFixture;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using SG.Api.Authentication;
using SG.Api.Controllers;
using SG.Api.Extensions;
using SG.Api.Models;
using SG.Core.Features.Log;
using SG.Core.Features.MemberAuthentications;
using SG.Core.Features.Operators;
using SG.Core.Shared.Configurations;
using SG.Core.Shared.Constants;
using Xunit;

namespace SG.Api.Tests.Controllers
{
    public class SignInControllerTests
    {
        private readonly IMemberAuthService memberAuthService;
        private readonly IOperatorDataService operatorDataService;
        private readonly ILogService logService;
        private readonly IOperatorRequestAuthentication operatorRequestAuthentication;
        private readonly SignInController controller;
        private readonly Fixture fixture = new();
        private readonly ISystemSetting systemSetting;

        public SignInControllerTests()
        {
            memberAuthService = Substitute.For<IMemberAuthService>();
            operatorDataService = Substitute.For<IOperatorDataService>();
            logService = Substitute.For<ILogService>();
            operatorRequestAuthentication = Substitute.For<IOperatorRequestAuthentication>();
            systemSetting = Substitute.For<ISystemSetting>();

            controller = new SignInController(memberAuthService, operatorDataService, logService, operatorRequestAuthentication, systemSetting);
        }

        [Fact]
        public async Task Index_InvalidClient_ReturnUnauthorizedResult()
        {
            // Arrange
            var model = fixture.Create<SignIn>();
            model.Auth = null;
            operatorDataService.VerifyWithCurrency(Arg.Any<string>(), Arg.Any<string>()).Returns(false);

            // Act
            var actualValue = await controller.Index(model) as UnauthorizedResult;

            // Assert
            Assert.NotNull(actualValue);
        }

        [Fact]
        public async Task Index_ValidClientWithEmptyIP_ReturnOkResult()
        {
            // Arrange
            var model = fixture.Create<SignIn>();
            model.IP = string.Empty;
            operatorDataService.VerifyWithCurrency(Arg.Any<string>(), Arg.Any<string>()).Returns(true);
            operatorRequestAuthentication.Verify(Arg.Any<string>(), Arg.Any<HttpRequest>()).Returns(true);

            var result = fixture.Create<SingleSignOnResult>();
            memberAuthService.SignIn(Arg.Any<MemberAuth>(), Arg.Any<byte>()).Returns(result);

            // Act
            var actualValue = await controller.Index(model) as OkObjectResult;

            // Assert
            Assert.NotNull(actualValue);
        }

        [Fact]
        public async Task Index_ValidClientWithInvalidAppType_ReturnOkResult()
        {
            // Arrange
            var model = fixture.Create<SignIn>();
            model.IP = "IP";
            model.Auth = new Auth { OperatorId = "VG" };
            model.AppType = 0;
            operatorDataService.VerifyWithCurrency(Arg.Any<string>(), Arg.Any<string>()).Returns(true);
            operatorRequestAuthentication.Verify(Arg.Any<string>(), Arg.Any<HttpRequest>()).Returns(true);

            var result = fixture.Create<SingleSignOnResult>();
            memberAuthService.SignIn(Arg.Any<MemberAuth>(), Arg.Any<byte>()).Returns(result);

            // Act
            var actualValue = await controller.Index(model) as OkObjectResult;

            // Assert
            Assert.NotNull(actualValue);
        }

        [Fact]
        public async Task Index_ValidClientWithValidAppType_ReturnOkResult()
        {
            // Arrange
            var model = fixture.Create<SignIn>();
            model.IP = "IP";
            model.Auth = new Auth();
            model.AppType = AppTypes.SabaClub;
            operatorDataService.VerifyWithCurrency(Arg.Any<string>(), Arg.Any<string>()).Returns(true);
            operatorRequestAuthentication.Verify(Arg.Any<string>(), Arg.Any<HttpRequest>()).Returns(true);

            var result = fixture.Create<SingleSignOnResult>();
            memberAuthService.SignIn(Arg.Any<MemberAuth>(), Arg.Any<byte>()).Returns(result);

            // Act
            var actualValue = await controller.Index(model) as OkObjectResult;

            // Assert
            Assert.NotNull(actualValue);
        }

        [Fact]
        public async Task Index_ValidClient_FormatPlayerIdCorrectly()
        {
            // Arrange
            var model = fixture.Create<SignIn>();
            model.IP = "IP";
            model.Auth = new Auth { OperatorId = "VG" };
            model.AppType = AppTypes.SabaClub;

            operatorDataService.VerifyWithCurrency(Arg.Any<string>(), Arg.Any<string>()).Returns(true);
            operatorRequestAuthentication.Verify(Arg.Any<string>(), Arg.Any<HttpRequest>()).Returns(true);

            var result = fixture.Create<SingleSignOnResult>();
            var formattedPlayerId = string.Empty;
            memberAuthService.SignIn(Arg.Do<MemberAuth>(x => formattedPlayerId =  x.PlayerId), Arg.Any<byte>()).Returns(result);

            // Act
            var actualValue = await controller.Index(model) as OkObjectResult;

            // Assert
            Assert.Equal(StringFormat.FormatPlayerId(model.SiteId, model.PlayerId), formattedPlayerId);
        }

    }
}