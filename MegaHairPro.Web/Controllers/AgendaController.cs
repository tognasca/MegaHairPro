
using MegaHairPro.Web.Data;
using MegaHairPro.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
namespace MegaHairPro.Web.Controllers
{
    public class AgendaController : Controller
    {
        private readonly AppDbContext _context;

        public AgendaController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var agendamentos = _context.Agendamentos
                .Include(a => a.Cliente)
                .Include(a => a.Profissional)
                .Include(a => a.Servico)
                .OrderBy(a => a.DataHora)
                .ToList();

            return View(agendamentos);
        }

        [HttpPost]
        public IActionResult Criar(int clienteId, int servicoId, int profissionalId, DateTime data)
        {
            var servico = _context.Servicos.Find(servicoId);

            var agendamento = new Agendamento
            {
                ClienteId = clienteId,
                ServicoId = servicoId,
                ProfissionalId = profissionalId,
                DataHora = data,
                DataFim = data.AddMinutes(servico.DuracaoMinutos)
            };

            _context.Add(agendamento);
            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }
    }
}