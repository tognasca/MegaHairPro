using MegaHairPro.Web.Data;
using MegaHairPro.Web.Models;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
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

        // ✅ CREATE AJAX
        [HttpPost]
        public IActionResult CreateAjax(MetodoAplicacao metodo)
        {
            metodo.Status = true;

            // ValorPorGrama chega como string com ponto (enviado pelo JS com parseBR)
            // O model binding já deve ter feito o parse, mas garantimos via InvariantCulture
            if (Request.Form.TryGetValue("ValorPorGrama", out var valorStr))
            {
                if (decimal.TryParse(valorStr, NumberStyles.Any, CultureInfo.InvariantCulture, out var valor))
                    metodo.ValorPorGrama = valor;
            }

            _context.MetodosAplicacao.Add(metodo);
            _context.SaveChanges();

            return PartialView("_MetodoRow", metodo);
        }

        // ✅ EDIT AJAX
        [HttpPost]
        public IActionResult EditAjax(MetodoAplicacao metodo)
        {
            var original = _context.MetodosAplicacao.Find(metodo.Id);

            if (original == null)
                return Json(new { success = false });

            original.Nome = metodo.Nome;

            // Garante parse correto do valor
            if (Request.Form.TryGetValue("ValorPorGrama", out var valorStr))
            {
                if (decimal.TryParse(valorStr, NumberStyles.Any, CultureInfo.InvariantCulture, out var valor))
                    original.ValorPorGrama = valor;
            }

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

            return PartialView("_MetodoRow", metodo);
        }

        // ✅ DELETE
        [HttpPost]
        public IActionResult Delete(int id)
        {
            var metodo = _context.MetodosAplicacao.Find(id);

            if (metodo != null)
            {
                _context.MetodosAplicacao.Remove(metodo);
                _context.SaveChanges();
            }

            return Ok();
        }
    }
}