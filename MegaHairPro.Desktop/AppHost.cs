using MegaHairPro.Web.Data;
using MegaHairPro.Web.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MegaHairPro.Desktop;

public static class AppHost
{
    public static IServiceProvider Configure()
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false)
            .Build();

        var services = new ServiceCollection();

        services.AddSingleton<IConfiguration>(config);

        // AddDbContextFactory registers IDbContextFactory<AppDbContext>, which lets
        // AtendimentoService create a short-lived context per operation instead of
        // sharing one long-lived context across every call (which caused the
        // "second operation started on this context" exception).
        services.AddDbContextFactory<AppDbContext>(options =>
            options.UseSqlServer(
                config.GetConnectionString("DefaultConnection"),
                sql => sql.MigrationsAssembly("MegaHairPro.Web")));

        // Registered as Singleton (not Scoped): this app never calls CreateScope(),
        // so a "Scoped" service resolved from the root provider already behaves like
        // a singleton in practice. Making it explicit avoids relying on that implicit
        // behavior. This is safe now because AtendimentoService no longer holds a
        // single shared DbContext - it pulls a fresh one from the factory per call.
        services.AddSingleton<AtendimentoService>();

        return services.BuildServiceProvider();
    }
}