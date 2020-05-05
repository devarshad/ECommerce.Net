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
using Serilog;

namespace ECommerce.Net.Identity.API
{
    public class Program
    {
        public static readonly string Namespace = typeof(Program).Namespace;
        public static readonly string AppName = Namespace.Substring(Namespace.LastIndexOf('.', Namespace.LastIndexOf('.') - 1) + 1);

        public static void Main(string[] args)
        {
            var configuration = GetConfiguration();
            //Removing SeriLogs as Using Application insight for logging
            //Log.Logger = CreateSerilogLogger(configuration);

            var host = BuildWebHost(configuration, args);
            var logger = host.Services.GetRequiredService<ILogger<Program>>();

            logger.LogInformation("Applying migrations ({ApplicationContext})...", AppName);
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

            logger.LogInformation("Starting web host ({ApplicationContext})...", AppName);
            host.Run();

        }

        public static IWebHost BuildWebHost(IConfiguration configuration, string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .UseConfiguration(configuration)
                .ConfigureLogging(
                    builder =>
                    {
                        // Providing an instrumentation key here is required if you're using
                        // standalone package Microsoft.Extensions.Logging.ApplicationInsights
                        // or if you want to capture logs from early in the application startup 
                        // pipeline from Startup.cs or Program.cs itself.
                        builder.AddApplicationInsights(configuration["ApplicationInsights:InstrumentationKey"]);

                        // Adding the filter below to ensure logs of all severity from Startup.cs
                        // is sent to ApplicationInsights.
                        //   builder.AddFilter<Microsoft.Extensions.Logging.ApplicationInsights.ApplicationInsightsLoggerProvider>
                        //                  (typeof(Startup).FullName, LogLevel.Trace);
                    }
                )
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
        private static Serilog.ILogger CreateSerilogLogger(IConfiguration configuration)
        {
            var seqServerUrl = configuration["Serilog:SeqServerUrl"];
            var logstashUrl = configuration["Serilog:LogstashgUrl"];
            return new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .ReadFrom.Configuration(configuration)
                .Enrich.WithProperty("ApplicationContext", AppName)
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .WriteTo.Seq(string.IsNullOrWhiteSpace(seqServerUrl) ? "http://seq" : seqServerUrl)
                .WriteTo.Http(string.IsNullOrWhiteSpace(logstashUrl) ? "http://localhost:8080" : logstashUrl)
                .WriteTo.AzureBlobStorage(configuration["StorageConnectionString"], Serilog.Events.LogEventLevel.Verbose, "logs", null, "{yyyy}/{MM}/{dd}/log.txt")
                .CreateLogger();
        }
    }
}
