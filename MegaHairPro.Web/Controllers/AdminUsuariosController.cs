using MegaHairPro.Web.Models;
using MegaHairPro.Web.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MegaHairPro.Web.Controllers
{
    //[Authorize(Roles = "Admin")]
    public class AdminUsuariosController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        //✅ ÚNICO CONSTRUTOR
        public AdminUsuariosController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // ✅ LISTA USUÁRIOS COM PERFIL
        public async Task<IActionResult> Index()
        {
            var usuarios = _userManager.Users.ToList();

            var lista = new List<UsuarioAdminVM>();

            foreach (var u in usuarios)
            {
                var roles = await _userManager.GetRolesAsync(u);

                lista.Add(new UsuarioAdminVM
                {
                    Id = u.Id,
                    Nome = u.Nome,
                    Email = u.Email,
                    Perfil = roles.FirstOrDefault() ?? "Sem Perfil",
                    Ativo = u.Ativo
                });
            }

            return View(lista);
        }

        // ✅ FORM CREATE
        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.Roles = _roleManager.Roles.ToList();
            return View();
        }

        // ✅ CRIAR USUÁRIO
        [HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string nome, string email, string senha, string role)
        {
            ViewBag.Roles = _roleManager.Roles.ToList();

            var user = new ApplicationUser
            {
                Nome = nome,
                UserName = email,
                Email = email,
                Ativo = true
            };

            var result = await _userManager.CreateAsync(user, senha);

            if (!result.Succeeded)
            {
                ViewBag.Erros = result.Errors;
                return View();
            }

            // ✅ Vincular Role
            await _userManager.AddToRoleAsync(user, role);

            return RedirectToAction("Index");
        }

        // ✅ BLOQUEAR / DESBLOQUEAR
        [HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            
            if (user.Email == User.Identity.Name)
            {
                TempData["Msg"] = "Você não pode bloquear seu próprio usuário.";
                return RedirectToAction("Index");
            }
            user.Ativo = !user.Ativo;
            await _userManager.UpdateAsync(user);

            return RedirectToAction("Index");
        }

        // ✅ RESETAR SENHA
        [HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetSenha(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound("Usuário não encontrado.");
            if (user.Email == User.Identity.Name)
            {
                TempData["Msg"] = "Você não pode resetar a senha do seu próprio usuário.";
                return RedirectToAction("Index");
            }
            

            string novaSenha = "Senha123!";

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            var result = await _userManager.ResetPasswordAsync(user, token, novaSenha);

            if (!result.Succeeded)
            {
                TempData["Msg"] = "❌ Erro ao resetar senha.";
                return RedirectToAction("Index");
            }

            TempData["Msg"] = $"✅ Senha resetada! Nova senha: {novaSenha}";

            return RedirectToAction("Index");
        }
    }
}
