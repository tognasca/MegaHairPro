using Microsoft.AspNetCore.Identity;
using System;

namespace MegaHairPro.Web.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string Nome { get; set; } = "";
        public bool Ativo { get; set; } = true;
    }
}
