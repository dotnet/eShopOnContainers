using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Entities;
using IdentityServer4.EntityFramework.Mappers;
using Microsoft.EntityFrameworkCore;
using Microsoft.eShopOnContainers.Services.Identity.API.Configuration;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.eShopOnContainers.Services.Identity.API.Data
{
    public class ConfigurationDbContextSeed
    {
        public async Task SeedAsync(ConfigurationDbContext context, IConfiguration configuration)
        {

            //callbacks urls from config:
            var clientUrls = new Dictionary<string, string>();

            clientUrls.Add("Mvc", configuration.GetValue<string>("MvcClient"));
            clientUrls.Add("Spa", configuration.GetValue<string>("SpaClient"));
            clientUrls.Add("Xamarin", configuration.GetValue<string>("XamarinCallback"));
            clientUrls.Add("BasketApi", configuration.GetValue<string>("BasketApiClient"));
            clientUrls.Add("OrderingApi", configuration.GetValue<string>("OrderingApiClient"));
            clientUrls.Add("MobileShoppingAgg", configuration.GetValue<string>("MobileShoppingAggClient"));
            clientUrls.Add("WebShoppingAgg", configuration.GetValue<string>("WebShoppingAggClient"));
            clientUrls.Add("WebhooksApi", configuration.GetValue<string>("WebhooksApiClient"));
            clientUrls.Add("WebhooksWeb", configuration.GetValue<string>("WebhooksWebClient"));

            if (!context.Clients.Any())
            {
                foreach (var client in Config.GetClients(clientUrls))
                {
                    context.Clients.Add(client.ToEntity());
                }
                await context.SaveChangesAsync();
            }
            // Checking always for old redirects to fix existing deployments
            // to use new swagger-ui redirect uri as of v3.0.0
            // There should be no problem for new ones
            // ref: https://github.com/dotnet-architecture/eShopOnContainers/issues/586
            else
            {
                List<ClientRedirectUri> oldRedirects = (await context.Clients.Include(c => c.RedirectUris).ToListAsync())
                    .SelectMany(c => c.RedirectUris)
                    .Where(ru => ru.RedirectUri.EndsWith("/o2c.html"))
                    .ToList();

                if (oldRedirects.Any())
                {
                    foreach (var ru in oldRedirects)
                    {
                        ru.RedirectUri = ru.RedirectUri.Replace("/o2c.html", "/oauth2-redirect.html");
                        context.Update(ru.Client);
                    }
                    await context.SaveChangesAsync();
                }
            }

            if (!context.IdentityResources.Any())
            {
                foreach (var resource in Config.GetResources())
                {
                    context.IdentityResources.Add(resource.ToEntity());
                }
                await context.SaveChangesAsync();
            }

            if (!context.ApiResources.Any())
            {
                foreach (var api in Config.GetApis())
                {
                    context.ApiResources.Add(api.ToEntity());
                }

                await context.SaveChangesAsync();
            }

            if (!context.ApiScopes.Any())
            {
                foreach (var apiScope in Config.GetApiScopes())
                {
                    context.ApiScopes.Add(apiScope.ToEntity());
                }

                await context.SaveChangesAsync();
            }
        }
    }
}
