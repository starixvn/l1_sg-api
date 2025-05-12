using System.Threading.Tasks;
using AutoFixture;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using SG.Api.Authentication;
using SG.Api.Controllers;
using SG.Api.Models;
using SG.Core.Features.MemberAuthentications;
using Xunit;

namespace SG.Api.Tests.Controllers
{
    public class MemberAuthControllerTests
    {
        private readonly IMemberAuthService memberAuthService;
        private readonly IOperatorRequestAuthentication operatorRequestAuthentication;
        private readonly MemberAuthController controller;
        private readonly Fixture fixture = new();

        public MemberAuthControllerTests()
        {
            memberAuthService = Substitute.For<IMemberAuthService>();
            operatorRequestAuthentication = Substitute.For<IOperatorRequestAuthentication>();

            controller = new MemberAuthController(memberAuthService, operatorRequestAuthentication);
        }

        [Fact]
        public async Task Update_InvalidRequest_ReturnUnauthorizedResult()
        {
            // Arrange
            var model = fixture.Create<UpdateMemberAuthRequest>();
            operatorRequestAuthentication.Verify(Arg.Any<string>(), Arg.Any<HttpRequest>()).Returns(false);

            // Act
            var actualValue = (await controller.Update(model)).Result as OkObjectResult;

            // Assert
            Assert.Equal(SingleSignOnResult.Unauthorize, ((SingleSignOnResult)actualValue.Value).Code);
        }

        [Fact]
        public async Task Update_ValidRequest_ReturnUrlCorrectly()
        {
            // Arrange
            var model = fixture.Create<UpdateMemberAuthRequest>();
            operatorRequestAuthentication.Verify(Arg.Any<string>(), Arg.Any<HttpRequest>()).Returns(true);
            var expected = SingleSignOnResult.CreateSuccessResponse("https://domain.com");
            memberAuthService.UpdateMemberAuth(Arg.Any<UpdateMemberAuth>()).Returns(expected);

            // Act
            var actualValue = (await controller.Update(model)).Result as OkObjectResult;

            // Assert
            Assert.Equal(expected, (SingleSignOnResult)actualValue.Value);
        }
    }
}