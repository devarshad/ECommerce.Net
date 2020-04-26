using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Threading.Tasks;
using ECommerce.Net.MVC.UI.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MVC.UI.Models;
using Newtonsoft.Json.Linq;

namespace MVC.UI.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IForecastService forecastService;

        public HomeController(ILogger<HomeController> logger, IForecastService forecastService)
        {
            _logger = logger;
            this.forecastService = forecastService;
        }

        public async Task<IActionResult> Index()
        {
            
            return View();
        }

        [Authorize(AuthenticationSchemes = OpenIdConnectDefaults.AuthenticationScheme)]
        public async Task<IActionResult> PrivacyAsync()
        {
            var user = User as ClaimsPrincipal;
            ViewBag.UserName = user?.FindFirstValue("name");
            ViewBag.Access_Token = await forecastService.Get();
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
