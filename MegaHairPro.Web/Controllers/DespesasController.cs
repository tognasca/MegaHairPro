using MegaHairPro.Web.Data;
using MegaHairPro.Web.Models;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace MegaHairPro.Web.Controllers
{
    public class DespesasController : Controller
    {
        private readonly AppDbContext _context;
        int mediaVendasMes = 50; // você define
        public DespesasController(AppDbContext context)
        {
            _context = context;
        }

        // ✅ LISTAGEM
        [HttpGet]
        public IActionResult Index()
        {
            var despesas = _context.Despesas
                .OrderByDescending(d => d.DataCadastro)
                .ToList();

            return View(despesas);
        }

        // ✅ CREATE
        [HttpPost]
        ////[ValidateAntiForgeryToken]
        public IActionResult Index(Despesas despesa)
        {
            if (!ModelState.IsValid)
                return RedirectToAction("Index");

            despesa.Status = true;

            _context.Despesas.Add(despesa);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        // ✅ EDITAR VIA AJAX (igual Compras)
        [HttpPost]
        public IActionResult EditAjax(Despesas despesa)
        {
            var original = _context.Despesas.Find(despesa.Id);

            if (original == null)
                return Json(new { success = false });


            original.Categoria = despesa.Categoria;
            original.Descricao = despesa.Descricao;
            original.Valor = despesa.Valor;
            original.DataCadastro = despesa.DataCadastro;

            _context.SaveChanges();

            return PartialView("_DespesaRow", original);
        }

        // ✅ ATIVAR / INATIVAR
        [HttpPost]
        public IActionResult ToggleStatusAjax(int id)
        {
            var despesa = _context.Despesas.Find(id);

            if (despesa == null)
                return Json(new { success = false });

            despesa.Status = !despesa.Status;
            _context.SaveChanges();

            return Json(new { success = true, ativo = despesa.Status });
        }

        // ✅ EXCLUIR
        [HttpPost]
        public IActionResult Delete(int id)
        {
            var despesa = _context.Despesas.Find(id);

            if (despesa == null)
                return RedirectToAction("Index");

            _context.Despesas.Remove(despesa);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }
    }
}
