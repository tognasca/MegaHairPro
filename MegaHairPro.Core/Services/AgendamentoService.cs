using MegaHairPro.Web.Data;
using MegaHairPro.Web.Models;
using MegaHairPro.Web.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MegaHairPro.Web.Services
{
    public class AgendamentoService
    {
        private readonly AppDbContext _context;
        private readonly INotificationService _notifier;

        public AgendamentoService(AppDbContext context, INotificationService notifier)
        {
            _context  = context;
            _notifier = notifier;
        }

        public async Task<Agendamento> CriarAgendamentoAsync(Agendamento model, int[] servicosSelecionados)
        {
            // ServicoId é FK obrigatória no model — usa o primeiro serviço selecionado
            if (servicosSelecionados.Any())
                model.ServicoId = servicosSelecionados[0];

            // Calcula DataFim com duração total de todos os serviços
            var servicos = _context.Servicos
                .Where(s => servicosSelecionados.Contains(s.Id))
                .ToList();

            int duracaoTotal = servicos.Sum(s => s.DuracaoMinutos);
            model.DataFim    = model.DataHora.AddMinutes(duracaoTotal > 0 ? duracaoTotal : 60);

            _context.Agendamentos.Add(model);
            await _context.SaveChangesAsync();

            // Cria os vínculos N:N com todos os serviços
            foreach (var id in servicosSelecionados)
            {
                _context.AgendamentoServicos.Add(new AgendamentoServico
                {
                    AgendamentoId = model.Id,
                    ServicoId     = id
                });
            }

            await _context.SaveChangesAsync();

            await EnviarNotificacaoConfirmacaoAsync(model.Id);

            return model;
        }

        public async Task EnviarNotificacaoConfirmacaoAsync(int agendamentoId)
        {
            var agendamento = await _context.Agendamentos
                .Include(a => a.Cliente)
                .Include(a => a.Profissional)
                .Include(a => a.AgendamentosServicos).ThenInclude(s => s.Servico)
                .FirstOrDefaultAsync(a => a.Id == agendamentoId);

            if (agendamento?.Cliente == null ||
                string.IsNullOrWhiteSpace(agendamento.Cliente.Email))
                return;

            var cliente    = agendamento.Cliente;
            var dataTexto  = agendamento.DataHora.ToString("dd/MM/yyyy HH:mm");
            var servicos   = string.Join(", ", agendamento.AgendamentosServicos.Select(s => s.Servico.Nome));

            var mensagem = $@"
                <p>Olá, {cliente.Nome}!</p>
                <p>Seu agendamento foi registrado:</p>
                <p>
                    <strong>Data/Hora:</strong> {dataTexto}<br/>
                    <strong>Profissional:</strong> {agendamento.Profissional?.Nome}<br/>
                    <strong>Serviços:</strong> {servicos}
                </p>
                <p>Qualquer dúvida, entre em contato com o salão.</p>";

            await _notifier.EnviarEmailAsync(cliente.Email, "Confirmação de agendamento", mensagem);
        }
    }
}
