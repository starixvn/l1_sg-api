using System.IO;
using System.Text;
using System.Threading.Tasks;
using AutoFixture;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using NSubstitute;
using Sentry;
using SG.Api.Authentication;
using SG.Core.Features.Operators;
using Xunit;

namespace SG.Api.Tests.Authentication
{
    public class OperatorRequestAuthenticationTests
    {
        private readonly IOperatorRequestAuthentication operatorRequestAuthentication;
        private readonly IOperatorDataService operatorDataService;
        private readonly HttpRequest httpRequest;
        private readonly Fixture fixture = new();
        private readonly ISentryClient sentryClient;

        public OperatorRequestAuthenticationTests()
        {
            operatorDataService = Substitute.For<IOperatorDataService>();
            sentryClient = Substitute.For<ISentryClient>();

            operatorRequestAuthentication = new OperatorRequestAuthentication(operatorDataService, sentryClient);

            httpRequest = Substitute.For<HttpRequest>();
            httpRequest.Body = new MemoryStream(Encoding.ASCII.GetBytes("data"));
        }


        [Fact]
        public async Task Verify_ValidOperatorIdAndAccessKey_ReturnTrue()
        {
            //Arrange
            httpRequest.Headers["Access-Key"].Returns((StringValues)"GywWt1vSqHDBFBU8zaW8/KYzFLxyL6Fg1pDeEzzLuds=");
            var operatorId = fixture.Create<string>();
            operatorDataService.GetById(operatorId).Returns(new Operator { Id = operatorId, SecretKey = "secret" });
        
            // Act
            var result = await operatorRequestAuthentication.Verify(operatorId, httpRequest);

            //Assert
            Assert.True(result);
        }

        [Fact]
        public async Task Verify_InvalidOperatorId_ReturnFalse()
        {
            //Arrange
            var operatorId = fixture.Create<string>();

            // Act
            var result = await operatorRequestAuthentication.Verify(operatorId, httpRequest);

            //Assert
            Assert.False(result);
        }

        [Fact]
        public async Task Verify_InvalidAccessKey_ReturnFalse()
        {
            //Arrange
            var operatorId = fixture.Create<string>();
            operatorDataService.GetById(operatorId).Returns(new Operator { Id = operatorId, SecretKey = "secret" });

            // Act
            var result = await operatorRequestAuthentication.Verify(operatorId, httpRequest);

            //Assert
            Assert.False(result);
        }
    }
}