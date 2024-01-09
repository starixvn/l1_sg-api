using System.Net.Http;
using System.Net;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Refit;
using SG.Api.Models;
using SG.Core.Features.DailySummaries;
using SG.Core.Shared.Configurations;
using SG.Core.Shared.Models;

namespace SG.Core.Features.ApiCommunicators
{
    public interface ISGApi
    {
        [Post("/Integrity/DailySummarize")]
        Task<SGResponse<DailySummaryResult>> DailySummarize([Header("Access-Key")] string accessKey, DailySummaryRequest model);
    }

    public static class ApiCommunicatorInit
    {
        public static void InitApi(this IServiceCollection services, IAppSettings appSettings)
        {
            var dailySummarySettings = appSettings.DailySummaryApiSettings;
            var client = InitHttpClient(dailySummarySettings.SGApiCloudUrl, dailySummarySettings.ProxyUrl);

            services.AddSingleton(RestService.For<ISGApi>(client, new RefitSettings(new NewtonsoftJsonContentSerializer())));
        }

        private static HttpClient InitHttpClient(string url, string proxy)
        {
            HttpClientHandler handler = null;

            if (!string.IsNullOrEmpty(proxy))
            {
                handler = new HttpClientHandler
                {
                    Proxy = new WebProxy(proxy),
                    UseProxy = true,
                };
            }

            var client = (handler == null) ? new HttpClient() : new HttpClient(handler);
            client.BaseAddress = new Uri(url);

            return client;
        }
    }
}