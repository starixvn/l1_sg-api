using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Sentry;
using SG.Core.Shared;

namespace SG.Core.Features.GameConfigurations
{
    public interface IConfigurationApiService
    {
        Task<GameConfigurationResponse> GetDetailGameConfiguration(int gameId);

    }
    public class ConfigurationApiService : IConfigurationApiService
    {
        private readonly HttpClient httpClient;
        private readonly ConfigurationApiEndPoints configurationApiEndPoints;
        private readonly ISentryClient sentryClient;
        private readonly ICacheManager cache;

        public ConfigurationApiService(
            HttpClient httpClient, 
            ConfigurationApiEndPoints configurationApiEndPoints, 
            ISentryClient sentryClient,
            ICacheManager cache
            )
        {
            this.httpClient = httpClient;
            this.configurationApiEndPoints = configurationApiEndPoints;
            this.sentryClient = sentryClient;
            this.cache = cache;
        }

        public Task<GameConfigurationResponse> GetDetailGameConfiguration(int gameId)
            => Get(configurationApiEndPoints.DetailGameConfiguration, gameId);

        private async Task<GameConfigurationResponse> Get(string apiEndpoint, int gameId)
        {
            var key = $"game-configuration-{gameId}";
            var oldGameConfiguration = cache.GetCache<GameConfigurationResponse>(key);
            if (oldGameConfiguration != default)
            {
                return oldGameConfiguration;
            }
            try
            {
                var res = await httpClient.GetAsync(new Uri($"{apiEndpoint}/{gameId}", UriKind.Relative));
                res.EnsureSuccessStatusCode();
                var result = await res.Content.ReadFromJsonAsync<GameConfigurationResponse>();
                cache.SetCache(key, result);
                return result;
            }
            catch (Exception ex)
            {
                sentryClient.CaptureMessage(ex.ToString(), SentryLevel.Error);
                return null;
            }
        }
    }
}
