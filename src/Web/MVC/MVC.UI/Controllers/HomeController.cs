using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.InteropServices;
using System.Security.Claims;
using System.Threading.Tasks;
using ECommerce.Net.MVC.UI;
using ECommerce.Net.MVC.UI.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using MVC.UI.Models;
using Newtonsoft.Json.Linq;

namespace MVC.UI.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IForecastService forecastService;
        private readonly IDistributedCache cache;

        public IHttpContextAccessor HttpContextAccessor { get; }

        public HomeController(ILogger<HomeController> logger, IForecastService forecastService, IDistributedCache cache)
        {
            _logger = logger;
            this.forecastService = forecastService;
            this.cache = cache;
        }
        [ResponseCache(CacheProfileName = "Default")]
        public ActionResult Index()
        {
            var cacheEntry = cache.GetString("AccessToken");
            return View();
        }

        [Authorize(AuthenticationSchemes = OpenIdConnectDefaults.AuthenticationScheme)]
        public async Task<IActionResult> PrivacyAsync()
        {
            var user = User as ClaimsPrincipal;
            ViewBag.UserName = user?.FindFirstValue("name");
            var cacheEntry = cache.GetString("AccessToken");
            if (cacheEntry==null)
            {
                cacheEntry = await forecastService.Get();
                cache.SetString("AccessToken", cacheEntry);
            }
            ViewBag.Access_Token = cacheEntry;
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
