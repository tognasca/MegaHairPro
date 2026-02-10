using MegaHairPro.Web.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace MegaHairPro.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;

        // ✅ ÚNICO CONSTRUTOR
        public AccountController(
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        ////[ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(
            string email,
            string senha,
            string? returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;

            var user = await _userManager.FindByEmailAsync(email);

            if (user == null)
            {
                ViewBag.Erro = "Usuário não encontrado.";
                return View();
            }

            var result = await _signInManager.PasswordSignInAsync(
                user.UserName,
                senha,
                true,
                false);

            if (!result.Succeeded)
            {
                ViewBag.Erro = "Senha incorreta.";
                return View();
            }

            if (!string.IsNullOrEmpty(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Index", "Dashboard");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login", "Account");
        }
        public IActionResult AcessoNegado()
        {
            return View();
        }
    }
}
