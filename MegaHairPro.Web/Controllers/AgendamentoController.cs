using MegaHairPro.Web.Data;
using MegaHairPro.Web.Models;
using MegaHairPro.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace MegaHairPro.Web.Controllers
{
    public class AgendamentoController : Controller
    {
        private readonly AppDbContext _context;
        private readonly AgendamentoService _agendamentoService;

        public AgendamentoController(AppDbContext context, AgendamentoService agendamentoService)
        {
            _context = context;
            _agendamentoService = agendamentoService;
        }

        // Calendário principal
        [HttpGet]
        public IActionResult Index()
        {
            ViewBag.Profissionais = new SelectList(_context.Profissionais, "Id", "Nome");
            return View();
        }

        // Eventos para o calendário (JSON)
        [HttpGet]
        public async Task<IActionResult> ListarEventos(int? profissionalId)
        {
            var query = _context.Agendamentos
                .Include(a => a.Profissional)
                .Include(a => a.Cliente)
                .AsQueryable();

            if (profissionalId.HasValue && profissionalId > 0)
            {
                query = query.Where(a => a.ProfissionalId == profissionalId.Value);
            }

            var eventos = await query
                .Select(a => new
                {
                    id = a.Id,
                    title = a.Cliente.Nome + " - " + a.Servico.Nome,
                    start = a.DataHora,
                    end = a.DataFim
                })
                .ToListAsync();

            return Json(eventos);
        }

        public IActionResult CadastroAgendamento()
        {
            ViewBag.Clientes = new SelectList(_context.Clientes, "Id", "Nome");
            ViewBag.Profissionais = new SelectList(_context.Profissionais, "Id", "Nome");
            ViewBag.Servicos = _context.Servicos.ToList();

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CadastroAgendamento(Agendamento model, int[] servicosSelecionados)
        {
            await _agendamentoService.CriarAgendamentoAsync(model, servicosSelecionados);

            return RedirectToAction("PainelCliente", new { id = model.ClienteId });
        }

        public async Task<IActionResult> PainelCliente(int id)
        {
            var agendamentos = await _context.Agendamentos
                .Include(a => a.Profissional)
                .Include(a => a.AgendamentosServicos).ThenInclude(s => s.Servico)
                .Where(a => a.ClienteId == id)
                .ToListAsync();

            return View(agendamentos);
        }
    }
}
