using MegaHairPro.Web.Data;
using MegaHairPro.Web.Infrastructure.Transactions;
using MegaHairPro.Web.Models;
using MegaHairPro.Web.Services;
using MegaHairPro.Web.Services.Implementations;
using MegaHairPro.Web.Services.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();


// ✅ BANCO DE DADOS
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")
    )
);


// ✅ SERVICES
builder.Services.AddScoped<ISimulacaoService, SimulacaoService>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IVendaService, VendaService>();


// ✅ IDENTITY COMPLETO
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;

    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();


// ✅ LOGIN PATH (IMPORTANTE)
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AcessoNegado";
});


var app = builder.Build();

var cultureInfo = new CultureInfo("pt-BR");

app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture(cultureInfo),
    SupportedCultures = new[] { cultureInfo },
    SupportedUICultures = new[] { cultureInfo }
});

app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}"
);

// SEED COM TRY-CATCH
using (var scope = app.Services.CreateScope())
{
    try
    {
        await SeedIdentity.CriarAdmin(scope.ServiceProvider);
    }
    catch (Exception ex)
    {
        Console.WriteLine("Erro no Seed: " + ex.Message);
    }
}

app.Run();

