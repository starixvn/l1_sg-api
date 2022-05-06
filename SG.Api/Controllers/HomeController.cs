using Microsoft.AspNetCore.Mvc;

namespace SG.Api.Controllers
{
    public class HomeController : ControllerBase
    {
        public ActionResult Index() => new ContentResult
        {
            Content = "SG Api is running!!!",
            ContentType = "text/html"
        };
    }
}