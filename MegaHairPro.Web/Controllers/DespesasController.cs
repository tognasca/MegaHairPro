using MegaHairPro.Web.Data;
using MegaHairPro.Web.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;

namespace MegaHairPro.Web.Controllers
{
    public class DespesasController : Controller
    {
        private readonly AppDbContext _context;
        int mediaVendasMes = 50; // você define
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

        // ✅ CREATE
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Index(Despesas despesa)
        {

            despesa.Codigo = "DESP-" + DateTime.Now.Ticks.ToString().Substring(10);


            string cor = despesa.Categoria switch
            {
                "Salão" => "bg-success",

                "Marketing" => "bg-success",

                "Produto" => "bg-primary",

                "Internet" => "bg-warning",

                "Aluguel" => "bg-danger",

                _ => "bg-secondary"

            };

            despesa.CorCategoria = cor;


            _context.Despesas.Add(despesa);

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

            return RedirectToAction("Index");

        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateAjax(Despesas d)
        {

            d.CorCategoria = CorCategoria(d.Categoria);

            _context.Add(d);

            _context.SaveChanges();

            _context.ContasFinanceiras.Add(new ContaFinanceira
            {
                Tipo = TipoContaFinanceira.Pagar,
                Origem = OrigemContaFinanceira.Despesa,
                Descricao = $"Despesa {d.Categoria} - {d.Descricao}",
                Valor = d.Valor,
                DataCompetencia = d.DataCadastro,
                DataVencimento = d.DataCadastro,
                Status = StatusContaFinanceira.Aberta,
                DespesaId = d.Id
            });

            _context.SaveChanges();

            return PartialView("_DespesaRow", d);

        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditAjax(Despesas d)
        {

            var despesa =
            _context.Despesas.Find(d.Id);


            despesa.Categoria = d.Categoria;

            despesa.Descricao = d.Descricao;

            despesa.Valor = d.Valor;

            despesa.DataCadastro = d.DataCadastro;

            despesa.Ratear = d.Ratear;

            despesa.CorCategoria =
            CorCategoria(d.Categoria);


            _context.SaveChanges();

            return PartialView("_DespesaRow", despesa);

        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteAjax(int id)
        {

            var d =
            _context.Despesas.Find(id);

            _context.Remove(d);

            _context.SaveChanges();

            return Ok();

        }
        string CorCategoria(string cat)
        {

            return cat switch
            {

                "Marketing" => "bg-success",

                "Produto" => "bg-primary",

                "Internet" => "bg-warning",

                "Aluguel" => "bg-danger",

                _ => "bg-secondary"

            };

        }
    }
}
