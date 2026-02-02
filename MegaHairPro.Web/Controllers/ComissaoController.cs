using MegaHairPro.Web.Data;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;

namespace MegaHairPro.Web.Controllers
{
    public class ComissaoController : Controller
    {
        private readonly AppDbContext _context;

        public ComissaoController(AppDbContext context)
        {
            _context = context;
        }

        // ✅ LISTA PENDENTES
        [HttpGet]
        public IActionResult Index()
        {
            var pendentes = _context.Comissoes
                .Where(c => c.Pago == false)
                .OrderByDescending(c => c.DataGeracao)
                .ToList();

            ViewBag.TotalPendente = pendentes.Sum(x => x.Valor);

            return View(pendentes);
        }

        // ✅ MARCAR COMO PAGO
        [HttpPost]
        public IActionResult Index(int id)
        {
            var comissao = _context.Comissoes.FirstOrDefault(x => x.Id == id);

            if (comissao == null)
                return NotFound();

            comissao.Pago = true;
            comissao.DataPagamento = DateTime.Now;

            _context.SaveChanges();

            return RedirectToAction("Index");
        }
    }
}
