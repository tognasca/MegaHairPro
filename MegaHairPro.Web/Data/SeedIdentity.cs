using MegaHairPro.Web.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System;
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

                string email = "admin@megahairpro.com";
                string senha = "Admin123!";

                var user = await userManager.FindByEmailAsync(email);

                if (user == null)
                {
                    user = new ApplicationUser
                    {
                        UserName = email,
                        Email = email,
                        Nome = "Tatiane (Admin)"
                    };

                    var result =
                        await userManager.CreateAsync(user, senha);

                    if (result.Succeeded)
                        await userManager.AddToRoleAsync(user, "Admin");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Erro ao criar admin: " + ex.Message);
            }
        }

    }
}
