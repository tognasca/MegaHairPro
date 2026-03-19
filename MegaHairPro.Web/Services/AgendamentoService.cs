using MegaHairPro.Web.Data;
using MegaHairPro.Web.Models;
using MegaHairPro.Web.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MegaHairPro.Web.Services;

public class AgendamentoService
{
    private readonly AppDbContext _context;
    private readonly INotificationService _notifier;

    public AgendamentoService(AppDbContext context, INotificationService notifier)
    {
        _context = context;
        _notifier = notifier;
    }

    public async Task<Agendamento> CriarAgendamentoAsync(Agendamento model, int[] servicosSelecionados)
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

        if (agendamento == null)
            return;

        var cliente = agendamento.Cliente;
        if (cliente == null || string.IsNullOrWhiteSpace(cliente.Email))
            return;

        var dataTexto = agendamento.DataHora.ToString("dd/MM/yyyy HH:mm");
        var servicos = string.Join(", ", agendamento.AgendamentosServicos.Select(s => s.Servico.Nome));

        var assunto = "Confirmação de agendamento";
        var mensagem = $@"
            <p>Olá, {cliente.Nome}!</p>
            <p>Seu agendamento foi registrado:</p>
            <p><strong>Data/Hora:</strong> {dataTexto}<br/>
               <strong>Profissional:</strong> {agendamento.Profissional.Nome}<br/>
               <strong>Serviços:</strong> {servicos}</p>
            <p>Qualquer dúvida, entre em contato com o salão.</p>";

        await _notifier.EnviarEmailAsync(cliente.Email, assunto, mensagem);
    }
}

