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
    public IActionResult Index(DateTime? inicio, DateTime? fim)
    {
        inicio ??= DateTime.Today.AddDays(-7);
        fim ??= DateTime.Today;

        var contas = _context.ContasFinanceiras
            .Where(c => c.DataCompetencia.Date >= inicio.Value.Date &&
                        c.DataCompetencia.Date <= fim.Value.Date)
            .OrderBy(c => c.DataCompetencia)
            .ToList();

        ViewBag.Inicio = inicio.Value.ToString("yyyy-MM-dd");
        ViewBag.Fim = fim.Value.ToString("yyyy-MM-dd");

        ViewBag.TotalEntradas = contas
            .Where(c => c.Tipo == TipoContaFinanceira.Receber &&
                        c.Status == StatusContaFinanceira.Paga)
            .Sum(c => c.Valor);

        ViewBag.TotalSaidas = contas
            .Where(c => c.Tipo == TipoContaFinanceira.Pagar &&
                        c.Status == StatusContaFinanceira.Paga)
            .Sum(c => c.Valor);

        ViewBag.Saldo = (decimal)ViewBag.TotalEntradas - (decimal)ViewBag.TotalSaidas;

        return View(contas);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult MarcarComoPago(int id, string? formaPagamento)
    {
        var conta = _context.ContasFinanceiras.FirstOrDefault(c => c.Id == id);
        if (conta == null)
            return NotFound();

        conta.Status = StatusContaFinanceira.Paga;
        conta.DataPagamento = DateTime.Now;
        conta.FormaPagamento = formaPagamento;

        _context.SaveChanges();

        return RedirectToAction("Index");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Cancelar(int id)
    {
        var conta = _context.ContasFinanceiras.FirstOrDefault(c => c.Id == id);
        if (conta == null)
            return NotFound();

        conta.Status = StatusContaFinanceira.Cancelada;
        _context.SaveChanges();

        return RedirectToAction("Index");
    }
}

