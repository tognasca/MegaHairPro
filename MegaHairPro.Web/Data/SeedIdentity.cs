using MegaHairPro.Web.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MegaHairPro.Web.Data
{
    public static class SeedIdentity
    {
        public static async Task CriarAdmin(IServiceProvider service)
        {
            try
            {
                var userManager =
                    service.GetRequiredService<UserManager<ApplicationUser>>();

                var roleManager =
                    service.GetRequiredService<RoleManager<IdentityRole>>();

                string[] roles =
                    { "Admin", "Vendedora", "Cabeleireira" };

                foreach (var r in roles)
                {
                    if (!await roleManager.RoleExistsAsync(r))
                        await roleManager.CreateAsync(new IdentityRole(r));
                }

                var configuration = service.GetRequiredService<IConfiguration>();
                var email = configuration["BootstrapAdmin:Email"] ?? "admin@megahairpro.com";
                var senha = configuration["BootstrapAdmin:Password"];

                var user = await userManager.FindByEmailAsync(email);

                if (user == null)
                {
                    if (string.IsNullOrWhiteSpace(senha))
                    {
                        Console.WriteLine(
                            "Admin inicial não criado: configure BootstrapAdmin__Password por variável de ambiente.");
                        return;
                    }

                    user = new ApplicationUser
                    {
                        UserName = email,
                        Email = email,
                        Nome = "Tatiane (Admin)"
                    };

                    var result = await userManager.CreateAsync(user, senha);

                    if (result.Succeeded)
                        await userManager.AddToRoleAsync(user, "Admin");
                    else
                        Console.WriteLine("Falha ao criar admin inicial: " +
                            string.Join("; ", result.Errors.Select(e => e.Description)));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Erro ao criar admin: " + ex.Message);
            }
        }

    }
}
