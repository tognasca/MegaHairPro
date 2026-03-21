using MegaHairPro.Web.Data;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;

namespace MegaHairPro.Web.Controllers
{
    public class DashboardController : Controller
    {
        private readonly AppDbContext _context;

        public DashboardController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var hoje = DateTime.Today;
            var amanha = hoje.AddDays(1);

            var vendasHoje = _context.Vendas
                .Where(v => v.DataVenda >= hoje && v.DataVenda < amanha)
                .ToList();

            ViewBag.TotalHoje = vendasHoje.Sum(x => x.PrecoVenda);
            ViewBag.LucroHoje = vendasHoje.Sum(x => x.LucroTotal);

            ViewBag.ComissoesPendentes = _context.Comissoes
                .Where(c => !c.Pago)
                .Sum(c => (decimal?)c.Valor) ?? 0;

            ViewBag.EstoqueBaixo = _context.Compras
                .GroupBy(c => c.ProdutoId)
                .Select(g => new { Total = g.Sum(x => x.QuantidadeGramas) })
                .Count(x => x.Total < 100);

            return View();
        }

    }
}
