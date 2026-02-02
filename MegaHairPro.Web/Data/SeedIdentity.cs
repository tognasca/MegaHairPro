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
            var userManager = service.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = service.GetRequiredService<RoleManager<IdentityRole>>();

            string role = "Admin";

            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));

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

                await userManager.CreateAsync(user, senha);
                await userManager.AddToRoleAsync(user, role);
            }
        }
    }
}
