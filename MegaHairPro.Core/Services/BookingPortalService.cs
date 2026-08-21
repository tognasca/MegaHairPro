using MegaHairPro.Web.Data;
using MegaHairPro.Web.Models;
using MegaHairPro.Web.Models.Booking;
using MegaHairPro.Web.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MegaHairPro.Web.Services;

public class BookingPortalService
{
    private readonly AppDbContext _context;
    private readonly BookingConfigService _config;
    private readonly PixBookingService _pix;
    private readonly INotificationService _notifier;
    private readonly IConfiguration _configuration;
    private static readonly TimeZoneInfo TzSp = TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time");

    public BookingPortalService(
        AppDbContext context,
        BookingConfigService config,
        PixBookingService pix,
        INotificationService notifier,
        IConfiguration configuration)
    {
        _context = context;
        _config = config;
        _pix = pix;
        _notifier = notifier;
        _configuration = configuration;
    }

    public static string OnlyDigits(string? s) =>
        string.IsNullOrEmpty(s) ? "" : new string(s.Where(char.IsDigit).ToArray());

    public async Task<(bool ok, string? erro, int? clienteId)> CadastrarAsync(CadastroClienteRequest req)
    {
        var wa = OnlyDigits(req.WhatsApp ?? req.Telefone);
        var email = req.Email?.Trim() ?? "";

        var existe = await _context.Clientes.AnyAsync(c =>
            c.Status && (c.WhatsApp == wa || (!string.IsNullOrEmpty(email) && c.Email == email)));
        if (existe) return (false, "Já existe cadastro com este WhatsApp ou e-mail.", null);

        var cliente = new Cliente
        {
            Nome = req.Nome,
            Telefone = OnlyDigits(req.Telefone),
            WhatsApp = wa,
            Email = email,
            CPF = req.Cpf ?? "",
            DataNascimento = req.DataNascimento,
            Status = true
        };
        _context.Clientes.Add(cliente);
        await _context.SaveChangesAsync();

        if (!string.IsNullOrWhiteSpace(req.Senha))
        {
            await EnsureUsuarioClienteTableAsync();
            _context.Set<UsuarioCliente>().Add(new UsuarioCliente
            {
                ClienteId = cliente.Id,
                Senha = BCrypt.Net.BCrypt.HashPassword(req.Senha),
                Ativo = true
            });
            await _context.SaveChangesAsync();
        }

        return (true, null, cliente.Id);
    }

    public async Task<(bool ok, string? erro, Cliente? cliente)> LoginAsync(LoginClienteRequest req)
    {
        var id = req.WhatsApp != null ? OnlyDigits(req.WhatsApp) : req.Email?.Trim() ?? "";
        var cliente = await _context.Clientes
            .FirstOrDefaultAsync(c => c.Status && (c.WhatsApp == id || c.Email == id));
        if (cliente == null) return (false, "Cliente não encontrado. Por favor, se cadastre.", null);

        var usuario = await _context.Set<UsuarioCliente>()
            .FirstOrDefaultAsync(u => u.ClienteId == cliente.Id && u.Ativo);

        if (usuario != null && !string.IsNullOrEmpty(usuario.Senha))
        {
            if (string.IsNullOrWhiteSpace(req.Senha))
                return (false, "Informe sua senha para entrar.", null);
            if (!BCrypt.Net.BCrypt.Verify(req.Senha, usuario.Senha))
                return (false, "Senha incorreta.", null);
        }

        return (true, null, cliente);
    }

