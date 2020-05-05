using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Forecast.API.Infrastructure.Filters;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;

namespace Forecast.API
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "ECommerce.Net - Forecast HTTP API",
                    Version = "v1",
                    Description = "The Forecast Service HTTP API"
                });

                options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.OAuth2,
                    Flows = new OpenApiOAuthFlows()
                    {
                        Implicit = new OpenApiOAuthFlow()
                        {
                            AuthorizationUrl = new Uri($"{Configuration.GetValue<string>("IdentityUrl")}connect/authorize"),
                            TokenUrl = new Uri($"{Configuration.GetValue<string>("IdentityUrl")}connect/token"),
                            Scopes = new Dictionary<string, string>()
                            {
                                { "forecast", "Forecast API" }
                            }
                        }
                    }
                });

                options.OperationFilter<AuthorizeCheckOperationFilter>();
            });

            services.AddAuthentication("Bearer")
                .AddJwtBearer("Bearer", options =>
                {
                    options.Authority = Configuration["IdentityUrl"];
                    options.RequireHttpsMetadata = false;

                    options.Audience = "forecast";
                });
            services.AddApplicationInsightsTelemetry(Configuration);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            var pathBase = Configuration["PATH_BASE"];
            app.UseSwagger()
              .UseSwaggerUI(setup =>
              {
                  setup.SwaggerEndpoint($"{ (!string.IsNullOrEmpty(pathBase) ? pathBase : string.Empty) }/swagger/v1/swagger.json", "Forecast.API V1");
                  setup.OAuthClientId("forecastswaggerui");
                  setup.OAuthAppName("Forecast Swagger UI");
              });

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
