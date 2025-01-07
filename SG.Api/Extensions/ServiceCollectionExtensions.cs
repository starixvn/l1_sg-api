using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson.Serialization;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using SG.Api.Authentication;
using SG.Core.Features.ApiCommunicators;
using SG.Core.Features.DailySummaries;
using SG.Core.Features.Log;
using SG.Core.Features.MemberAuthentications;
using SG.Core.Features.Operators;
using SG.Core.Features.ResultApiCommunicators;
using SG.Core.Shared.Configurations;
using SG.Core.Features.User;

namespace SG.Api.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void AddMongoDB(this IServiceCollection services, IConfiguration configuration)
        {
            var dbSettings = new DatabaseSettings();

            configuration.GetSection("MongoDBSettings").Bind(dbSettings);
            services.AddSingleton<IDatabaseSettings>(dbSettings);

            var camelIgnoringExtraElementConventions = new ConventionPack
            {
                new CamelCaseElementNameConvention(),
                new IgnoreExtraElementsConvention(true)
            };

            ConventionRegistry.Register(nameof(camelIgnoringExtraElementConventions), camelIgnoringExtraElementConventions, t => true);
            services.AddSingleton<IMongoClient>(new MongoClient(dbSettings.ConnectionString));

            BsonSerializer.RegisterSerializer(typeof(DateTimeOffset), new DateTimeOffsetSerializer(BsonType.DateTime));

            var objectSerializer = new ObjectSerializer(type => ObjectSerializer.DefaultAllowedTypes(type) || type.FullName.StartsWith("SG"));
            BsonSerializer.RegisterSerializer(objectSerializer);
        }

        public static void AddServices(this IServiceCollection services)
        {
            services.AddScoped<IOperatorRequestAuthentication, OperatorRequestAuthentication>();

            services.AddScoped<IOperatorDataAccess, OperatorDataAccess>();
            services.AddScoped<ICurrencyDataAccess, CurrencyDataAccess>();
            services.AddScoped<IOperatorDataService, OperatorDataService>();

            services.AddScoped<ICurrencyGroupDataAccess, CurrencyGroupDataAccess>();
            services.AddScoped<ICurrencyGroupDataService, CurrencyGroupDataService>();

            services.AddScoped<IDailySummaryDataAccess, DailySummaryDataAccess>();
            services.AddScoped<IDailySummaryService, DailySummaryService>();

            services.AddScoped<ILogDataAccess, LogDataAccess>();
            services.AddScoped<ILogService, LogService>();

            services.AddScoped<IMemberAuthDataAccess, MemberAuthDataAccess>();
            services.AddScoped<IMemberAuthService, MemberAuthService>();

            services.AddScoped<IUserDataAccess, UserDataAccess>();
            services.AddScoped<IUserDataService, UserDataService>();
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

            if (appSettings.DailySummaryApiSettings != null)
            {
                ApiCommunicatorInit.InitApi(services, appSettings);
            }
        }

        public static void AddSytemSetting(this IServiceCollection services, IConfiguration configuration)
        {
            var systemType = configuration.GetSection("SystemType").Get<string>();
            var disableJackpotOperators = configuration.GetSection("DisableJackpotOperators").Get<IEnumerable<string>>();
            var systemSettings = new SystemSetting(systemType, disableJackpotOperators);

            services.AddSingleton<ISystemSetting>(systemSettings);
        }
    }
}