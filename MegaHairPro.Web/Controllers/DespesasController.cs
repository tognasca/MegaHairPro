using MegaHairPro.Web.Data;
using MegaHairPro.Web.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Globalization;
using System.Linq;

namespace MegaHairPro.Web.Controllers
{
    public class DespesasController : Controller
    {
        private readonly AppDbContext _context;
        int mediaVendasMes = 50;

        public DespesasController(AppDbContext context)
        {
            _context = context;
        }

        // ✅ LISTAGEM
        [HttpGet]
        public IActionResult Index()
        {
            var despesas = _context.Despesas
                .OrderByDescending(d => d.DataCadastro)
                .ToList();
            return View(despesas);
        }

        [HttpGet]
        public IActionResult TotalMes()
        {
            var total = _context.Despesas
                .Where(d => d.DataCadastro.Month == DateTime.Now.Month
                         && d.DataCadastro.Year == DateTime.Now.Year)
                .Sum(d => (decimal?)d.Valor) ?? 0;

            return Json(total.ToString("N2", new System.Globalization.CultureInfo("pt-BR")));
        }

        // ✅ CREATE AJAX
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateAjax(Despesas despesa)
        {

            despesa.Codigo = "DESP-" + DateTime.Now.Ticks.ToString().Substring(10);
            despesa.CorCategoria = CorCategoria(despesa.Categoria);

            // Garante parse correto do valor (ponto decimal do JS)
            if (Request.Form.TryGetValue("Valor", out var valorStr))
            {
                if (decimal.TryParse(valorStr, NumberStyles.Any, CultureInfo.InvariantCulture, out var valor))
                    despesa.Valor = valor;
            }
            _context.Add(despesa);
            _context.SaveChanges();

            _context.ContasFinanceiras.Add(new ContaFinanceira
            {
                Tipo = TipoContaFinanceira.Pagar,
                Origem = OrigemContaFinanceira.Despesa,
                Descricao = $"Despesa {despesa.Categoria} - {despesa.Descricao}",
                Valor = despesa.Valor,
                DataCompetencia = despesa.DataCadastro,
                DataVencimento = despesa.DataCadastro,
                Status = StatusContaFinanceira.Aberta,
                DespesaId = despesa.Id
            });

            _context.SaveChanges();
            return PartialView("_DespesaRow", despesa);
        }

        // ✅ EDIT AJAX
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditAjax(Despesas d)
        {
            var despesa = _context.Despesas.Find(d.Id);

            if (despesa == null)
                return Json(new { success = false });

            despesa.Categoria = d.Categoria;
            despesa.Descricao = d.Descricao;
            despesa.DataCadastro = d.DataCadastro;
            despesa.Ratear = d.Ratear;
            despesa.CorCategoria = CorCategoria(d.Categoria);

            // Garante parse correto do valor
            if (Request.Form.TryGetValue("Valor", out var valorStr))
            {
                if (decimal.TryParse(valorStr, NumberStyles.Any, CultureInfo.InvariantCulture, out var valor))
                    despesa.Valor = valor;
            }

            _context.Update(despesa);
            _context.SaveChanges(); // estava faltando no original!

            return PartialView("_DespesaRow", despesa);
        }

        // ✅ DELETE AJAX
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteAjax(int id)
        {
            var despesa = _context.Despesas.Find(id);

            if (despesa == null)
                return Ok();

            var contas = _context.ContasFinanceiras.Where(c => c.DespesaId == despesa.Id);
            _context.ContasFinanceiras.RemoveRange(contas);
            _context.Despesas.Remove(despesa);
            _context.SaveChanges();

            return Ok();
        }

        string CorCategoria(string cat)
        {
            return cat switch
            {
                "Salão" => "bg-success",
                "Marketing" => "bg-info",
                "Produto" => "bg-primary",
                "Internet" => "bg-warning",
                "Aluguel" => "bg-danger",
                "Outros" => "bg-dark",
                _ => "bg-secondary"
            };
        }
    }
}