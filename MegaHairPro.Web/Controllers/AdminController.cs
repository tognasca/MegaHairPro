
using MegaHairPro.Web.Data;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace MegaHairPro.Web.Controllers
{
    public class AdminController : Controller
    {
        private readonly AppDbContext _context;

        public AdminController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Dashboard()
        {
            ViewBag.TotalClientes = _context.Clientes.Count();
            ViewBag.TotalProfissionais = _context.Profissionais.Count();
            ViewBag.TotalAgendamentos = _context.Agendamentos.Count();
            ViewBag.TotalFaturamento = _context.Agendamentos
                .SelectMany(a => a.AgendamentosServicos)
                .Sum(s => s.Servico.Valor);

            return View();
        }

        public IActionResult Calendario()
        {
            var eventos = _context.Agendamentos
                .Select(a => new
                {
                    title = a.Cliente.Nome,
                    start = a.DataHora
                }).ToList();

            return Json(eventos);
        }
    }
}