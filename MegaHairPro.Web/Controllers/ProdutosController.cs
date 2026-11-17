using System;
using System.Globalization;
using MegaHairPro.Web.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Authorization;
using MegaHairPro.Web.Services;
using System.Threading.Tasks;
using MegaHairPro.Web.Data;
using MegaHairPro.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace MegaHairPro.Web.Controllers
{
    public class ProdutosController : Controller
    {
        private readonly AppDbContext _context;

        public ProdutosController(AppDbContext context)
        {
            _context = context;
        }

        public sealed record OrigemSimulador(string Nome, string Codigo, string Situacao);

        [HttpGet]
        public IActionResult Index()
        {
            var produtos = _context.Produtos.AsNoTracking().OrderByDescending(p => p.Id).ToList();
            return View(produtos);
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Simulador([FromServices] BookingConfigService config,
            [FromServices] IConfiguration configuracao, [FromServices] ICustoMedioService custos)
        {
            var codigos = CatalogoSimulador.Origens
                .Select(origem => CatalogoSimulador.Codigo(configuracao, origem.Chave))
                .ToArray();
            var produtos = _context.Produtos.AsNoTracking()
                .Where(produto => codigos.Contains(produto.Codigo!)).ToList();

            var orcamento = await config.LerOrcamentoSimuladorAsync();
            ViewBag.MostrarOrcamentoAntes = orcamento.MostrarAntes;
            ViewBag.PercentualLucro = orcamento.PercentualLucro.ToString(CultureInfo.InvariantCulture);
            ViewBag.OrigensSimulador = CatalogoSimulador.Origens.Select(origem =>
            {
                var codigo = CatalogoSimulador.Codigo(configuracao, origem.Chave);
                var encontrados = produtos.Where(p => p.Codigo == codigo).Take(2).ToArray();
                var situacao = "Produto ainda não cadastrado";
                if (encontrados.Length > 1 || encontrados.Any(p => p.TipoProduto != TipoProduto.Cabelo))
                    situacao = "Código em conflito";
                else if (encontrados.Length == 1 && !encontrados[0].Status)
                    situacao = "Produto inativo";
                else if (encontrados.Length == 1)
                {
                    var custo = custos.CalculaCustoCabelo(encontrados[0].Id, 1m, orcamento.PercentualLucro).SingleOrDefault();
                    situacao = custo is not null && Convert.ToDecimal(custo["custo"]) > 0
                        ? "Custo disponível para sincronizar" : "Aguardando compra com custo";
                }

                return new OrigemSimulador(origem.Origem, codigo, situacao);
            }).ToArray();
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SalvarOrcamentoAntes(
            bool ativo, string? percentualLucro, [FromServices] BookingConfigService config)
        {
            if (!decimal.TryParse(percentualLucro?.Trim().Replace(',', '.'), NumberStyles.AllowDecimalPoint,
                CultureInfo.InvariantCulture, out var lucro) || lucro < 0)
            {
                TempData["SimuladorErro"] = "Informe um percentual de lucro igual ou maior que zero.";
                return RedirectToAction(nameof(Simulador));
            }

            await config.SalvarOrcamentoSimuladorAsync(ativo, lucro);
            TempData["SimuladorSucesso"] = "Configuração do orçamento salva.";
            return RedirectToAction(nameof(Simulador));
        }

        // ✅ CREATE CORRETO
        [HttpPost]
        
        [ValidateAntiForgeryToken]
        public IActionResult Index(Produto produto)
        {
            produto.Status = true;
            produto.Codigo = GerarCodigoProduto(produto.TipoProduto);
            produto.UnidadeMedida = produto.TipoProduto == TipoProduto.Cabelo
                ? UnidadeMedida.Grama
                : produto.UnidadeMedida;

            produto.Descricao = produto.ObterDescricao();
            _context.Produtos.Add(produto);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        // ✅ EDIT AJAX
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditAjax(Produto produto)
        {
            var original = _context.Produtos.Find(produto.Id);

            if (original == null)
                return Json(new { success = false });

            original.Descricao = produto.ObterDescricao();
            original.TipoProduto = produto.TipoProduto;
            original.UnidadeMedida = produto.TipoProduto == TipoProduto.Cabelo
                ? UnidadeMedida.Grama
                : produto.UnidadeMedida;
            original.Origem = produto.Origem;
            original.Tamanho = produto.Tamanho;
            original.Cor = produto.Cor;

            _context.SaveChanges();

            return PartialView("_ProdutoRow", original);
        }

        // ✅ ATIVAR / INATIVAR
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ToggleStatusAjax(int id)
        {
            var produto = _context.Produtos.Find(id);

            if (produto == null)
                return Json(new { success = false });

            produto.Status = !produto.Status;
            _context.SaveChanges();


            return PartialView("_ProdutoRow", produto);
        }

        // ✅ DELETE
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteAjax(int id)
        {

            var produto = _context.Produtos.Find(id);

            if (produto != null)
            {

                _context.Produtos.Remove(produto);

                _context.SaveChanges();

            }

            return Ok();

        }
        [HttpPost]
        public IActionResult CreateAjax(Produto model)
        {

            model.Codigo = GerarCodigoProduto(model.TipoProduto);

            model.Status = true;
            model.UnidadeMedida = model.TipoProduto == TipoProduto.Cabelo
                ? UnidadeMedida.Grama
                : model.UnidadeMedida;

            model.Descricao = model.ObterDescricao();
            _context.Produtos.Add(model);

            _context.SaveChanges();


            return PartialView("_ProdutoRow", model);

        }

          // ✅ GERAR CÓDIGO
        private string GerarCodigoProduto(TipoProduto tipo)
        {
            string prefixo = tipo switch
            {
                TipoProduto.Cabelo => "CAB",
                TipoProduto.Acessorio => "ACE",
                _ => "GEN"
            };

            var ultimoCodigo = _context.Produtos
                .Where(p => p.TipoProduto == tipo && p.Codigo != null && p.Codigo.StartsWith(prefixo + "-"))
                .OrderByDescending(p => p.Codigo)
                .Select(p => p.Codigo)
                .FirstOrDefault();

            int numero = 1;

            if (!string.IsNullOrEmpty(ultimoCodigo))
            {
                var partes = ultimoCodigo.Split('-');

                if (partes.Length == 2 && int.TryParse(partes[1], out int n))   
                    numero = n + 1;
            }

            return $"{prefixo}-{numero:D4}";
        }
    }
}
