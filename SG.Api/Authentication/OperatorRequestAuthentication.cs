using System;
using System.Collections;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Sentry;
using SG.Core.Features.Operators;

namespace SG.Api.Authentication
{
    public interface IOperatorRequestAuthentication
    {
        Task<bool> Verify(string operatorId, HttpRequest request, bool writeLog = false);
    }

    public class OperatorRequestAuthentication : IOperatorRequestAuthentication
    {
        private readonly IOperatorDataService operatorDataService;
        private readonly ISentryClient sentryClient;

        public OperatorRequestAuthentication(IOperatorDataService operatorDataService, ISentryClient sentryClient)
        {
            this.operatorDataService = operatorDataService;
            this.sentryClient = sentryClient;
        }

        public async Task<bool> Verify(string operatorId, HttpRequest request, bool writeLog = false)
        {
            if (string.IsNullOrWhiteSpace(request.Headers["Access-Key"]))
            {
                if (writeLog)
                {
                    this.sentryClient.CaptureMessage(JsonConvert.SerializeObject(new
                    {
                        operatorId,
                        message = "Missing access key"
                    }), Sentry.Protocol.SentryLevel.Warning);
                }
                return false;
            }

            var @operator = await operatorDataService.GetById(operatorId);

            if (@operator != null)
            {
                var secretKey = @operator.SecretKey;
                var accessKey = request.Headers["Access-Key"];

                request.Body.Seek(0, SeekOrigin.Begin);
                var streamData = new byte[request.Body.Length];
                _ = await request.Body.ReadAsync(streamData);

                var hmac = new HMACSHA256(Encoding.ASCII.GetBytes(secretKey));
                var hashResult = hmac.ComputeHash(streamData);

                var validAccessKey = accessKey == Convert.ToBase64String(hashResult);

                if (writeLog)
                {
                    var requestContent = Encoding.ASCII.GetString(streamData);
                    this.sentryClient.CaptureMessage(JsonConvert.SerializeObject(new
                    {
                        operatorId,
                        message = "validate access key",
                        accessKey,
                        requestHashedResult = Convert.ToBase64String(hashResult),
                        validAccessKey,
                        requestContent
                    }), Sentry.Protocol.SentryLevel.Warning);
                }

                return validAccessKey;
            }

            if (writeLog)
            {
                this.sentryClient.CaptureMessage(JsonConvert.SerializeObject(new
                {
                    operatorId,
                    message = "Cannot found operator"
                }), Sentry.Protocol.SentryLevel.Warning);
            }

            return false;
        }
    }
}