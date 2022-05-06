using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using SG.Core.Features.Operators;

namespace SG.Api.Authentication
{
    public interface IOperatorRequestAuthentication
    {
        Task<bool> Verify(string operatorId, HttpRequest request);
    }

    public class OperatorRequestAuthentication : IOperatorRequestAuthentication
    {
        private readonly IOperatorDataService operatorDataService;

        public OperatorRequestAuthentication(IOperatorDataService operatorDataService)
        {
            this.operatorDataService = operatorDataService;
        }

        public async Task<bool> Verify(string operatorId, HttpRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Headers["Access-Key"]))
            {
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

                return accessKey == Convert.ToBase64String(hashResult);
            }

            return false;
        }
    }
}