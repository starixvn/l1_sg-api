using System.Threading.Tasks;
using AutoFixture;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using SG.Api.Authentication;
using SG.Api.Controllers;
using SG.Api.Models;
using SG.Core.Features.DailySummaries;
using Xunit;

namespace SG.Api.Tests.Controllers
{
    public class IntegrityControllerTests
    {
        private readonly IntegrityController controller;
        private readonly IDailySummaryService dailySummaryService;
        private readonly IOperatorRequestAuthentication operatorRequestAuthentication;
        private readonly Fixture fixture = new();

        public IntegrityControllerTests()
        {
            dailySummaryService = Substitute.For<IDailySummaryService>();
            operatorRequestAuthentication = Substitute.For<IOperatorRequestAuthentication>();

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers["Access-Key"] = "Access-Key";
            var controllerContext = new ControllerContext() { HttpContext = httpContext };

            controller = new IntegrityController(operatorRequestAuthentication, dailySummaryService)
            {
                ControllerContext = controllerContext
            };
        }

        [Fact]
        public async Task DailySummarize_InvalidModel_ReturnBadRequest()
        {
            // Arrange
            controller.ModelState.AddModelError("Auth.OperatorId", "OperatorId is required");

            var model = new DailySummaryRequest();
            // Act
            var result = await controller.DailySummarize(model);

            // Assert
            Assert.IsType<BadRequestResult>(result);
        }

        [Fact]
        public async Task DailySummarize_InvalidAccessKey_ReturnUnauthorizedRequest()
        {
            // Arrange
            var model = fixture.Create<DailySummaryRequest>();

            operatorRequestAuthentication.Verify(model.Auth.OperatorId, controller.Request).Returns(false);

            // Act
            var result = await controller.DailySummarize(model);

            // Assert
            await operatorRequestAuthentication.Received().Verify(model.Auth.OperatorId, controller.Request);
            Assert.IsType<UnauthorizedResult>(result);
        }

        [Fact]
        public async Task DailySummarize_ValidModelAndAccessKey_ReturnOkRequest()
        {
            // Arrange
            var model = fixture.Create<DailySummaryRequest>();

            operatorRequestAuthentication.Verify(model.Auth.OperatorId, controller.Request).Returns(true);
            dailySummaryService.Get(model.SummarizedDate, model.Auth.OperatorId).Returns(fixture.Create<DailySummaryResult>());

            // Act
            var result = await controller.DailySummarize(model);

            // Assert
            await operatorRequestAuthentication.Received().Verify(model.Auth.OperatorId, controller.Request);
            await dailySummaryService.Received().Get(model.SummarizedDate, model.Auth.OperatorId);
            Assert.IsType<OkObjectResult>(result);
        }
    }
}