using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using HRCentral.Web.Models;

namespace HRCentral.Web.Controllers
{
    public class SplashController : Controller
    {
        private readonly ILogger<SplashController> _logger;
        /// <summary>
        /// The Constructor
        /// </summary>
        /// <param name="logger"></param>
        public SplashController(ILogger<SplashController> logger)
        {
            _logger = logger;
        }
        public IActionResult Index()
        {
            return View();
        }

        //public IActionResult Error()
        //{
        //    return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        //}

        
    }
}