using MegaHairPro.Web.Data;
using MegaHairPro.Web.Models;
using MegaHairPro.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
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

        // ✅ Calendário principal
        [HttpGet]
        public IActionResult Index()
        {
            ViewBag.Profissionais = new SelectList(_context.Profissionais.OrderBy(p => p.Nome), "Id", "Nome");
            return View();
        }

        // ✅ Eventos para o FullCalendar (JSON)
        [HttpGet]
        public async Task<IActionResult> ListarEventos(int? profissionalId)
        {
            var query = _context.Agendamentos
                .Include(a => a.Profissional)
                .Include(a => a.Cliente)
                .Include(a => a.AgendamentosServicos).ThenInclude(s => s.Servico)
                .AsQueryable();

            if (profissionalId.HasValue && profissionalId > 0)
                query = query.Where(a => a.ProfissionalId == profissionalId.Value);

            var agendamentos = await query.ToListAsync();

            var eventos = agendamentos.Select(a => new
            {
                id = a.Id,
                title = $"{a.Cliente?.Nome} — {string.Join(", ", a.AgendamentosServicos.Select(s => s.Servico.Nome))}",
                start = a.DataHora.ToString("yyyy-MM-ddTHH:mm:ss"),
                end = a.DataFim.ToString("yyyy-MM-ddTHH:mm:ss"),
                color = a.ProfissionalId % 2 == 0 ? "#2563eb" : "#16a34a", // cor por profissional
                extendedProps = new
                {
                    profissional = a.Profissional?.Nome,
                    valorTotal = a.AgendamentosServicos.Sum(s => s.Servico.ValorFinal)
                }
            });

            return Json(eventos);
        }

        // ✅ Formulário de novo agendamento
        [HttpGet]
        public IActionResult Novo()
        {
            CarregarViewBags();
            return View();
        }

        // ✅ Salvar agendamento
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Novo(Agendamento model, int[] servicosSelecionados)
        {
            if (!servicosSelecionados.Any())
            {
                ModelState.AddModelError("", "Selecione ao menos um serviço.");
                CarregarViewBags();
                return View(model);
            }

            // Calcula DataFim somando duração de todos os serviços selecionados
            var servicos = _context.Servicos
                .Where(s => servicosSelecionados.Contains(s.Id))
                .ToList();

            var duracaoTotal = _context.Servicos.Where(s => servicosSelecionados.Contains(s.Id)).Sum(s => s.DuracaoMinutos);

            var dataFim = model.DataHora.AddMinutes(duracaoTotal);

            bool conflito = _context.Agendamentos.Any(a =>
                a.ProfissionalId == model.ProfissionalId &&
                a.DataHora < dataFim &&
                a.DataFim > model.DataHora);

            if (conflito)
            {
                ModelState.AddModelError("", "Esta profissional já possui agendamento neste horário.");
                CarregarViewBags();
                return View(model);
            }

            await _agendamentoService.CriarAgendamentoAsync(model, servicosSelecionados);

            TempData["Sucesso"] = "Agendamento criado com sucesso!";
            return RedirectToAction(nameof(Index));
        }

        // ✅ Painel do cliente
        [HttpGet]
        public async Task<IActionResult> PainelCliente(int id)
        {
            var cliente = await _context.Clientes.FindAsync(id);
            if (cliente == null) return NotFound();

            ViewBag.ClienteNome = cliente.Nome;

            var agendamentos = await _context.Agendamentos
                .Include(a => a.Profissional)
                .Include(a => a.AgendamentosServicos).ThenInclude(s => s.Servico)
                .Where(a => a.ClienteId == id)
                .OrderByDescending(a => a.DataHora)
                .ToListAsync();

            return View(agendamentos);
        }

        // ✅ Cancelar agendamento via AJAX
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancelar(int id)
        {
            var agendamento = await _context.Agendamentos.FindAsync(id);
            if (agendamento == null) return Json(new { success = false });

            // Remove os serviços vinculados primeiro
            var servicos = _context.AgendamentoServicos
                .Where(a => a.AgendamentoId == id);
            _context.AgendamentoServicos.RemoveRange(servicos);

            _context.Agendamentos.Remove(agendamento);
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }
        // ✅ Detalhes via AJAX (tooltip no calendário)
        [HttpGet]
        public async Task<IActionResult> Detalhes(int id)
        {
            var a = await _context.Agendamentos
                .Include(x => x.Cliente)
                .Include(x => x.Profissional)
                .Include(x => x.AgendamentosServicos).ThenInclude(s => s.Servico)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (a == null) return NotFound();

            return Json(new
            {
                cliente = a.Cliente?.Nome,
                profissional = a.Profissional?.Nome,
                dataHora = a.DataHora.ToString("dd/MM/yyyy HH:mm"),
                dataFim = a.DataFim.ToString("HH:mm"),
                servicos = a.AgendamentosServicos.Select(s => new
                {
                    nome = s.Servico.Nome,
                    valor = s.Servico.ValorFinal
                }),
                valorTotal = a.AgendamentosServicos.Sum(s => s.Servico.ValorFinal)
            });
        }

        private void CarregarViewBags()
        {
            ViewBag.Clientes = new SelectList(_context.Clientes.OrderBy(c => c.Nome), "Id", "Nome");
            ViewBag.Profissionais = new SelectList(_context.Profissionais.OrderBy(p => p.Nome), "Id", "Nome");
            ViewBag.Servicos = _context.Servicos.OrderBy(s => s.Nome).ToList();
        }

        // Adicione estes dois métodos no AgendamentoController

        // ✅ AJAX — Retorna horários disponíveis para uma profissional em uma data
        [HttpGet]
        public IActionResult HorariosDisponiveis(int profissionalId, string data)
        {
            if (!DateTime.TryParse(data, out var dataSelecionada))
                return BadRequest("Data inválida.");

            // Configurações do salão
            var inicio = TimeSpan.FromHours(8);   // 08:00
            var fim = TimeSpan.FromHours(20);  // 20:00
            var intervalo = TimeSpan.FromMinutes(30); // slots de 30 em 30 min

            // Gera todos os slots do dia
            var todosSlots = new List<DateTime>();
            var slot = dataSelecionada.Date + inicio;
            while (slot.TimeOfDay + intervalo <= fim)
            {
                todosSlots.Add(slot);
                slot = slot.Add(intervalo);
            }

            // Busca agendamentos da profissional nesse dia
            var agendamentosDia = _context.Agendamentos
                .Where(a => a.ProfissionalId == profissionalId &&
                            a.DataHora.Date == dataSelecionada.Date)
                .Select(a => new { a.DataHora, a.DataFim })
                .ToList();

            // Filtra slots que conflitam com agendamentos existentes
            var disponiveis = todosSlots
                .Where(s => !agendamentosDia.Any(a =>
                    s < a.DataFim && s.Add(intervalo) > a.DataHora))
                .Select(s => new
                {
                    valor = s.ToString("HH:mm"),
                    label = s.ToString("HH:mm")
                })
                .ToList();

            return Json(disponiveis);
        }

       
    }
}