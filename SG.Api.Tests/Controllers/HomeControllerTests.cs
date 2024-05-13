using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using NSubstitute;
using SG.Api.Controllers;
using Xunit;

namespace SG.Api.Tests.Controllers
{
    public class HomeControllerTests
    {
        private readonly HomeController controller;

        public HomeControllerTests()
        {
            var configuration = Substitute.For<IConfiguration>();
            controller = new HomeController(configuration);
        }

        [Fact]
        public void Index_Always_ReturnCorrectResult()
        {
            // Arrange
            var expectedResult = new ContentResult
            {
                Content = "SG Api is running!!! -  - ",
                ContentType = "text/html"
            };

            // Act
            var actualValue = controller.Index() as ContentResult;

            // Assert
            Assert.True(expectedResult.Content == actualValue.Content
                && expectedResult.ContentType == actualValue.ContentType);
        }
    }
}