    public async Task<List<SlotHorarioDto>> HorariosDisponiveisAsync(int profissionalId, string data, int duracaoMin)
    {
        if (!DateTime.TryParse(data, out var dia)) return new List<SlotHorarioDto>();

        var ano = dia.Year;
        var mes = dia.Month;
        var d = dia.Day;

        var inicioDiaUtc = new DateTime(ano, mes, d, 12, 0, 0, DateTimeKind.Utc);
        var fimDiaUtc = new DateTime(ano, mes, d + 1, 2, 0, 0, DateTimeKind.Utc);
        var janelaIni = new DateTime(ano, mes, d, 3, 0, 0, DateTimeKind.Utc);
        var janelaFim = new DateTime(ano, mes, d + 1, 2, 59, 59, DateTimeKind.Utc);

        var ocupados = await _context.Agendamentos
            .Where(a => a.ProfissionalId == profissionalId &&
                        a.DataHora >= janelaIni && a.DataHora <= janelaFim &&
                        !a.Cancelado &&
                        (a.Status == StatusAgendamento.Agendado || a.Status == StatusAgendamento.Confirmado))
            .Select(a => new { a.DataHora, a.DataFim })
            .ToListAsync();

        var agoraUtc = DateTime.UtcNow;
        var agoraSp = TimeZoneInfo.ConvertTimeFromUtc(agoraUtc, TzSp);
        var ehHoje = agoraSp.Year == ano && agoraSp.Month == mes && agoraSp.Day == d;

        var inicioReal = inicioDiaUtc;
        if (ehHoje && agoraUtc > inicioDiaUtc)
            inicioReal = ArredondarIntervalo(agoraUtc, 15);

        var slots = new List<SlotHorarioDto>();
        var cursor = inicioReal;
        const int intervalo = 30;

        while (cursor < fimDiaUtc)
        {
            var fimSlot = cursor.AddMinutes(duracaoMin);
            if (fimSlot > fimDiaUtc) break;

            var conflita = ocupados.Any(o => cursor < o.DataFim && fimSlot > o.DataHora);
            if (!conflita)
            {
                var local = TimeZoneInfo.ConvertTimeFromUtc(cursor, TzSp);
                slots.Add(new SlotHorarioDto
                {
                    Inicio = cursor.ToString("o"),
                    FimPrevisto = fimSlot.ToString("o"),
                    Hora = local.ToString("HH:mm")
                });
            }
            cursor = cursor.AddMinutes(intervalo);
        }

        return slots;
    }

    public async Task<(bool ok, object? data, string? erro, int status)> CriarAgendamentoAsync(
        int clienteId, CriarAgendamentoRequest req)
    {
        var servico = await _context.Servicos.FindAsync(req.ServicoId);
        if (servico == null) return (false, null, "Serviço não encontrado.", 404);

        var prof = await _context.Profissionais.FindAsync(req.ProfissionalId);
        if (prof == null) return (false, null, "Profissional não encontrado.", 404);

        var cliente = await _context.Clientes.FindAsync(clienteId);
        if (cliente == null) return (false, null, "Cliente não encontrado.", 404);

        var cfg = await _config.GetAsync();
        var valorTotal = req.ValorTotalCustom ?? servico.ValorFinal;
        var duracao = req.DuracaoTotalCustom ?? servico.DuracaoMinutos;
        var perc = cfg.PercentualSinal;
        var valorSinal = Math.Round(valorTotal * perc / 100m, 2);
        var valorRestante = Math.Round(valorTotal - valorSinal, 2);

        var dataInicio = DateTime.SpecifyKind(req.DataHora, DateTimeKind.Utc);
        if (req.DataHora.Kind == DateTimeKind.Unspecified)
            dataInicio = req.DataHora;
        var dataFim = dataInicio.AddMinutes(duracao);

        var conflito = await _context.Agendamentos.AnyAsync(a =>
            a.ProfissionalId == req.ProfissionalId && !a.Cancelado &&
            (a.Status == StatusAgendamento.Agendado || a.Status == StatusAgendamento.Confirmado) &&
            dataInicio < a.DataFim && dataFim > a.DataHora);

        if (conflito) return (false, null, "Esse horário acabou de ser reservado por outro cliente.", 409);

        var ag = new Agendamento
        {
            DataHora = dataInicio,
            DataFim = dataFim,
            ClienteId = clienteId,
            ProfissionalId = req.ProfissionalId,
            ServicoId = req.ServicoId,
            Status = StatusAgendamento.Agendado,
            Cancelado = false,
            PercentualSinal = perc,
            ValorSinal = valorSinal,
            ValorRestante = valorRestante,
            SinalPago = false
        };

        _context.Agendamentos.Add(ag);
        await _context.SaveChangesAsync();

        var ids = new List<int> { req.ServicoId };
        if (req.ServicosAdicionais?.Length > 0) ids.AddRange(req.ServicosAdicionais);
        foreach (var sid in ids.Distinct())
        {
            _context.AgendamentoServicos.Add(new AgendamentoServico
            {
                AgendamentoId = ag.Id,
                ServicoId = sid
            });
        }
        await _context.SaveChangesAsync();

        var nomeServico = req.NomeServicosCompleto ?? servico.Nome;
        var txid = $"MH{ag.Id}{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}".Substring(0, 25);
        var pagamento = _pix.CriarPagamento(valorSinal, $"Sinal - {nomeServico}", txid);

        await NotificarSalaoNovoAsync(cliente, ag, nomeServico, valorSinal);

        return (true, new
        {
            ok = true,
            agendamentoId = ag.Id,
            pagamento,
            servico = new { nome = nomeServico, valor = valorTotal },
            valorSinal,
            valorRestante,
            percentualSinal = perc,
            dataHora = dataInicio,
            expiraEm = DateTime.UtcNow.AddMinutes(10)
        }, null, 200);
    }

