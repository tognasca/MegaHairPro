using Microsoft.AspNetCore.Mvc;
using MegaHairPro.Web.Data;
using System.Linq;

namespace MegaHairPro.Web.Controllers;
public class AccountController : Controller
{
    private readonly AppDbContext _context;
    public AccountController(AppDbContext context) => _context = context;

    [HttpGet]
    public IActionResult Login() => View();
    
    [HttpPost]
    public IActionResult Login(string login, string senha)
    {
        if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(senha))
        {
            ModelState.AddModelError("", "Login e senha são obrigatórios.");
            return View();
        }
        
        var user = _context.Usuarios.FirstOrDefault(u => u.Login == login && u.Senha == senha);
        if (user == null)
        {
            ModelState.AddModelError("", "Credenciais inválidas.");
            return View();
        }
        
        return RedirectToAction("Index", "Produtos");
    }
}
