using DocumentFormat.OpenXml.Math;
using MegaHairPro.Web.Data;
using MegaHairPro.Web.Models;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace MegaHairPro.Web.Controllers
{
    public class UsuarioController : Controller
    {
        private readonly AppDbContext _context;

        public UsuarioController(AppDbContext context)
        {
            _context = context;
        }

        // ✅ Lista
        public IActionResult Index()
        {
            var lista = _context.Usuarios.ToList();
            return View(lista);
        }

        // ✅ Novo
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // ✅ Salvar
        [HttpPost]
        public IActionResult Create(Usuario model)
        {
            model.SenhaHash = BCrypt.Net.BCrypt.HashPassword(model.SenhaHash);

            _context.Usuarios.Add(model);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }
    }
}
