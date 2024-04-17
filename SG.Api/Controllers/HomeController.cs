using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace SG.Api.Controllers
{
    public class HomeController : ControllerBase
    {
        private readonly IConfiguration configuration;

        public HomeController(IConfiguration configuration) {
            this.configuration = configuration;
        }

        public ActionResult Index() => new ContentResult
        {
            Content = $"SG Api is running!!! - {configuration["Env"]} - {configuration["SystemType"]}",
            ContentType = "text/html"
        };
    }
}