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

        [HttpGet]
        public IActionResult Index()
        {
            ViewBag.Profissionais = new SelectList(
                _context.Profissionais.OrderBy(p => p.Nome), "Id", "Nome");
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> ListarEventos(int? profissionalId)
        {
            var query = _context.Agendamentos
                .Include(a => a.Profissional)
                .Include(a => a.Cliente)
                .Include(a => a.AgendamentosServicos).ThenInclude(s => s.Servico)
                .Where(a => a.Status != StatusAgendamento.Cancelado)
                .AsQueryable();

            if (profissionalId.HasValue && profissionalId > 0)
                query = query.Where(a => a.ProfissionalId == profissionalId.Value);

            var agendamentos = await query.ToListAsync();

            static string CorStatus(StatusAgendamento s) => s switch
            {
                StatusAgendamento.Confirmado => "#16a34a",
                StatusAgendamento.Concluido => "#2563eb",
                _ => "#f59e0b"
            };

            var eventos = agendamentos.Select(a => new
            {
                id = a.Id,
                title = $"{a.Cliente?.Nome} — {string.Join(", ", a.AgendamentosServicos.Select(s => s.Servico.Nome))}",
                start = a.DataHora.ToString("yyyy-MM-ddTHH:mm:ss"),
                end = a.DataFim.ToString("yyyy-MM-ddTHH:mm:ss"),
                color = CorStatus(a.Status),
                extendedProps = new
                {
                    profissional = a.Profissional?.Nome,
                    status = a.Status.ToString(),
                    sinalPago = a.SinalPago,
                    valorSinal = a.ValorSinal,
                    valorRestante = a.ValorRestante,
                    valorTotal = a.AgendamentosServicos.Sum(s => s.Servico.ValorFinal)
                }
            });

            return Json(eventos);
        }

        [HttpGet]
        public IActionResult Novo()
        {
            CarregarViewBags();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Novo(Agendamento model, int[] servicosSelecionados,
            decimal percentualSinal)
        {
            if (!servicosSelecionados.Any())
            {
                ModelState.AddModelError("", "Selecione ao menos um serviço.");
                CarregarViewBags();
                return View(model);
            }

            var servicos = _context.Servicos.Where(s => servicosSelecionados.Contains(s.Id)).ToList();
            int duracaoTotal = servicos.Sum(s => s.DuracaoMinutos);
            model.DataFim = model.DataHora.AddMinutes(duracaoTotal > 0 ? duracaoTotal : 60);

            bool conflito = _context.Agendamentos.Any(a =>
                a.ProfissionalId == model.ProfissionalId &&
                a.Status != StatusAgendamento.Cancelado &&
                a.DataHora < model.DataFim && a.DataFim > model.DataHora);

            if (conflito)
            {
                ModelState.AddModelError("", "Esta profissional já possui agendamento neste horário.");
                CarregarViewBags();
                return View(model);
            }

            decimal valorTotal = servicos.Sum(s => s.ValorFinal);
            decimal percSinal = Math.Clamp(percentualSinal, 10, 50);
            model.PercentualSinal = percSinal;
            model.ValorSinal = Math.Round(valorTotal * (percSinal / 100m), 2);
            model.ValorRestante = Math.Round(valorTotal - model.ValorSinal, 2);
            model.Status = StatusAgendamento.Agendado;

            await _agendamentoService.CriarAgendamentoAsync(model, servicosSelecionados);

            TempData["Sucesso"] = "Agendamento criado!";
            return RedirectToAction(nameof(Index));
        }

        // ✅ Registrar sinal pago
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegistrarSinal(int id)
        {
            var ag = await _context.Agendamentos.FindAsync(id);
            if (ag == null) return Json(new { success = false, msg = "Não encontrado." });
            if (ag.SinalPago) return Json(new { success = false, msg = "Sinal já registrado." });

            ag.SinalPago = true;
            ag.DataPagamentoSinal = DateTime.Now;
            ag.Status = StatusAgendamento.Confirmado;

            _context.ContasFinanceiras.Add(new ContaFinanceira
            {
                Tipo = TipoContaFinanceira.Receber,
                Origem = OrigemContaFinanceira.Agendamento,
                Descricao = $"Sinal — Agendamento #{ag.Id} ({ag.DataHora:dd/MM/yyyy HH:mm})",
                Valor = ag.ValorSinal,
                DataCompetencia = DateTime.Now,
                DataVencimento = DateTime.Now,
                Status = StatusContaFinanceira.Paga,
                DataPagamento = DateTime.Now,
                AgendamentoId = ag.Id
            });

            await _context.SaveChangesAsync();

            return Json(new
            {
                success = true,
                msg = $"Sinal de R$ {ag.ValorSinal:N2} registrado!",
                valorRestante = ag.ValorRestante.ToString("N2")
            });
        }

        // ✅ Concluir atendimento e receber restante
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Concluir(int id)
        {
            var ag = await _context.Agendamentos
                .Include(a => a.AgendamentosServicos).ThenInclude(s => s.Servico)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (ag == null)
                return Json(new { success = false, msg = "Não encontrado." });
            if (ag.Status == StatusAgendamento.Concluido)
                return Json(new { success = false, msg = "Já concluído." });
            if (!ag.SinalPago)
                return Json(new { success = false, msg = "Registre o sinal antes de concluir." });

            ag.RestantePago = true;
            ag.DataConclusao = DateTime.Now;
            ag.Status = StatusAgendamento.Concluido;

            _context.ContasFinanceiras.Add(new ContaFinanceira
            {
                Tipo = TipoContaFinanceira.Receber,
                Origem = OrigemContaFinanceira.Agendamento,
                Descricao = $"Restante — Agendamento #{ag.Id} ({ag.DataHora:dd/MM/yyyy HH:mm})",
                Valor = ag.ValorRestante,
                DataCompetencia = DateTime.Now,
                DataVencimento = DateTime.Now,
                Status = StatusContaFinanceira.Paga,
                DataPagamento = DateTime.Now,
                AgendamentoId = ag.Id
            });

            await _context.SaveChangesAsync();

            return Json(new
            {
                success = true,
                msg = $"Concluído! R$ {ag.ValorRestante:N2} recebido."
            });
        }

        // ✅ Cancelar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancelar(int id)
        {
            var ag = await _context.Agendamentos.FindAsync(id);
            if (ag == null) return Json(new { success = false });

            _context.AgendamentoServicos.RemoveRange(
                _context.AgendamentoServicos.Where(a => a.AgendamentoId == id));

            ag.Status = StatusAgendamento.Cancelado;
            ag.Cancelado = true;
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }

        // ✅ Detalhes (AJAX modal)
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
                id = a.Id,
                cliente = a.Cliente?.Nome,
                profissional = a.Profissional?.Nome,
                dataHora = a.DataHora.ToString("dd/MM/yyyy HH:mm"),
                dataFim = a.DataFim.ToString("HH:mm"),
                status = a.Status.ToString(),
                sinalPago = a.SinalPago,
                valorSinal = a.ValorSinal,
                valorRestante = a.ValorRestante,
                restantePago = a.RestantePago,
                servicos = a.AgendamentosServicos.Select(s => new
                {
                    nome = s.Servico.Nome,
                    valor = s.Servico.ValorFinal
                }),
                valorTotal = a.AgendamentosServicos.Sum(s => s.Servico.ValorFinal)
            });
        }

        // ✅ Horários disponíveis
        [HttpGet]
        public IActionResult HorariosDisponiveis(int profissionalId, string data)
        {
            if (!DateTime.TryParse(data, out var dataSelecionada))
                return BadRequest("Data inválida.");

            var inicio = TimeSpan.FromHours(8);
            var fim = TimeSpan.FromHours(20);
            var intervalo = TimeSpan.FromMinutes(30);

            var todosSlots = new List<DateTime>();
            var slot = dataSelecionada.Date + inicio;
            while (slot.TimeOfDay + intervalo <= fim)
            {
                todosSlots.Add(slot);
                slot = slot.Add(intervalo);
            }

            var agendamentosDia = _context.Agendamentos
                .Where(a => a.ProfissionalId == profissionalId &&
                            a.DataHora.Date == dataSelecionada.Date &&
                            a.Status != StatusAgendamento.Cancelado)
                .Select(a => new { a.DataHora, a.DataFim })
                .ToList();

            var disponiveis = todosSlots
                .Where(s => !agendamentosDia.Any(a =>
                    s < a.DataFim && s.Add(intervalo) > a.DataHora))
                .Select(s => new { valor = s.ToString("HH:mm"), label = s.ToString("HH:mm") })
                .ToList();

            return Json(disponiveis);
        }

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

        private void CarregarViewBags()
        {
            ViewBag.Clientes = new SelectList(_context.Clientes.OrderBy(c => c.Nome), "Id", "Nome");
            ViewBag.Profissionais = new SelectList(_context.Profissionais.OrderBy(p => p.Nome), "Id", "Nome");
            ViewBag.Servicos = _context.Servicos.OrderBy(s => s.Nome).ToList();
        }
    }
}