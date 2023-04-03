using Duende.IdentityServer.EntityFramework.DbContexts;
using Duende.IdentityServer.EntityFramework.Mappers;
using Microsoft.EntityFrameworkCore;

namespace CompanyEmployees.IDP.InitialSeed;

public static class MigrationManager
{
    public static WebApplication MigrateDatabase(this WebApplication app)
    {
        using(var scope = app.Services.CreateScope())
        {
            scope.ServiceProvider
                .GetRequiredService<PersistedGrantDbContext>()
                .Database
                .Migrate();

            using(var context = scope.ServiceProvider.GetService<ConfigurationDbContext>())
            {
                try
                {
                    context.Database.Migrate();

                    if(!context.Clients.Any())
                    {
                        foreach(var client in Config.Clients)
                        {
                            context.Clients.Add(client.ToEntity());
                        }

                        context.SaveChanges();
                    }

                    if (!context.IdentityResources.Any())
                    {
                        foreach(var resource in Config.IdentityResources)
                        {
                            context.IdentityResources.Add(resource.ToEntity());
                        }

                        context.SaveChanges();
                    }

                    if (!context.ApiScopes.Any())
                    {
                        foreach(var api in Config.ApiScopes)
                        {
                            context.ApiScopes.Add(api.ToEntity());
                        }

                        context.SaveChanges();
                    }

                    if (!context.ApiResources.Any())
                    {
                        foreach(var resource in Config.Apis)
                        {
                            context.ApiResources.Add(resource.ToEntity());
                        }

                        context.SaveChanges();
                    }
                }
                catch (Exception ex)
                {
                    // Log errors or do nothing
                    throw;
                }
            }
        }

        return app;
    }
}
