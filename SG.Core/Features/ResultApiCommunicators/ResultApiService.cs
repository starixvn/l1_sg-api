using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Sentry;
using SG.Core.Features.EndRoundInfo;
using SG.Core.Shared.Constants;
using SG.Core.Shared.Models;

namespace SG.Core.Features.ResultApiCommunicators
{
    public interface IResultApiService
    {
        Task<SGResponse<EndRoundInfoResult>> RetrieveEndRoundInfoAsync(EndRoundInfoRequest request);
    }

    public class ResultApiService : IResultApiService
    {
        private readonly HttpClient httpClient;
        private readonly ResultApiEndPoints apiEndPoints;

        public ResultApiService(HttpClient httpClient, ResultApiEndPoints apiEndPoints)
        {
            this.httpClient = httpClient;
            this.apiEndPoints = apiEndPoints;
        }

        public Task<SGResponse<EndRoundInfoResult>> RetrieveEndRoundInfoAsync(EndRoundInfoRequest request)
          => Post<EndRoundInfoResult>(apiEndPoints.EndRoundInfo, request);

        public async Task<SGResponse<T>> Post<T>(string apiEndpoint, EndRoundInfoRequest request) where T : SGResult, new()
        {
            var response = await Post(apiEndpoint, request);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();

                return new SGResponse<T>
                {
                    TimeStamp = DateTimeOffset.UtcNow,
                    Result = JsonConvert.DeserializeObject<T>(responseContent)
                };
            }

            var failureResponse = await CreateErrorResponse<T>(response);

            SentrySdk.CaptureMessage(
                $"Request to Result API's {apiEndpoint} failed at {DateTimeOffset.UtcNow} - {failureResponse.Result.ErrorDescription}",
                SentryLevel.Warning);

            return failureResponse;
        }

        private Task<HttpResponseMessage> Post(string apiEndpoint, EndRoundInfoRequest request)
        {
            const string defaultMediaType = "application/json";

            var content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, defaultMediaType);

            return httpClient.PostAsync(new Uri(apiEndpoint, UriKind.Relative), content);
        }

        private static async Task<SGResponse<T>> CreateErrorResponse<T>(HttpResponseMessage response) where T : SGResult, new()
        => new SGResponse<T>
        {
            TimeStamp = DateTimeOffset.UtcNow,
            Result = new T
            {
                ErrorCode = ErrorCodes.InternalError,
                ErrorDescription = $"{response.StatusCode}-{response.ReasonPhrase}-{await response.Content.ReadAsStringAsync()}"
            }
        };
    }
}