    public async Task<bool> AprovarPagamentoAsync(int id)
    {
        var ag = await _context.Agendamentos
            .Include(a => a.Cliente)
            .Include(a => a.Profissional)
            .Include(a => a.Servico)
            .Include(a => a.AgendamentosServicos).ThenInclude(x => x.Servico)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (ag == null) return false;

        ag.Status = StatusAgendamento.Confirmado;
        ag.SinalPago = true;
        ag.DataPagamentoSinal = DateTime.Now;
        await _context.SaveChangesAsync();

        var servicos = string.Join(" + ", ag.AgendamentosServicos.Select(s => s.Servico.Nome));
        if (string.IsNullOrEmpty(servicos)) servicos = ag.Servico?.Nome ?? "";

        if (!string.IsNullOrWhiteSpace(ag.Cliente?.Email))
        {
            var dataFmt = ag.DataHora.ToString("dddd, dd 'de' MMMM", new System.Globalization.CultureInfo("pt-BR"));
            await _notifier.EnviarEmailAsync(ag.Cliente.Email, "Agendamento Confirmado - Tati Mega Hair",
                $"<p>Olá, {ag.Cliente.Nome}! Seu horário foi confirmado para {dataFmt} às {ag.DataHora:HH:mm}.</p>" +
                $"<p>Serviço: {servicos}<br/>Profissional: {ag.Profissional?.Nome}</p>");
        }

        return true;
    }

    public async Task InformarPagamentoAsync(int agendamentoId, int clienteId)
    {
        var ag = await _context.Agendamentos
            .Include(a => a.Cliente)
            .Include(a => a.Servico)
            .FirstOrDefaultAsync(a => a.Id == agendamentoId && a.ClienteId == clienteId);
        if (ag == null || ag.Cancelado || ag.SinalPago) return;

        var appUrl = _configuration["Booking:AppUrl"] ?? "";
        var token = _configuration["Booking:AdminToken"] ?? "";
        var salaoEmail = _configuration["Booking:SalaoEmail"] ?? _configuration["Email:Remetente"];
        var msg = $"Sinal informado — aprovar agendamento #{ag.Id}. Cliente: {ag.Cliente?.Nome}. " +
                  $"Link: {appUrl}/admin/aprovar/{ag.Id}?token={token}";

        if (!string.IsNullOrWhiteSpace(salaoEmail))
            await _notifier.EnviarEmailAsync(salaoEmail, $"Sinal aguardando aprovação — {ag.Cliente?.Nome}", $"<p>{msg}</p>");
    }

    private async Task NotificarSalaoNovoAsync(Cliente cliente, Agendamento ag, string servico, decimal valorSinal)
    {
        var salaoEmail = _configuration["Booking:SalaoEmail"] ?? _configuration["Email:Remetente"];
        var appUrl = _configuration["Booking:AppUrl"] ?? "";
        var token = _configuration["Booking:AdminToken"] ?? "";
        if (string.IsNullOrWhiteSpace(salaoEmail)) return;

        await _notifier.EnviarEmailAsync(salaoEmail, $"Novo agendamento — {cliente.Nome}",
            $"<p>Novo agendamento pendente de PIX.</p>" +
            $"<p>Cliente: {cliente.Nome}<br/>Serviço: {servico}<br/>Sinal: R$ {valorSinal:N2}</p>" +
            $"<p><a href=\"{appUrl}/admin/aprovar/{ag.Id}?token={token}\">Aprovar</a></p>");
    }

