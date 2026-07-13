using MegaHairPro.Web.Data;
using MegaHairPro.Web.Models;
using MegaHairPro.Web.Models.Booking;
using MegaHairPro.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MegaHairPro.Web.Controllers;

/// <summary>
/// Portal de agendamento online (migrado do bookingController.js do Node).
/// Rotas compatíveis com o front em wwwroot/booking.
/// </summary>
[AllowAnonymous]
[IgnoreAntiforgeryToken]
public class BookingPortalController : Controller
{
    private const string SessionClienteId = "BookingClienteId";
    private const string SessionClienteNome = "BookingClienteNome";

    private readonly BookingPortalService _portal;
    private readonly BookingConfigService _config;
    private readonly AppDbContext _context;
    private readonly IWebHostEnvironment _env;

    public BookingPortalController(
        BookingPortalService portal,
        BookingConfigService config,
        AppDbContext context,
        IWebHostEnvironment env)
    {
        _portal = portal;
        _config = config;
        _context = context;
        _env = env;
    }

    private int? ClienteIdSessao => HttpContext.Session.GetInt32(SessionClienteId);

    private IActionResult ServirPagina(string arquivo)
    {
        var path = Path.Combine(_env.WebRootPath, arquivo);
        if (!System.IO.File.Exists(path))
            return NotFound("Portal de agendamento não configurado. Copie os arquivos PWA para wwwroot.");
        return PhysicalFile(path, "text/html");
    }

    // ─── Páginas PWA ─────────────────────────────────────────────────────────
    [HttpGet("/")]
    public IActionResult PortalHome() => ServirPagina("index.html");

    [HttpGet("/agendar")]
    public IActionResult PortalAgendar() => ServirPagina("agendar.html");

    [HttpGet("/pagamento/{id:int}")]
    public IActionResult PortalPagamento(int id) => ServirPagina("pagamento.html");

    [HttpGet("/meus-agendamentos")]
    public IActionResult PortalMeusAgendamentos() => ServirPagina("meus-agendamentos.html");

    [HttpGet("/admin")]
    public IActionResult PortalAdmin() => ServirPagina("admin.html");

    [HttpGet("/admin/agenda")]
    public IActionResult PortalAdminAgenda() => ServirPagina("admin-agenda.html");

    private void SetClienteSessao(int id, string nome)
    {
        HttpContext.Session.SetInt32(SessionClienteId, id);
        HttpContext.Session.SetString(SessionClienteNome, nome);
    }

        // ─── Auth ────────────────────────────────────────────────────────────────
    [HttpPost("/auth/cadastro")]
    public async Task<IActionResult> Cadastro([FromBody] CadastroClienteRequest req)
    {
        var (ok, erro, clienteId) = await _portal.CadastrarAsync(req);
        if (!ok) return BadRequest(new { erro });
        SetClienteSessao(clienteId!.Value, req.Nome);
        return Json(new { ok = true, clienteId, redirect = "/agendar" });
    }

    [HttpPost("/auth/login")]
    public async Task<IActionResult> Login([FromBody] LoginClienteRequest req)
    {
        var (ok, erro, cliente) = await _portal.LoginAsync(req);
        if (!ok) return NotFound(new { erro });
        SetClienteSessao(cliente.Id, cliente.Nome);
        return Json(new { ok = true, redirect = "/agendar" });
    }

