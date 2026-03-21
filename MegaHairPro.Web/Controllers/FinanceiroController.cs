using MegaHairPro.Web.Data;
using MegaHairPro.Web.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;

namespace MegaHairPro.Web.Controllers;

public class FinanceiroController : Controller
{
    private readonly AppDbContext _context;

    public FinanceiroController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public IActionResult Index(DateTime? inicio, DateTime? fim, string? tipo, string? status)
    {
        inicio ??= DateTime.Today.AddDays(-7);
        fim ??= DateTime.Today;

        var query = _context.ContasFinanceiras
            .Where(c => c.DataCompetencia.Date >= inicio.Value.Date &&
                        c.DataCompetencia.Date <= fim.Value.Date);

        if (!string.IsNullOrEmpty(tipo) && Enum.TryParse<TipoContaFinanceira>(tipo, out var tipoEnum))
            query = query.Where(c => c.Tipo == tipoEnum);

        if (!string.IsNullOrEmpty(status) && Enum.TryParse<StatusContaFinanceira>(status, out var statusEnum))
            query = query.Where(c => c.Status == statusEnum);

        var contas = query.OrderByDescending(c => c.DataCompetencia).ToList();

        // Força decimal para evitar o bug do 0 virar int no ViewBag
        decimal entradas = contas
            .Where(c => c.Tipo == TipoContaFinanceira.Receber && c.Status == StatusContaFinanceira.Paga)
            .Sum(c => (decimal?)c.Valor) ?? 0m;

        decimal saidas = contas
            .Where(c => c.Tipo == TipoContaFinanceira.Pagar && c.Status == StatusContaFinanceira.Paga)
            .Sum(c => (decimal?)c.Valor) ?? 0m;

        ViewBag.Inicio = inicio.Value.ToString("yyyy-MM-dd");
        ViewBag.Fim = fim.Value.ToString("yyyy-MM-dd");
        ViewBag.TotalEntradas = entradas;
        ViewBag.TotalSaidas = saidas;
        ViewBag.Saldo = entradas - saidas;
        ViewBag.FiltroTipo = tipo ?? "";
        ViewBag.FiltroStatus = status ?? "";

        return View(contas);
    }

    // ✅ AJAX — Marcar como pago
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult MarcarComoPagoAjax(int id)
    {
        var conta = _context.ContasFinanceiras.FirstOrDefault(c => c.Id == id);
        if (conta == null) return Json(new { success = false });

        conta.Status = StatusContaFinanceira.Paga;
        conta.DataPagamento = DateTime.Now;
        _context.SaveChanges();

        return Json(new { success = true });
    }

    // ✅ AJAX — Cancelar
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult CancelarAjax(int id)
    {
        var conta = _context.ContasFinanceiras.FirstOrDefault(c => c.Id == id);
        if (conta == null) return Json(new { success = false });

        conta.Status = StatusContaFinanceira.Cancelada;
        _context.SaveChanges();

        return Json(new { success = true });
    }

    // ✅ AJAX — Retorna totais atualizados
    [HttpGet]
    public IActionResult Totais(DateTime? inicio, DateTime? fim)
    {
        inicio ??= DateTime.Today.AddDays(-7);
        fim ??= DateTime.Today;

        var contas = _context.ContasFinanceiras
            .Where(c => c.DataCompetencia.Date >= inicio.Value.Date &&
                        c.DataCompetencia.Date <= fim.Value.Date)
            .ToList();

        decimal entradas = contas
            .Where(c => c.Tipo == TipoContaFinanceira.Receber && c.Status == StatusContaFinanceira.Paga)
            .Sum(c => (decimal?)c.Valor) ?? 0m;

        decimal saidas = contas
            .Where(c => c.Tipo == TipoContaFinanceira.Pagar && c.Status == StatusContaFinanceira.Paga)
            .Sum(c => (decimal?)c.Valor) ?? 0m;

        var culture = new System.Globalization.CultureInfo("pt-BR");

        return Json(new
        {
            entradas = entradas.ToString("N2", culture),
            saidas = saidas.ToString("N2", culture),
            saldo = (entradas - saidas).ToString("N2", culture),
            saldoNegativo = (entradas - saidas) < 0
        });
    }
}