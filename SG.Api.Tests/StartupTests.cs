using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using SG.Api.Authentication;
using SG.Core.Shared.Configurations;
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
            var connectionString = Substitute.For<IConfigurationSection>();
            connectionString.Value = "mongodb://127.0.0.1:27017";

            var databaseSettings = Substitute.For<IConfigurationSection>();
            databaseSettings.GetChildren().Returns(new List<IConfigurationSection> { connectionString });
            databaseSettings.GetSection("ConnectionString").Returns(connectionString);
            configuration.GetSection("MongoDBSettings").Returns(databaseSettings);
        }

        private static void AddResultApiSettingsConfigsSection(IConfiguration configuration)
        {
            var resultApiSettingsSection = Substitute.For<IConfigurationSection>();
            var appSettingsSection = Substitute.For<IConfigurationSection>();

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

            var tokenExpiredSection = Substitute.For<IConfigurationSection>();
            tokenExpiredSection.Value = "60";
            appSettingsSection.GetSection("TokenExpiredSeconds").Returns(tokenExpiredSection);

            appSettingsSection.GetSection("ResultApiSettings").Returns(resultApiSettingsSection);
            appSettingsSection.GetChildren().Returns(new List<IConfigurationSection> { resultApiSettingsSection, tokenExpiredSection });

            configuration.GetSection("AppSettings").Returns(appSettingsSection);
        }

        [Fact]
        public void ConfigureServices_Always_RegisterCorrectServices()
        {
            // Arrange
            var services = new ServiceCollection();
            // Register a mock ISentryClient for OperatorRequestAuthentication
            var sentryClient = Substitute.For<Sentry.ISentryClient>();
            services.AddSingleton(sentryClient);

            // Act
            startup.ConfigureServices(services);

            // Assert
            var provider = services.BuildServiceProvider();
            var result = provider.GetRequiredService<IOperatorRequestAuthentication>();
            Assert.NotNull(result);
        }
    }
}