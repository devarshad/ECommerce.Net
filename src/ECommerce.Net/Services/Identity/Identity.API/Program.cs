using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ECommerce.Net.Identity.API.Data;
using ECommerce.Net.Identity.API.Extensions;
using IdentityServer4.EntityFramework.DbContexts;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;

namespace ECommerce.Net.Identity.API
{
    public class Program
    {
        public static int Main(string[] args)
        {
            var configuration = GetConfiguration();
            try
            {
                var host = BuildWebHost(configuration, args);
                host.MigrateDbContext<PersistedGrantDbContext>((_, __) => { })
                    .MigrateDbContext<ApplicationDbContext>((context, services) =>
                    {
                        var env = services.GetService<IWebHostEnvironment>();
                        var logger = services.GetService<ILogger<ApplicationDbContextSeed>>();
                        var settings = services.GetService<IOptions<AppSettings>>();

                        new ApplicationDbContextSeed()
                            .SeedAsync(context, env, logger, settings)
                            .Wait();
                    })
                    .MigrateDbContext<ConfigurationDbContext>((context, services) =>
                    {
                        new ConfigurationDbContextSeed()
                            .SeedAsync(context, configuration)
                            .Wait();
                    });
                host.Run();
                return 0;
            }
            catch (Exception ex)
            {
                return 1;
            }
            finally
            {
            }
        }

        public static IWebHost BuildWebHost(IConfiguration configuration, string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .UseConfiguration(configuration)
                .Build();

        private static IConfiguration GetConfiguration()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddEnvironmentVariables();

            var config = builder.Build();
            return builder.Build();
        }
    }
}
