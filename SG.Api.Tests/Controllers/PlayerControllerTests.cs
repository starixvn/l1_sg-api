using System.Threading.Tasks;
using AutoFixture;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using SG.Api.Authentication;
using SG.Api.Controllers;
using SG.Api.Extensions;
using SG.Core.Features.EndRoundInfo;
using SG.Core.Features.Log;
using SG.Core.Features.Operators;
using SG.Core.Features.ResultApiCommunicators;
using SG.Core.Shared.Configurations;
using SG.Core.Shared.Models;
using Xunit;
using EndRoundInfoRequest = SG.Core.Features.EndRoundInfo.EndRoundInfoRequest;

namespace SG.Api.Tests.Controllers
{
    public class PlayerControllerTests
    {
        private readonly PlayerController controller;
        private readonly IOperatorRequestAuthentication operatorRequestAuthentication;
        private readonly IResultApiService resultApiService;
        private readonly IOperatorDataService operatorDataService;
        private readonly ILogService logService;
        private readonly Fixture fixture = new();
        private readonly ISystemSetting systemSetting;

        public PlayerControllerTests()
        {
            operatorRequestAuthentication = Substitute.For<IOperatorRequestAuthentication>();
            resultApiService = Substitute.For<IResultApiService>();
            operatorDataService = Substitute.For<IOperatorDataService>();
            logService = Substitute.For<ILogService>();
            systemSetting = Substitute.For<ISystemSetting>();

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers["Access-Key"] = "Access-Key";
            var controllerContext = new ControllerContext() { HttpContext = httpContext };

            controller = new PlayerController(operatorRequestAuthentication, resultApiService, operatorDataService, logService, systemSetting)
            {
                ControllerContext = controllerContext
            };
        }

        [Fact]
        public async Task RetrieveEndRoundInfo_ValidModelAndAccessKey_ReturnOkResult()
        {
            // Arrange
            var model = fixture.Create<Models.EndRoundInfoRequest>();
            operatorDataService.GetById(model.Auth.OperatorId).Returns(fixture.Create<Operator>());
            operatorRequestAuthentication.Verify(model.Auth.OperatorId, controller.Request).Returns(true);

            var sgResponse = fixture.Create<SGResponse<EndRoundInfoResult>>();
            resultApiService.RetrieveEndRoundInfoAsync(Arg.Any<EndRoundInfoRequest>()).Returns(sgResponse);

            // Act
            var result = await controller.RetrieveEndRoundInfo(model) as OkObjectResult;

            // Assert
            Assert.Equal(sgResponse, result.Value);
        }

        [Fact]
        public async Task RetrieveEndRoundInfo_InvalidModel_ReturnBadRequest()
        {
            // Arrange
            controller.ModelState.AddModelError("Auth.OperatorId", "OperatorId is required");

            var model = new Models.EndRoundInfoRequest();
            // Act
            var result = await controller.RetrieveEndRoundInfo(model);

            // Assert
            Assert.IsType<BadRequestResult>(result);
        }

        [Fact]
        public async Task DailySummarize_InvalidAccessKey_ReturnUnauthorizedRequest()
        {
            // Arrange
            var model = fixture.Create<Models.EndRoundInfoRequest>();

            operatorRequestAuthentication.Verify(model.Auth.OperatorId, controller.Request).Returns(false);

            // Act
            var result = await controller.RetrieveEndRoundInfo(model);

            // Assert
            Assert.IsType<UnauthorizedResult>(result);
        }

        [Fact]
        public async Task RetrieveEndRoundInfo_ValidModelAndAccessKey_FormatPlayerIdCorrectly()
        {
            // Arrange
            var model = fixture.Create<Models.EndRoundInfoRequest>();
            operatorDataService.GetById(model.Auth.OperatorId).Returns(fixture.Create<Operator>());
            operatorRequestAuthentication.Verify(model.Auth.OperatorId, controller.Request).Returns(true);

            var sgResponse = fixture.Create<SGResponse<EndRoundInfoResult>>();
            var formattedPlayerId = string.Empty;
            resultApiService.RetrieveEndRoundInfoAsync(Arg.Do<EndRoundInfoRequest>(x => formattedPlayerId = x.PlayerId)).Returns(sgResponse);

            // Act
            var result = await controller.RetrieveEndRoundInfo(model) as OkObjectResult;

            // Assert
            Assert.Equal(StringFormat.FormatPlayerId(model.Auth.OperatorId, model.SiteId, model.PlayerId), formattedPlayerId);
        }
    }
}