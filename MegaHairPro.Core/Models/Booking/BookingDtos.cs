using System;
using System.Collections.Generic;

namespace MegaHairPro.Web.Models.Booking;

public class CriarAgendamentoRequest
{
    public int ServicoId { get; set; }
    public int ProfissionalId { get; set; }
    public DateTime DataHora { get; set; }
    public int[]? ServicosAdicionais { get; set; }
    public decimal? ValorTotalCustom { get; set; }
    public int? DuracaoTotalCustom { get; set; }
    public string? NomeServicosCompleto { get; set; }
}

public class CadastroClienteRequest
{
    public string Nome { get; set; } = "";
    public string? Telefone { get; set; }
    public string? WhatsApp { get; set; }
    public string? Email { get; set; }
    public string? Cpf { get; set; }
    public DateTime? DataNascimento { get; set; }
    public string? Senha { get; set; }
}

public class LoginClienteRequest
{
    public string? WhatsApp { get; set; }
    public string? Email { get; set; }
    public string? Senha { get; set; }
}

public class ReagendarRequest
{
    public DateTime DataHora { get; set; }
    public int? ProfissionalId { get; set; }
}

public class AgendamentoManualRequest
{
    public string ClienteNome { get; set; } = "";
    public string ClienteWA { get; set; } = "";
    public string? ClienteObs { get; set; }
    public int ServicoId { get; set; }
    public int ProfissionalId { get; set; }
    public DateTime DataHora { get; set; }
    public decimal? PercentualSinal { get; set; }
    public int[]? ServicosAdicionais { get; set; }
    public decimal? ValorTotalCustom { get; set; }
    public int? DuracaoTotalCustom { get; set; }
    public string? NomeServicosCompleto { get; set; }
    public string? Token { get; set; }
}

public class ImportarContatoDto
{
    public string? Nome { get; set; }
    public string? Telefone { get; set; }
    public string? WhatsApp { get; set; }
    public string? Email { get; set; }
}

public class SlotHorarioDto
{
    public string Inicio { get; set; } = "";
    public string FimPrevisto { get; set; } = "";
    public string Hora { get; set; } = "";
    public int Score { get; set; }
    public string EncaixeTipo { get; set; } = "normal";
}

public class BookingConfigDto
{
    public decimal PercentualSinal { get; set; } = 30;
    public int DiasLembreteAntes { get; set; } = 2;
    public int PrazoAlteracaoHoras { get; set; } = 24;
}

public class AdminConfigPutRequest
{
    public string? Token { get; set; }
    public decimal? PercentualSinal { get; set; }
    public int? DiasLembreteAntes { get; set; }
    public int? PrazoAlteracaoHoras { get; set; }
}

public class PagamentoPixDto
{
    public string Tipo { get; set; } = "pix_manual";
    public string? PixCopiaECola { get; set; }
    public string? QrCodeUrl { get; set; }
    public string? Chave { get; set; }
    public string? Mensagem { get; set; }
}
