using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using AutoFixture.Xunit2;
using Newtonsoft.Json;
using NSubstitute;
using Sentry;
using SG.Core.Features.EndRoundInfo;
using SG.Core.Features.ResultApiCommunicators;
using SG.Core.Shared.Constants;
using SG.Core.Shared.Models;
using SG.Core.Tests.Shared;
using Xunit;

namespace SG.Core.Tests.Features.ResultApiCommunicators
{
    public class ResultApiServiceTests
    {
        private readonly ISentryClient sentryClient;
        private readonly ResultApiEndPoints apiEndPoints = new() 
        {
            EndRoundInfo = "EndRoundInfo"
        };

        public ResultApiServiceTests()
        {
            sentryClient = Substitute.For<ISentryClient>();
        }

        [Theory, AutoData]
        public async Task RetrieveEndRoundInfoAsync_Succeed_ReturnSGResponse(EndRoundInfoRequest request, EndRoundInfoResult result)
        {
            // Arrange
            var httpClient = MockHttpClient.Create(HttpStatusCode.OK, JsonConvert.SerializeObject(result));
            var service = new ResultApiService(httpClient, apiEndPoints, sentryClient);

            // Act
            var actualResult = await service.RetrieveEndRoundInfoAsync(request);

            // Assert
            Assert.IsType<SGResponse<EndRoundInfoResult>>(actualResult);
            Assert.NotNull(actualResult.Result);
        }

        [Theory, AutoData]
        public async Task RetrieveEndRoundInfoAsync_Failed_ReturnSGResponseAndLogtoSentry(EndRoundInfoRequest request)
        {
            // Arrange
            var httpClient = MockHttpClient.Create(HttpStatusCode.InternalServerError, JsonConvert.SerializeObject(string.Empty));
            var service = new ResultApiService(httpClient, apiEndPoints, sentryClient);

            // Act
            var actualResult = await service.RetrieveEndRoundInfoAsync(request);

            // Assert
            Assert.IsType<SGResponse<EndRoundInfoResult>>(actualResult);
            Assert.Equal(ErrorCodes.InternalError, actualResult.Result.ErrorCode);
        }
    }
}