    private static DateTime ArredondarIntervalo(DateTime dt, int minutos)
    {
        var ms = 60000L * minutos;
        return new DateTime((long)Math.Ceiling(dt.Ticks / (double)ms) * ms, dt.Kind);
    }

    public async Task<(bool ok, string? msg, string? erro, int code)> CancelarClienteAsync(int id, int clienteId)
    {
        var ag = await _context.Agendamentos.FirstOrDefaultAsync(a => a.Id == id && a.ClienteId == clienteId);
        if (ag == null) return (false, null, "Agendamento não encontrado.", 404);
        if (ag.Cancelado) return (false, null, "Agendamento já está cancelado.", 403);
        if (ag.Status == StatusAgendamento.Cancelado)
            return (false, null, "Este agendamento não pode mais ser cancelado.", 403);

        if (!await _config.PodeAlterarAsync(ag.DataHora))
        {
            var cfg = await _config.GetAsync();
            return (false, null,
                $"Não é possível cancelar: o prazo de {cfg.PrazoAlteracaoHoras}h antes do horário já passou.",
                403);
        }

        ag.Cancelado = true;
        ag.Status = StatusAgendamento.Cancelado;
        await _context.SaveChangesAsync();

        var aviso = ag.SinalPago ? " O sinal não será reembolsado conforme política do salão." : "";
        return (true, $"Agendamento cancelado com sucesso.{aviso}", null, 200);
    }

    public async Task<(bool ok, object? data, string? erro, int code)> ReagendarAsync(int id, int clienteId, ReagendarRequest req)
    {
        var ag = await _context.Agendamentos
            .Include(a => a.Servico)
            .FirstOrDefaultAsync(a => a.Id == id && a.ClienteId == clienteId);
        if (ag == null) return (false, null, "Agendamento não encontrado.", 404);
        if (ag.Cancelado) return (false, null, "Agendamento cancelado.", 403);
        if (ag.Status != StatusAgendamento.Agendado && ag.Status != StatusAgendamento.Confirmado)
            return (false, null, "Este agendamento não pode ser reagendado.", 403);

        if (!await _config.PodeAlterarAsync(ag.DataHora))
        {
            var cfg = await _config.GetAsync();
            return (false, null,
                $"Não é possível alterar o horário: faltam menos de {cfg.PrazoAlteracaoHoras}h para o atendimento.",
                403);
        }

        var pid = req.ProfissionalId ?? ag.ProfissionalId;
        var duracao = (ag.DataFim - ag.DataHora).TotalMinutes;
        if (duracao <= 0) duracao = ag.Servico?.DuracaoMinutos ?? 60;
        var inicio = req.DataHora;
        var fim = inicio.AddMinutes(duracao);

        var conflito = await _context.Agendamentos.AnyAsync(a =>
            a.ProfissionalId == pid && !a.Cancelado && a.Id != id &&
            (a.Status == StatusAgendamento.Agendado || a.Status == StatusAgendamento.Confirmado) &&
            inicio < a.DataFim && fim > a.DataHora);
        if (conflito) return (false, null, "O novo horário não está disponível.", 409);

        ag.DataHora = inicio;
        ag.DataFim = fim;
        ag.ProfissionalId = pid;
        await _context.SaveChangesAsync();

        return (true, new { ok = true, mensagem = "Horário alterado com sucesso.", dataHora = inicio }, null, 200);
    }

    public bool ValidarAdminToken(string? token) =>
        !string.IsNullOrEmpty(token) &&
        token == (_configuration["Booking:AdminToken"] ?? "");

    private async Task EnsureUsuarioClienteTableAsync()
    {
        await _context.Database.ExecuteSqlRawAsync(@"
            IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='UsuariosClientes' AND xtype='U')
            CREATE TABLE [dbo].[UsuariosClientes] (
                [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                [ClienteId] INT NOT NULL,
                [Senha] NVARCHAR(MAX) NULL,
                [DataCadastro] DATETIME2(7) NOT NULL DEFAULT GETDATE(),
                [Ativo] BIT NOT NULL DEFAULT 1
            )");
    }
}
