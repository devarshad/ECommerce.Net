using ECommerce.Net.MVC.UI.Models;
using Microsoft.CodeAnalysis.Options;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace ECommerce.Net.MVC.UI.Services
{
    public class ForecastService : IForecastService
    {
        private readonly HttpClient httpClient;
        private readonly IOptions<AppSettings> settings;

        public ForecastService(HttpClient httpClient, IOptions<AppSettings> settings)
        {
            this.httpClient = httpClient;
            this.settings = settings;
        }
        public async Task<string> Get()
        {
            return await httpClient.GetStringAsync("");
        }
    }
}
