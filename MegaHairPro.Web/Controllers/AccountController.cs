using MegaHairPro.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

namespace MegaHairPro.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser>  _userManager;

        public AccountController(
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser>  userManager)
        {
            _signInManager = signInManager;
            _userManager   = userManager;
        }

        // ── Login ─────────────────────────────────────────────────────────────
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "Dashboard");

            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string email, string senha, string? returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;

            var user = await _userManager.FindByEmailAsync(email);

            if (user == null)
            {
                ViewBag.Erro = "Usuário não encontrado.";
                return View();
            }

            if (!user.Ativo)
            {
                ViewBag.Erro = "Usuário bloqueado. Entre em contato com o administrador.";
                return View();
            }

            var result = await _signInManager.PasswordSignInAsync(
                user.UserName!, senha, isPersistent: true, lockoutOnFailure: false);

            if (!result.Succeeded)
            {
                ViewBag.Erro = "Senha incorreta.";
                return View();
            }

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Index", "Dashboard");
        }

        // ── Logout ────────────────────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login", "Account");
        }

        // ── Acesso negado ─────────────────────────────────────────────────────
        [AllowAnonymous]
        public IActionResult AcessoNegado() => View();

        // ── Minha Conta ───────────────────────────────────────────────────────
        [HttpGet]
        [Authorize]
        public IActionResult MinhaConta() => View();

        // ── Atualizar Nome ────────────────────────────────────────────────────
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AtualizarNome(string nome)
        {
            if (string.IsNullOrWhiteSpace(nome))
                return BadRequest("Nome não pode ser vazio.");

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            user.Nome = nome;
            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
                return BadRequest("Erro ao atualizar dados.");

            return Ok();
        }

        // ── Alterar Senha ─────────────────────────────────────────────────────
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AlterarSenha(string senhaAtual, string novaSenha)
        {
            if (string.IsNullOrWhiteSpace(senhaAtual) || string.IsNullOrWhiteSpace(novaSenha))
                return BadRequest("Preencha todos os campos.");

            if (novaSenha.Length < 6)
                return BadRequest("A nova senha deve ter no mínimo 6 caracteres.");

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var result = await _userManager.ChangePasswordAsync(user, senhaAtual, novaSenha);

            if (!result.Succeeded)
            {
                var erros = string.Join(" ", result.Errors.Select(e => e.Description));

                // Traduz erros comuns
                if (erros.Contains("Incorrect password") || erros.Contains("PasswordMismatch"))
                    return BadRequest("Senha atual incorreta.");

                return BadRequest(erros);
            }

            // Mantém o usuário logado após alterar a senha
            await _signInManager.RefreshSignInAsync(user);

            return Ok();
        }
    }
}
