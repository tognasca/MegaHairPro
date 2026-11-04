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
            .AddJsonFile("appsettings.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = config.GetConnectionString("DefaultConnection");
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException(
                "Configure a variável ConnectionStrings__DefaultConnection antes de iniciar o MegaHairPro Desktop.");

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(config);

        services.AddDbContextFactory<AppDbContext>(options =>
            options.UseNpgsql(connectionString, npgsql =>
                npgsql.MigrationsAssembly("MegaHairPro.PostgreSqlMigrations")));

        services.AddSingleton<AtendimentoService>();
        return services.BuildServiceProvider();
    }
}