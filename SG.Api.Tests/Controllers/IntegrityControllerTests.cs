using System;
using System.Threading.Tasks;
using AutoFixture;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Sentry;
using SG.Api.Authentication;
using SG.Api.Controllers;
using SG.Api.Models;
using SG.Core.Features.ApiCommunicators;
using SG.Core.Features.DailySummaries;
using SG.Core.Features.Operators;
using SG.Core.Shared.Configurations;
using Xunit;

namespace SG.Api.Tests.Controllers
{
    public class IntegrityControllerTests
    {
        private readonly IntegrityController controller;
        private readonly IDailySummaryService dailySummaryService;
        private readonly IOperatorRequestAuthentication operatorRequestAuthentication;
        private readonly ISGApi sgApi;
        private readonly IAppSettings appSettings;
        private readonly Fixture fixture = new();
        private readonly IOperatorDataService operatorDataService;
        private readonly ISentryClient sentryClient;

        public IntegrityControllerTests()
        {
            dailySummaryService = Substitute.For<IDailySummaryService>();
            operatorRequestAuthentication = Substitute.For<IOperatorRequestAuthentication>();
            sgApi = Substitute.For<ISGApi>();
            appSettings = Substitute.For<AppSettings>();
            operatorDataService = Substitute.For<IOperatorDataService>();
            sentryClient = Substitute.For<ISentryClient>();
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers["Access-Key"] = "Access-Key";
            var controllerContext = new ControllerContext() { HttpContext = httpContext };

            controller = new IntegrityController(operatorRequestAuthentication, dailySummaryService, sgApi, appSettings, operatorDataService, sentryClient)
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

            appSettings.DailySummaryApiSettings = new DailySummaryApiSettings { IsCloud = true };
            operatorRequestAuthentication.Verify(model.Auth.OperatorId, controller.Request).Returns(true);
            dailySummaryService.Get(model.SummarizedDate, model.Auth.OperatorId, model.SiteId).Returns(fixture.Create<DailySummaryResult>());

            // Act
            var result = await controller.DailySummarize(model);

            // Assert
            await operatorRequestAuthentication.Received().Verify(model.Auth.OperatorId, controller.Request);
            await dailySummaryService.Received().Get(model.SummarizedDate, model.Auth.OperatorId, model.SiteId);
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task DailySummarize_IsOnPremise_CallCloudApi()
        {
            // Arrange
            var model = fixture.Create<DailySummaryRequest>();
            this.operatorDataService.GetById(Arg.Any<string>()).Returns(new Operator { SecretKey = "key" });
            appSettings.DailySummaryApiSettings = new DailySummaryApiSettings { IsCloud = false, EnableCloud = true, SGApiCloudUrl = "http://demo.com" };
            operatorRequestAuthentication.Verify(model.Auth.OperatorId, controller.Request).Returns(true);

            // Act
            var result = await controller.DailySummarize(model);

            // Assert
            await sgApi.Received().DailySummarize(Arg.Any<string>(), model);
        }

        [Fact]
        public async Task DailySummarize_CallCloudAndThrowException_LogException()
        {
            // Arrange
            var model = fixture.Create<DailySummaryRequest>();
            this.operatorDataService.GetById(Arg.Any<string>()).Returns(new Operator { SecretKey = "key" });
            appSettings.DailySummaryApiSettings = new DailySummaryApiSettings { IsCloud = false, EnableCloud = true, SGApiCloudUrl = "http://demo.com" };
            operatorRequestAuthentication.Verify(model.Auth.OperatorId, controller.Request).Returns(true);
            dailySummaryService.Get(model.SummarizedDate, model.Auth.OperatorId).Returns(fixture.Create<DailySummaryResult>());
            var apiException = new Exception("Api Error");
            sgApi.DailySummarize(Arg.Any<string>(), Arg.Any<DailySummaryRequest>()).Throws(apiException);
            
            // Act
            var result = await controller.DailySummarize(model);

            // Assert
            this.sentryClient.Received().CaptureException(apiException);
        }
    }
}