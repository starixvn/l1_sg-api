using System.Threading.Tasks;
using AutoFixture;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using SG.Api.Authentication;
using SG.Api.Controllers;
using SG.Api.Models;
using SG.Core.Features.Log;
using SG.Core.Features.Promotions;
using SG.Core.Shared.Models;
using Xunit;

namespace SG.Api.Tests.Controllers
{
    public class PromotionControllerTests
    {
        private readonly PromotionController controller;
        private readonly IOperatorRequestAuthentication operatorRequestAuthentication;
        private readonly IPromotionService promotionService;
        private readonly ILogService logService;
        private readonly Fixture fixture = new();

        public PromotionControllerTests()
        {
            operatorRequestAuthentication = Substitute.For<IOperatorRequestAuthentication>();
            promotionService = Substitute.For<IPromotionService>();
            logService = Substitute.For<ILogService>();

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers["Access-Key"] = "Access-Key";
            var controllerContext = new ControllerContext() { HttpContext = httpContext };

            controller = new PromotionController(operatorRequestAuthentication, promotionService, logService)
            {
                ControllerContext = controllerContext
            };
        }

        [Fact]
        public async Task GetPromotion_ValidModelAndAccessKey_ReturnOkResult()
        {
            // Arrange
            var model = fixture.Create<PromotionRequest>();
            operatorRequestAuthentication.Verify(model.Auth.OperatorId, controller.Request).Returns(true);

            var promotionResult = fixture.Create<PromotionResult>();
            promotionService.GetPromotionAsync(model.PromotionCode, model.Auth.OperatorId, model.Seq).Returns(promotionResult);

            // Act
            var result = await controller.GetPromotion(model) as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            var response = result.Value as SGResponse<PromotionResult>;
            Assert.NotNull(response);
            Assert.Equal(promotionResult, response.Result);
        }

        [Fact]
        public async Task GetPromotion_InvalidModel_ReturnBadRequest()
        {
            // Arrange
            controller.ModelState.AddModelError("Auth.OperatorId", "OperatorId is required");

            var model = new PromotionRequest();
            
            // Act
            var result = await controller.GetPromotion(model);

            // Assert
            Assert.IsType<BadRequestResult>(result);
        }

        [Fact]
        public async Task GetPromotion_InvalidAccessKey_ReturnUnauthorizedRequest()
        {
            // Arrange
            var model = fixture.Create<PromotionRequest>();
            operatorRequestAuthentication.Verify(model.Auth.OperatorId, controller.Request).Returns(false);

            // Act
            var result = await controller.GetPromotion(model);

            // Assert
            Assert.IsType<UnauthorizedResult>(result);
        }

        [Fact]
        public async Task GetPromotion_ValidRequest_LogsPromotionRequest()
        {
            // Arrange
            var model = fixture.Create<PromotionRequest>();
            operatorRequestAuthentication.Verify(model.Auth.OperatorId, controller.Request).Returns(true);

            var promotionResult = fixture.Create<PromotionResult>();
            promotionService.GetPromotionAsync(model.PromotionCode, model.Auth.OperatorId, model.Seq).Returns(promotionResult);

            // Act
            await controller.GetPromotion(model);

            // Assert
            await logService.Received(1).LogPromotionAsync(Arg.Any<SBLog>());
        }
    }
} 