    [HttpGet("/auth/logout")]
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return Redirect("/");
    }

    [HttpGet("/auth/session")]
    public IActionResult SessionInfo()
    {
        var id = ClienteIdSessao;
        return Json(new
        {
            logado = id.HasValue,
            clienteId = id,
            clienteNome = HttpContext.Session.GetString(SessionClienteNome)
        });
    }

    // ─── API pública ─────────────────────────────────────────────────────────
    [HttpGet("/api/config")]
    public async Task<IActionResult> ConfigPublica()
    {
        var cfg = await _config.GetAsync();
        return Json(new { percentualSinal = cfg.PercentualSinal, prazoAlteracaoHoras = cfg.PrazoAlteracaoHoras });
    }

    [HttpGet("/api/servicos")]
    public async Task<IActionResult> Servicos()
    {
        var lista = await _context.Servicos.AsNoTracking()
            .OrderBy(s => s.Categoria).ThenBy(s => s.Nome)
            .Select(s => new
            {
                s.Id, s.Nome, s.Descricao, s.Valor, s.ValorPromocional,
                s.DuracaoMinutos, s.Categoria, s.ImagemUrl
            }).ToListAsync();
        return Json(lista);
    }

    [HttpGet("/api/profissionais")]
    public async Task<IActionResult> Profissionais() =>
        Json(await _context.Profissionais.AsNoTracking()
            .OrderBy(p => p.Nome)
            .Select(p => new { p.Id, p.Nome, p.Especialidade })
            .ToListAsync());

    [HttpGet("/api/horarios-disponiveis")]
    public async Task<IActionResult> HorariosDisponiveis(int profissionalId, string data, int? duracaoMinutos)
    {
        var slots = await _portal.HorariosDisponiveisAsync(
            profissionalId, data, duracaoMinutos ?? 60);
        return Json(slots);
    }

    [HttpPost("/api/agendamento")]
    public async Task<IActionResult> CriarAgendamento([FromBody] CriarAgendamentoRequest req)
    {
        var cid = ClienteIdSessao;
        if (!cid.HasValue) return Unauthorized(new { erro = "Sessão expirada. Faça login novamente." });

        var (ok, data, erro, code) = await _portal.CriarAgendamentoAsync(cid.Value, req);
        if (!ok) return StatusCode(code, new { erro });
        return Json(data);
    }

    [HttpGet("/api/meus-agendamentos")]
    public async Task<IActionResult> MeusAgendamentosApi()
    {
        var cid = ClienteIdSessao;
        if (!cid.HasValue) return Unauthorized(new { erro = "Não autenticado." });

        var lista = await _context.Agendamentos.AsNoTracking()
            .Where(a => a.ClienteId == cid.Value)
            .Include(a => a.Servico)
            .Include(a => a.Profissional)
            .OrderByDescending(a => a.DataHora)
            .ToListAsync();

        var result = lista.Select(a => new
        {
            a.Id,
            a.DataHora,
            a.DataFim,
            Status = BookingStatusMapper.ToPortal(a),
            a.Cancelado,
            a.ValorSinal,
            a.SinalPago,
            a.ValorRestante,
            a.RestantePago,
            Servico = a.Servico?.Nome,
            ValorServico = a.Servico?.Valor,
            DuracaoMinutos = a.Servico?.DuracaoMinutos ?? 0,
            Profissional = a.Profissional?.Nome
        });
        return Json(result);
    }

    [HttpGet("/api/agendamento/{id:int}")]
    public async Task<IActionResult> AgendamentoDetalhe(int id)
    {
        var cid = ClienteIdSessao;
        if (!cid.HasValue) return Unauthorized(new { erro = "Sessão expirada." });

        var a = await _context.Agendamentos.AsNoTracking()
            .Include(x => x.Servico)
            .Include(x => x.Profissional)
            .FirstOrDefaultAsync(x => x.Id == id && x.ClienteId == cid.Value);
        if (a == null) return NotFound(new { erro = "Agendamento não encontrado." });

        return Json(new
        {
            a.Id,
            a.DataHora,
            a.DataFim,
            Status = BookingStatusMapper.ToPortal(a),
            a.Cancelado,
            a.ValorSinal,
            a.SinalPago,
            a.ValorRestante,
            Servico = a.Servico?.Nome,
            Profissional = a.Profissional?.Nome
        });
    }

    [HttpDelete("/api/agendamentos/{id:int}")]
    public async Task<IActionResult> Cancelar(int id)
    {
        var cid = ClienteIdSessao;
        if (!cid.HasValue) return Unauthorized(new { erro = "Não autenticado." });

        var (ok, msg, erro, code) = await _portal.CancelarClienteAsync(id, cid.Value);
        if (!ok) return StatusCode(code, new { erro });
        return Json(new { mensagem = msg });
    }

    [HttpPost("/api/agendamentos/{id:int}/reagendar")]
    public async Task<IActionResult> Reagendar(int id, [FromBody] ReagendarRequest req)
    {
        var cid = ClienteIdSessao;
        if (!cid.HasValue) return Unauthorized(new { erro = "Não autenticado." });

        var (ok, data, erro, code) = await _portal.ReagendarAsync(id, cid.Value, req);
        if (!ok) return StatusCode(code, new { erro });
        return Json(data);
    }

    [HttpPost("/api/agendamento/{id:int}/informar-pagamento")]
    public async Task<IActionResult> InformarPagamento(int id)
    {
        var cid = ClienteIdSessao;
        if (!cid.HasValue) return Unauthorized(new { erro = "Não autenticado." });

        await _portal.InformarPagamentoAsync(id, cid.Value);
        return Json(new
        {
            ok = true,
            mensagem = "Recebemos sua confirmação! A administração irá validar o PIX e aprovar seu horário em breve."
        });
    }

    // ─── Admin (token) ───────────────────────────────────────────────────────
    [HttpGet("/admin/aprovar/{id:int}")]
    public async Task<IActionResult> Aprovar(int id, [FromQuery] string? token)
    {
        if (!_portal.ValidarAdminToken(token)) return Content("Acesso negado.");
        var ok = await _portal.AprovarPagamentoAsync(id);
        if (!ok) return Content("Erro ao aprovar pagamento.");
        return Content($"<html><body style='font-family:sans-serif;text-align:center;padding:40px;background:#1a1a2e;color:#c9a84c;'>" +
                         $"<h2>Agendamento #{id} aprovado!</h2><p style='color:#ccc'>Cliente notificada por e-mail.</p></body></html>",
            "text/html");
    }

    [HttpGet("/api/admin/config")]
    public async Task<IActionResult> AdminConfigGet([FromQuery] string? token)
    {
        if (!_portal.ValidarAdminToken(token) && !_portal.ValidarAdminToken(Request.Headers["x-admin-token"]))
            return StatusCode(403, new { erro = "Acesso negado." });
        return Json(await _config.GetAsync());
    }

    [HttpPut("/api/admin/config")]
    public async Task<IActionResult> AdminConfigPut([FromBody] AdminConfigPutRequest body)
    {
        var token = body?.Token ?? Request.Headers["x-admin-token"].FirstOrDefault();
        if (!_portal.ValidarAdminToken(token))
            return StatusCode(403, new { erro = "Acesso negado." });

        var cfg = await _config.GetAsync();
        var updates = await _config.UpdateAsync(new BookingConfigDto
        {
            PercentualSinal = body?.PercentualSinal ?? cfg.PercentualSinal,
            DiasLembreteAntes = body?.DiasLembreteAntes ?? cfg.DiasLembreteAntes,
            PrazoAlteracaoHoras = body?.PrazoAlteracaoHoras ?? cfg.PrazoAlteracaoHoras
        });
        return Json(new { ok = true, config = updates });
    }

    [HttpGet("/api/admin/agendamentos")]
    public async Task<IActionResult> AdminAgendamentos(
        [FromQuery] string? token,
        [FromQuery] DateTime? de,
        [FromQuery] DateTime? ate,
        [FromQuery] string? tipo)
    {
        var t = token ?? Request.Headers["x-admin-token"].FirstOrDefault();
        if (!_portal.ValidarAdminToken(t)) return StatusCode(403, new { erro = "Acesso negado." });

        var q = _context.Agendamentos.AsNoTracking()
            .Include(a => a.Cliente)
            .Include(a => a.Servico)
            .Include(a => a.Profissional)
            .Where(a => !a.Cancelado);

        if (de.HasValue && ate.HasValue)
            q = q.Where(a => a.DataHora >= de && a.DataHora < ate);
        else if (tipo == "proximos")
            q = q.Where(a => a.DataHora >= DateTime.Now && a.DataHora <= DateTime.Now.AddDays(7));

        var lista = await q.OrderBy(a => a.DataHora).ToListAsync();

        return Json(lista.Select(a => new
        {
            a.Id,
            a.DataHora,
            a.DataFim,
            Status = a.Status.ToString(),
            a.Cancelado,
            a.ValorSinal,
            a.SinalPago,
            a.ValorRestante,
            ClienteNome = a.Cliente?.Nome,
            ClienteWA = a.Cliente?.WhatsApp,
            Servico = a.Servico?.Nome,
            ProfissionalNome = a.Profissional?.Nome,
            ValorTotal = a.ValorSinal + a.ValorRestante
        }));
    }

    [HttpPost("/api/admin/cancelar/{id:int}")]
    public async Task<IActionResult> AdminCancelar(int id)
    {
        if (!_portal.ValidarAdminToken(Request.Headers["x-admin-token"].FirstOrDefault()))
            return StatusCode(403, new { erro = "Acesso negado." });

        var ag = await _context.Agendamentos.FindAsync(id);
        if (ag == null) return NotFound();
        ag.Cancelado = true;
        ag.Status = StatusAgendamento.Cancelado;
        await _context.SaveChangesAsync();
        return Json(new { ok = true });
    }

    [HttpPost("/api/admin/finalizar/{id:int}")]
    public async Task<IActionResult> AdminFinalizar(int id)
    {
        if (!_portal.ValidarAdminToken(Request.Headers["x-admin-token"].FirstOrDefault()))
            return StatusCode(403, new { erro = "Acesso negado." });

        var ag = await _context.Agendamentos.FindAsync(id);
        if (ag == null || ag.Status != StatusAgendamento.Confirmado)
            return BadRequest(new { erro = "Apenas confirmados podem ser finalizados." });

        ag.RestantePago = true;
        ag.Status = StatusAgendamento.Concluido;
        ag.DataConclusao = DateTime.Now;
        await _context.SaveChangesAsync();
        return Json(new { ok = true });
    }
}
