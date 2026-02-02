using MegaHairPro.Web.Data;
using MegaHairPro.Web.Models;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace MegaHairPro.Web.Controllers
{
    public class MetodosAplicacaoController : Controller
    {
        private readonly AppDbContext _context;

        public MetodosAplicacaoController(AppDbContext context)
        {
            _context = context;
        }

        // ✅ LISTAGEM
        [HttpGet]
        public IActionResult Index()
        {
            var metodos = _context.MetodosAplicacao
                .OrderByDescending(m => m.Id)
                .ToList();

            return View(metodos);
        }

        // ✅ CREATE
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(MetodoAplicacao metodo)
        {
            metodo.Status = true;

            _context.MetodosAplicacao.Add(metodo);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        // ✅ EDIT AJAX
        [HttpPost]
        public IActionResult EditAjax(MetodoAplicacao metodo)
        {
            var original = _context.MetodosAplicacao.Find(metodo.Id);

            if (original == null)
                return Json(new { success = false });

            original.Nome = metodo.Nome;
            original.Descricao = metodo.Descricao;

            _context.SaveChanges();

            return PartialView("_MetodoRow", original);
        }

        // ✅ ATIVAR / INATIVAR
        [HttpPost]
        public IActionResult ToggleStatusAjax(int id)
        {
            var metodo = _context.MetodosAplicacao.Find(id);

            if (metodo == null)
                return Json(new { success = false });

            metodo.Status = !metodo.Status;
            _context.SaveChanges();

            return Json(new { success = true });
        }

        // ✅ DELETE
        [HttpPost]
        public IActionResult Delete(int id)
        {
            var metodo = _context.MetodosAplicacao.Find(id);

            if (metodo == null)
                return RedirectToAction("Index");

            _context.MetodosAplicacao.Remove(metodo);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }
    }
}
