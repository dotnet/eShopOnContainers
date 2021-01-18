using Devspaces.Support;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.eShopOnContainers.Mobile.Shopping.HttpAggregator.Config;
using Microsoft.eShopOnContainers.Mobile.Shopping.HttpAggregator.Filters.Basket.API.Infrastructure.Filters;
using Microsoft.eShopOnContainers.Mobile.Shopping.HttpAggregator.Infrastructure;
using Microsoft.eShopOnContainers.Mobile.Shopping.HttpAggregator.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;

namespace Microsoft.eShopOnContainers.Mobile.Shopping.HttpAggregator
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
            services.AddHealthChecks()
                .AddCheck("self", () => HealthCheckResult.Healthy())
                .AddUrlGroup(new Uri(Configuration["CatalogUrlHC"]), name: "catalogapi-check", tags: new string[] { "catalogapi" })
                .AddUrlGroup(new Uri(Configuration["OrderingUrlHC"]), name: "orderingapi-check", tags: new string[] { "orderingapi" })
                .AddUrlGroup(new Uri(Configuration["BasketUrlHC"]), name: "basketapi-check", tags: new string[] { "basketapi" })
                .AddUrlGroup(new Uri(Configuration["IdentityUrlHC"]), name: "identityapi-check", tags: new string[] { "identityapi" })
                .AddUrlGroup(new Uri(Configuration["MarketingUrlHC"]), name: "marketingapi-check", tags: new string[] { "marketingapi" })
                .AddUrlGroup(new Uri(Configuration["PaymentUrlHC"]), name: "paymentapi-check", tags: new string[] { "paymentapi" })
                .AddUrlGroup(new Uri(Configuration["LocationUrlHC"]), name: "locationapi-check", tags: new string[] { "locationapi" });

            services.AddCustomMvc(Configuration)
                 .AddCustomAuthentication(Configuration)
                 .AddDevspaces()
                 .AddHttpServices();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {
            var pathBase = Configuration["PATH_BASE"];

            if (!string.IsNullOrEmpty(pathBase))
            {
                loggerFactory.CreateLogger<Startup>().LogDebug("Using PATH BASE '{pathBase}'", pathBase);
                app.UsePathBase(pathBase);
            }

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseSwagger().UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint($"{ (!string.IsNullOrEmpty(pathBase) ? pathBase : string.Empty) }/swagger/v1/swagger.json", "Purchase BFF V1");

                c.OAuthClientId("mobileshoppingaggswaggerui");
                c.OAuthClientSecret(string.Empty);
                c.OAuthRealm(string.Empty);
                c.OAuthAppName("Purchase BFF Swagger UI");
            });

            app.UseRouting();
            app.UseCors("CorsPolicy");
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapDefaultControllerRoute();
                endpoints.MapControllers();
                endpoints.MapHealthChecks("/hc", new HealthCheckOptions()
                {
                    Predicate = _ => true,
                    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
                });
                endpoints.MapHealthChecks("/liveness", new HealthCheckOptions
                {
                    Predicate = r => r.Name.Contains("self")
                });
            });
        }
    }

    public static class ServiceCollectionExtensions
    {
        [Obsolete]
        public static IServiceCollection AddCustomMvc(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddOptions();
            services.Configure<UrlsConfig>(configuration.GetSection("urls"));

            services.AddControllers()
                .AddNewtonsoftJson();

            services.AddSwaggerGen(options =>
            {
                options.DescribeAllEnumsAsStrings();
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Shopping Aggregator for Mobile Clients",
                    Version = "v1",
                    Description = "Shopping Aggregator for Mobile Clients"
                });
                options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.OAuth2,
                    Flows = new OpenApiOAuthFlows()
                    {
                        Implicit = new OpenApiOAuthFlow()
                        {
                            AuthorizationUrl = new Uri($"{configuration.GetValue<string>("IdentityUrlExternal")}/connect/authorize"),
                            TokenUrl = new Uri($"{configuration.GetValue<string>("IdentityUrlExternal")}/connect/token"),

                            Scopes = new Dictionary<string, string>()
                            {
                                { "mobileshoppingagg", "Shopping Aggregator for Mobile Clients" }
                            }
                        }
                    }
                });

                options.OperationFilter<AuthorizeCheckOperationFilter>();
            });

            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy",
                    builder => builder
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .SetIsOriginAllowed((host) => true)
                    .AllowCredentials());
            });

            return services;
        }
        public static IServiceCollection AddCustomAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Remove("sub");

            var identityUrl = configuration.GetValue<string>("urls:identity");

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;

            })
            .AddJwtBearer(options =>
            {
                options.Authority = identityUrl;
                options.RequireHttpsMetadata = false;
                options.Audience = "mobileshoppingagg";
            });

            return services;
        }

        public static IServiceCollection AddHttpServices(this IServiceCollection services)
        {
            //register delegating handlers
            services.AddTransient<HttpClientAuthorizationDelegatingHandler>();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            //register http services
            services
                .AddHttpClient<IBasketService, BasketService>()
                .AddHttpMessageHandler<HttpClientAuthorizationDelegatingHandler>()
                .AddDevspacesSupport();

            services.AddHttpClient<ICatalogService, CatalogService>()
                   .AddDevspacesSupport();

            services.AddHttpClient<IOrderApiClient, OrderApiClient>()
                   .AddDevspacesSupport();

            services.AddHttpClient<IOrderingService, OrderingService>()
                   .AddDevspacesSupport();

            return services;
        }

    }
}
