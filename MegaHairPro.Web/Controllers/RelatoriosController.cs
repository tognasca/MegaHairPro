using ClosedXML.Excel;
using MegaHairPro.Web.Data;
using MegaHairPro.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MegaHairPro.Web.Controllers;

public class RelatoriosController : Controller
{
    private readonly AppDbContext _context;

    public RelatoriosController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Vendas(DateTime? inicio, DateTime? fim, int? produtoId, int? clienteId)
    {
        inicio ??= DateTime.Today.AddDays(-7);
        fim ??= DateTime.Today;

        var query = _context.Vendas.AsQueryable();

        query = query.Where(v => v.DataVenda.Date >= inicio.Value.Date &&
                                 v.DataVenda.Date <= fim.Value.Date);

        if (produtoId.HasValue && produtoId > 0)
            query = query.Where(v => v.ProdutoId == produtoId.Value);

        if (clienteId.HasValue && clienteId > 0)
            query = query.Where(v => v.ClienteId == clienteId.Value);

        var lista = await query
            .OrderByDescending(v => v.DataVenda)
            .ToListAsync();

        ViewBag.Inicio = inicio.Value.ToString("yyyy-MM-dd");
        ViewBag.Fim = fim.Value.ToString("yyyy-MM-dd");
        ViewBag.Produtos = _context.Produtos.OrderBy(p => p.Nome).ToList();
        ViewBag.Clientes = _context.Clientes.OrderBy(c => c.Nome).ToList();
        ViewBag.TotalVendido = lista.Sum(v => v.PrecoVenda ?? 0);
        ViewBag.TotalLucro = lista.Sum(v => v.LucroTotal ?? 0);

        return View(lista);
    }

    [HttpGet]
    public async Task<IActionResult> ExportarVendas(DateTime? inicio, DateTime? fim, int? produtoId, int? clienteId)
    {
        inicio ??= DateTime.Today.AddDays(-7);
        fim ??= DateTime.Today;

        var query = _context.Vendas.AsQueryable();
        query = query.Where(v => v.DataVenda.Date >= inicio.Value.Date &&
                                 v.DataVenda.Date <= fim.Value.Date);

        if (produtoId.HasValue && produtoId > 0)
            query = query.Where(v => v.ProdutoId == produtoId.Value);

        if (clienteId.HasValue && clienteId > 0)
            query = query.Where(v => v.ClienteId == clienteId.Value);

        var dados = await query
            .OrderByDescending(v => v.DataVenda)
            .ToListAsync();

        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("Vendas");

        ws.Cell(1, 1).Value = "Data";
        ws.Cell(1, 2).Value = "Cliente";
        ws.Cell(1, 3).Value = "Produto";
        ws.Cell(1, 4).Value = "Quantidade";
        ws.Cell(1, 5).Value = "Preço";
        ws.Cell(1, 6).Value = "Lucro";

        var row = 2;
        foreach (var v in dados)
        {
            ws.Cell(row, 1).Value = v.DataVenda;
            ws.Cell(row, 2).Value = v.ClienteNome;
            ws.Cell(row, 3).Value = v.ProdutoNome;
            ws.Cell(row, 4).Value = v.QuantidadeGramas;
            ws.Cell(row, 5).Value = v.PrecoVenda ?? 0;
            ws.Cell(row, 6).Value = v.LucroTotal ?? 0;
            row++;
        }

        ws.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        wb.SaveAs(stream);
        var bytes = stream.ToArray();

        return File(bytes,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "RelatorioVendas.xlsx");
    }

    [HttpGet]
    public async Task<IActionResult> Comissoes(DateTime? inicio, DateTime? fim)
    {
        inicio ??= DateTime.Today.AddDays(-7);
        fim ??= DateTime.Today;

        var query = _context.Comissoes.AsQueryable();
        query = query.Where(c => c.DataGeracao.Date >= inicio.Value.Date &&
                                 c.DataGeracao.Date <= fim.Value.Date);

        var lista = await query
            .OrderByDescending(c => c.DataGeracao)
            .ToListAsync();

        ViewBag.Inicio = inicio.Value.ToString("yyyy-MM-dd");
        ViewBag.Fim = fim.Value.ToString("yyyy-MM-dd");
        ViewBag.TotalComissoes = lista.Sum(c => c.Valor);

        return View(lista);
    }
}

