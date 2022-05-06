using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SG.Core.Tests.Shared
{
    public class MockHttpMessageHandler : HttpMessageHandler
    {
        private const string defaultMediaType = "application/json";
        private readonly HttpStatusCode statusCode;
        private readonly string content;

        public MockHttpMessageHandler(HttpStatusCode statusCode, string content)
        {
            this.statusCode = statusCode;
            this.content = content;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = new HttpResponseMessage(statusCode)
            {
                Content = new StringContent(content, Encoding.UTF8, defaultMediaType)
            };

            return Task.FromResult(response);
        }
    }
}