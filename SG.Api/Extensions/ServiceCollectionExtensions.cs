using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;
using MongoDB.Driver.Core.Extensions.DiagnosticSources;
using SG.Api.Authentication;
using SG.Core.Features.DailySummaries;
using SG.Core.Features.Log;
using SG.Core.Features.MemberAuthentications;
using SG.Core.Features.Operators;
using SG.Core.Features.ResultApiCommunicators;
using SG.Core.Shared.Configurations;

namespace SG.Api.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void AddMongoDB(this IServiceCollection services, IConfiguration configuration)
        {
            var camelIgnoringExtraElementConventions = new ConventionPack
            {
                new CamelCaseElementNameConvention(),
                new IgnoreExtraElementsConvention(true)
            };

            ConventionRegistry.Register(nameof(camelIgnoringExtraElementConventions), camelIgnoringExtraElementConventions, t => true);

            var clientSettings = MongoClientSettings.FromUrl(new MongoUrl(configuration["MongoDBSettings:ConnectionString"]));
            clientSettings.ClusterConfigurator = cb => cb.Subscribe(new DiagnosticsActivityEventSubscriber());

            services.AddSingleton<IMongoClient>(new MongoClient(clientSettings));
        }

        public static void AddServices(this IServiceCollection services)
        {
            services.AddScoped<IOperatorRequestAuthentication, OperatorRequestAuthentication>();

            services.AddScoped<IOperatorDataAccess, OperatorDataAccess>();
            services.AddScoped<ICurrencyDataAccess, CurrencyDataAccess>();
            services.AddScoped<IOperatorDataService, OperatorDataService>();

            services.AddScoped<IDailySummaryDataAccess, DailySummaryDataAccess>();
            services.AddScoped<IDailySummaryService, DailySummaryService>();

            services.AddScoped<ILogDataAccess, LogDataAccess>();
            services.AddScoped<ILogService, LogService>();

            services.AddScoped<IMemberAuthDataAccess, MemberAuthDataAccess>();
            services.AddScoped<IMemberAuthService, MemberAuthService>();
        }

        public static void AddAppSettings(this IServiceCollection services, IConfiguration configuration)
        {
            var appSettings = new AppSettings();

            configuration.GetSection("AppSettings").Bind(appSettings);
            services.AddSingleton<IAppSettings>(appSettings);

            services.AddSingleton(appSettings.ResultApiSettings.Endpoints);
            services.AddHttpClient<IResultApiService, ResultApiService>(httpClient =>
            {
                httpClient.BaseAddress = appSettings.ResultApiSettings.BaseUrl;
            });
        }

        public static void AddSytemSetting(this IServiceCollection services, IConfiguration configuration)
        {
            var systemSettings = new SystemSetting(configuration.GetSection("SystemType").Get<string>());

            services.AddSingleton<ISystemSetting>(systemSettings);
        }
    }
}