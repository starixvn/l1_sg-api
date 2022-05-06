using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using SG.Api.Authentication;
using Xunit;

namespace SG.Api.Tests
{
    public class StartupTests
    {
        private readonly IConfiguration configuration;
        private readonly Startup startup;

        public StartupTests()
        {
            configuration = Substitute.For<IConfiguration>();

            AddMongoDBSettingsConfigsSection(configuration);
            AddResultApiSettingsConfigsSection(configuration);

            startup = new Startup(configuration);
        }

        private static void AddMongoDBSettingsConfigsSection(IConfiguration configuration)
        {
            configuration["MongoDBSettings:ConnectionString"].Returns("mongodb://127.0.0.1:27017");
        }

        private static void AddResultApiSettingsConfigsSection(IConfiguration configuration)
        {
            var resultApiSettingsSection = Substitute.For<IConfigurationSection>();

            var endPoints = Substitute.For<IConfigurationSection>();
            var endRoundInfo = Substitute.For<IConfigurationSection>();
            endRoundInfo.Value = "EndRoundInfo";
            endPoints.GetChildren().Returns(new List<IConfigurationSection> { endRoundInfo });
            endPoints.GetSection("EndRoundInfo").Returns(endRoundInfo);
            resultApiSettingsSection.GetSection("Endpoints").Returns(endPoints);

            var baseUrl = Substitute.For<IConfigurationSection>();
            baseUrl.Value = "http://localhost:9007/";
            resultApiSettingsSection.GetSection("BaseUrl").Returns(baseUrl);

            resultApiSettingsSection.GetChildren().Returns(new List<IConfigurationSection> { baseUrl, endPoints });

            var highStakeSection = Substitute.For<IConfigurationSection>();
            highStakeSection.Value = "60";
            resultApiSettingsSection.GetSection("HighStake").Returns(highStakeSection);

            configuration.GetSection("AppSettings:ResultApiSettings").Returns(resultApiSettingsSection);
        }

        [Fact]
        public void ConfigureServices_Always_RegisterCorrectServices()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            startup.ConfigureServices(services);

            // Assert
            var provider = services.BuildServiceProvider();
            var result = provider.GetRequiredService<IOperatorRequestAuthentication>();
            Assert.NotNull(result);
        }
    }
}