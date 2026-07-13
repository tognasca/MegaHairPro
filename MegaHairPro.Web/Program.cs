using DinkToPdf;
using DinkToPdf.Contracts;
using MegaHairPro.Web;
using MegaHairPro.Web.Data;
using MegaHairPro.Web.Infrastructure.Transactions;
using MegaHairPro.Web.Models;
using MegaHairPro.Web.Services;
using MegaHairPro.Web.Services.Implementations;
using MegaHairPro.Web.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Globalization;
using System.IO;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
    var policy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
    options.Filters.Add(new AuthorizeFilter(policy));
});

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
builder.Services.AddScoped<ICustoMedioService, CustoMedioService>();
builder.Services.AddScoped<IPdfService, PdfService>();
builder.Services.AddScoped<IViewRenderService, ViewRenderService>();
builder.Services.AddScoped<INotificationService, EmailNotificationService>();
builder.Services.AddScoped<AgendamentoService>();
builder.Services.AddScoped<AtendimentoService>();
builder.Services.AddScoped<BookingConfigService>();
builder.Services.AddScoped<PixBookingService>();
builder.Services.AddScoped<BookingPortalService>();

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.Cookie.Name = ".MegaHair.Booking";
    options.IdleTimeout = TimeSpan.FromDays(7);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddSingleton(typeof(IConverter),
    new SynchronizedConverter(new PdfTools()));

var wkhtmlPath = Path.Combine(Directory.GetCurrentDirectory(), "libwkhtmltox.dll");
if (File.Exists(wkhtmlPath))
{
    var context = new CustomAssemblyLoadContext();
    context.LoadUnmanagedLibrary(wkhtmlPath);
}
else
{
    Console.WriteLine("Aviso: libwkhtmltox.dll não encontrada — geração de PDF desabilitada.");
}

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

// ✅ LOGIN PATH
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AcessoNegado";
});


builder.Services.AddCors(options =>
{
    options.AddPolicy("React",
        policy =>
        {
            policy
                .AllowAnyOrigin()
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
});
var app = builder.Build();

if (app.Environment.IsDevelopment())
    app.UseDeveloperExceptionPage();
else
    app.UseExceptionHandler("/Home/Error");

// Cultura pt-BR para exibição e binding
var invariantCulture = new CultureInfo("pt-BR");
app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture(invariantCulture),
    SupportedCultures = new[] { invariantCulture },
    SupportedUICultures = new[] { invariantCulture }
});

app.UseStaticFiles();
app.UseRouting();
app.UseSession();

app.UseAuthentication();
app.UseCors("React");
app.UseAuthorization();

app.MapControllers();
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