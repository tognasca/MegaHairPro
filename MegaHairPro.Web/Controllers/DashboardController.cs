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

            // ✅ Vendas do dia
            var vendasHoje = _context.Vendas
                .Where(v => v.DataVenda.Date == hoje)
                .ToList();

            ViewBag.TotalHoje = vendasHoje.Sum(x => x.PrecoVenda);
            ViewBag.LucroHoje = vendasHoje.Sum(x => x.LucroTotal);

            // ✅ Comissões pendentes
            ViewBag.ComissoesPendentes = _context.Comissoes
                .Where(c => c.Pago == false)
                .Sum(c => c.Valor);

            // ✅ Estoque baixo (menos que 100g)
            var estoqueBaixo = _context.Compras
                .GroupBy(c => c.ProdutoId)
                .Select(g => new
                {
                    ProdutoId = g.Key,
                    TotalGramas = g.Sum(x => x.QuantidadeGramas)
                })
                .Where(x => x.TotalGramas < 100)
                .Count();

            ViewBag.EstoqueBaixo = estoqueBaixo;

            return View();
        }
    }
}
