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
    [Authorize(Roles = "Admin")]
    public class AdminUsuariosController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AdminUsuariosController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // ✅ LISTA
        public async Task<IActionResult> Index()
        {
            var usuarios = _userManager.Users.OrderBy(u => u.Nome).ToList();
            var lista    = new List<UsuarioAdminVM>();

            foreach (var u in usuarios)
            {
                var roles = await _userManager.GetRolesAsync(u);
                lista.Add(new UsuarioAdminVM
                {
                    Id     = u.Id,
                    Nome   = u.Nome,
                    Email  = u.Email,
                    Perfil = roles.FirstOrDefault() ?? "Sem Perfil",
                    Ativo  = u.Ativo
                });
            }

            ViewBag.Roles = _roleManager.Roles.OrderBy(r => r.Name).ToList();
            return View(lista);
        }

        // ✅ CRIAR via AJAX
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateAjax(string nome, string email, string senha, string role)
        {
            if (string.IsNullOrWhiteSpace(nome) || string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(senha) || string.IsNullOrWhiteSpace(role))
                return BadRequest("Todos os campos são obrigatórios.");

            var existente = await _userManager.FindByEmailAsync(email);
            if (existente != null)
                return BadRequest("E-mail já cadastrado.");

            var user = new ApplicationUser
            {
                Nome     = nome,
                UserName = email,
                Email    = email,
                Ativo    = true
            };

            var result = await _userManager.CreateAsync(user, senha);
            if (!result.Succeeded)
            {
                var erros = string.Join(", ", result.Errors.Select(e => e.Description));
                return BadRequest(erros);
            }

            await _userManager.AddToRoleAsync(user, role);

            var roles = await _userManager.GetRolesAsync(user);
            var vm    = new UsuarioAdminVM
            {
                Id     = user.Id,
                Nome   = user.Nome,
                Email  = user.Email,
                Perfil = roles.FirstOrDefault() ?? role,
                Ativo  = true
            };

            return PartialView("_UsuarioRow", vm);
        }

        // ✅ TOGGLE ATIVO/INATIVO via AJAX
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return Json(new { success = false });

            if (user.Email == User.Identity?.Name)
                return BadRequest("Você não pode bloquear seu próprio usuário.");

            user.Ativo = !user.Ativo;
            await _userManager.UpdateAsync(user);

            return Json(new { success = true, ativo = user.Ativo });
        }

        // ✅ RESETAR SENHA via AJAX
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetSenha(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return BadRequest("Usuário não encontrado.");

            if (user.Email == User.Identity?.Name)
                return BadRequest("Você não pode resetar sua própria senha.");

            string novaSenha = "Senha123!";
            var token  = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, novaSenha);

            if (!result.Succeeded)
                return BadRequest("Erro ao resetar senha.");

            return Json(new { success = true, novaSenha });
        }

        // ✅ EXCLUIR via AJAX
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return Ok();

            if (user.Email == User.Identity?.Name)
                return BadRequest("Você não pode excluir seu próprio usuário.");

            await _userManager.DeleteAsync(user);
            return Ok();
        }
    }
}
