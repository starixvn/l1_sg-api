using AutoFixture;
using Microsoft.AspNetCore.Mvc;
using SG.Api.Controllers;
using SG.Api.Models;
using Xunit;

namespace SG.Api.Tests.Controllers
{
    public class PublicControllerTests
    {
        private readonly Fixture fixture = new();

        [Fact]
        public void HashCheck_ValidModel_ReturnResult()
        {
            //Arrange
            var model = fixture.Create<HashCheck>();
            var controller = new PublicController();

            //Act
            var result = controller.HashCheck(model);

            //Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.False(string.IsNullOrWhiteSpace(okResult.Value.ToString()));
        }

        [Fact]
        public void HashCheck_InvalidModel_ReturnBadRequest()
        {
            //Arrange
            var model = fixture.Create<HashCheck>();
            var controller = new PublicController();
            controller.ModelState.AddModelError("Data", "Data is required");
            //Act
            var result = controller.HashCheck(model);

            //Assert
            Assert.IsType<BadRequestResult>(result);
        }
    }
}