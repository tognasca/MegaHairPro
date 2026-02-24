using MegaHairPro.Web.Data;
using MegaHairPro.Web.Models;
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

        public AgendamentoController(AppDbContext context)
        {
            _context = context;
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
            _context.Agendamentos.Add(model);
            await _context.SaveChangesAsync();

            foreach (var id in servicosSelecionados)
            {
                _context.AgendamentoServicos.Add(new AgendamentoServico
                {
                    AgendamentoId = model.Id,
                    ServicoId = id
                });
            }

            await _context.SaveChangesAsync();

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
