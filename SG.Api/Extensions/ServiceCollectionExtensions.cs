using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;
using SG.Api.Authentication;
using SG.Core.Features.DailySummaries;
using SG.Core.Features.Log;
using SG.Core.Features.Operators;
using SG.Core.Features.ResultApiCommunicators;

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
            services.AddSingleton<IMongoClient>(new MongoClient(configuration["MongoDBSettings:ConnectionString"]));
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
        }

        public static void AddResultApiServices(this IServiceCollection services, IConfiguration configuration)
        {
            var resultApiSetting = new ResultApiSettings();

            configuration.GetSection("AppSettings:ResultApiSettings").Bind(resultApiSetting);
            services.AddSingleton(resultApiSetting.Endpoints);
            services.AddHttpClient<IResultApiService, ResultApiService>(httpClient =>
            {
                httpClient.BaseAddress = resultApiSetting.BaseUrl;
            });
        }
    }
}