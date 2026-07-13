using MegaHairPro.Web.Data;
using MegaHairPro.Web.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.IO;

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
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(config.GetConnectionString("DefaultConnection")));
        services.AddScoped<AtendimentoService>();

        return services.BuildServiceProvider();
    }
}
