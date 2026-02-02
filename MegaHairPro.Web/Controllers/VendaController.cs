using MegaHairPro.Web.Data;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;

namespace MegaHairPro.Web.Controllers
{
    public class VendaController : Controller
    {
        private readonly AppDbContext _context;

        public VendaController(AppDbContext context)
        {
            _context = context;
        }

        // ✅ LISTAR VENDAS
        [HttpGet]
        public IActionResult Index(int? mes, int? ano)
        {
            mes ??= DateTime.Now.Month;
            ano ??= DateTime.Now.Year;

            var vendas = _context.Vendas
                .Where(v => v.DataVenda.Month == mes && v.DataVenda.Year == ano)
                .OrderByDescending(v => v.DataVenda)
                .ToList();

            // ✅ Totais do mês
            ViewBag.TotalVendas = vendas.Sum(x => x.PrecoVenda);
            ViewBag.TotalLucro = vendas.Sum(x => x.LucroTotal);
            ViewBag.TotalComissoes =
                vendas.Sum(x => x.ValorComissaoVendedora + x.ValorComissaoCabeleireira);

            ViewBag.Mes = mes;
            ViewBag.Ano = ano;

            return View(vendas);
        }
    }
}
