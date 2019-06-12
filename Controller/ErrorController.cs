using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace HRCentral.Web.Controllers
{
    public class ErrorController : Controller
    {
        public IActionResult Index(int id)
        {
            var feature = HttpContext.Features.Get<IStatusCodeReExecuteFeature>();

            ViewData["StatusCode"] = id.ToString();

            return View();
        }
    }
}