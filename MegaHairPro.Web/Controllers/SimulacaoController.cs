using MegaHairPro.Web.Data;
using MegaHairPro.Web.Models;
using MegaHairPro.Web.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;

namespace MegaHairPro.Web.Controllers
{
    public class SimulacaoController : Controller
    {
        private readonly ISimulacaoService _simulacao;
        private readonly AppDbContext _context;

        public SimulacaoController(
            ISimulacaoService simulacao,
            AppDbContext context)
        {
            _simulacao = simulacao;
            _context = context;
        }

        // ✅ GET
        [HttpGet]
        public IActionResult Index()
        {
            CarregarCliente();
            CarregarProdutos();
            CarregarDespesas();
            var lista = _context.Simulacao.Where(x => x.Status == true)
                .OrderByDescending(x => x.DataCadastro)
                .ToList();

            return View(lista); // ✅ SEMPRE LISTA
        }

        // ✅ POST (CORRIGIDO: NÃO PODE RETORNAR OBJETO ÚNICO)
        [HttpPost]
        public IActionResult Index(SimulacaoVenda model)
        {
            decimal totalDespesas = 0;
            // ✅ Quantas vendas/serviços você faz por mês (média)
            int mediaAtendimentosMes = 50;
            // ✅ Busca as despesas selecionadas
            var despesasSelecionadas = _context.Despesas.Where(d => model.DespesasSelecionadas.Contains(d.Id)).ToList();
            foreach (var desp in despesasSelecionadas)
            {
                if (desp.Ratear)
                    totalDespesas += desp.Valor / mediaAtendimentosMes;
                else
                    totalDespesas += desp.Valor;
            }
            model.TotalDespesasSelecionadas = totalDespesas;

            var resultado = _simulacao.Calcular(model);

            resultado.Status = true;
            resultado.DataCadastro = DateTime.Now;

            resultado.CustoTotal += totalDespesas;
            resultado.PrecoVenda += totalDespesas;

            // ✅ Corrige Cliente e Produto no Grid
            var cliente = _context.Clientes.FirstOrDefault(c => c.Id == model.ClienteId);
            var produto = _context.Produtos.FirstOrDefault(p => p.Id == model.ProdutoId);

            resultado.ClienteNome = cliente?.Nome;
            resultado.ProdutoNome = produto?.Nome;

            _context.Simulacao.Add(resultado);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }


        // ✅ CARREGAR PRODUTOS
        private void CarregarProdutos()
        {
            ViewBag.Produtos = _context.Produtos
                .Select(p => new
                {
                    p.Id,
                    p.Codigo,
                    p.Nome,
                    p.EstoqueGramas,
                    p.Status
                })
                .Where(x => x.Status == true)
                .ToList();
        }
        private void CarregarDespesas()
        {
            ViewBag.Despesas = _context.Despesas.Where(x => x.Status == true).ToList();
        }
        // ✅ CARREGAR CLIENTES
        private void CarregarCliente()
        {
            ViewBag.Clientes = _context.Clientes
                .Select(p => new
                {
                    p.Id,
                    p.CPF,
                    p.Nome
                })
                .ToList();
        }

        // ✅ AJAX – ESTOQUE (CORRIGIDO: SOMA TOTAL)
        [HttpGet]
        public IActionResult ObterEstoqueProduto(int produtoId)
        {
            var estoque = _context.Compras
                .Where(c => c.ProdutoId == produtoId)
                .Sum(c => c.QuantidadeGramas ?? 0);

            return Json(new { estoque });
        }

        // ✅ AJAX – CUSTO DO CABELO
        [HttpGet]
        public IActionResult ObterCustoCabelo(int produtoId, decimal quantidadeGramas)
        {
            if (quantidadeGramas <= 0)
                return Json(new { custo = 0 });

            var compras = _context.Compras
                .Where(c => c.ProdutoId == produtoId && c.QuantidadeGramas > 0)
                .ToList();

            if (!compras.Any())
                return Json(new { custo = 0 });

            var totalValor = compras.Sum(c => c.ValorTotal);
            var totalGramas = compras.Sum(c => c.QuantidadeGramas.Value);

            var custoMedioPorGrama = totalValor / totalGramas;
            var custoCabelo = custoMedioPorGrama * quantidadeGramas;

            return Json(new { custo = custoCabelo });
        }

        // ✅ AJAX – CUSTO + LUCRO
        [HttpGet]
        public IActionResult ObterLucroCabelo(int produtoId, decimal quantidadeGramas, decimal percentualLucro)
        {
            if (produtoId <= 0 || quantidadeGramas <= 0)
                return Json(new { custo = 0m, lucro = 0m });

            var compras = _context.Compras
                .Where(c => c.ProdutoId == produtoId && c.QuantidadeGramas > 0)
                .ToList();

            if (!compras.Any())
                return Json(new { custo = 0m, lucro = 0m });

            var totalValor = compras.Sum(c => c.ValorTotal);
            var totalGramas = compras.Sum(c => c.QuantidadeGramas!.Value);

            var custoMedioGrama = totalValor / totalGramas;
            var custo = custoMedioGrama * quantidadeGramas;
            var lucro = custo * (percentualLucro / 100m);

            return Json(new { custo, lucro });
        }

        // ✅ EDIT AJAX (CORRIGIDO: ATUALIZA LUCRO TOTAL TAMBÉM)
        [HttpPost]
        public IActionResult EditAjax(SimulacaoVenda model)
        {
            var simulacao = _context.Simulacao
                .FirstOrDefault(x => x.Id == model.Id);

            if (simulacao == null)
                return NotFound();

            simulacao.PrecoVenda = model.PrecoVenda;
            simulacao.CustoTotal = model.CustoTotal;

            simulacao.LucroLiquido = simulacao.PrecoVenda - simulacao.CustoTotal;

            // ✅ IMPORTANTE: SE SUA VIEW MOSTRA LucroTotal
            simulacao.LucroTotal = simulacao.LucroLiquido;

            _context.SaveChanges();

            return PartialView("_SimulacaoRow", simulacao);
        }

        // ✅ DELETE
        [HttpPost]
        public IActionResult Delete(int id)
        {
            var simulacao = _context.Simulacao
                .FirstOrDefault(x => x.Id == id);

            if (simulacao == null)
                return NotFound();

            _context.Simulacao.Remove(simulacao);
            _context.SaveChanges();

            return Ok();
        }

        // ✅ TOGGLE STATUS
        [HttpPost]
        public IActionResult ToggleStatusAjax(int id)
        {
            var simulacao = _context.Simulacao
                .FirstOrDefault(x => x.Id == id);

            if (simulacao == null)
                return NotFound();

            simulacao.Status = !simulacao.Status;
            _context.SaveChanges();

            return Ok();
        }

        [HttpPost]
        public IActionResult ConfirmarVenda(int id)
        {
            // ✅ Busca simulação
            var sim = _context.Simulacao.FirstOrDefault(x => x.Id == id);

            if (sim == null)
                return NotFound();

            // ✅ Criar Venda real
            var venda = new Venda
            {
                ClienteId = sim.ClienteId,
                ClienteNome = sim.ClienteNome,

                ProdutoId = sim.ProdutoId,
                ProdutoNome = sim.ProdutoNome,

                QuantidadeGramas = sim.QuantidadeGramas,

                PrecoVenda = sim.PrecoVenda,
                CustoTotal = sim.CustoTotal,
                LucroTotal = sim.LucroTotal,

                ValorComissaoVendedora = sim.ValorComissaoVendedora ?? 0,
                ValorComissaoCabeleireira = sim.ValorComissaoCabeleireira ?? 0,

                DataVenda = DateTime.Now
            };

            _context.Vendas.Add(venda);

            // ✅ Baixar estoque do produto
            var produto = _context.Produtos.FirstOrDefault(p => p.Id == sim.ProdutoId);

            if (produto != null)
            {
                produto.EstoqueGramas -= sim.QuantidadeGramas;
            }

            decimal quantidadeRestante = sim.QuantidadeGramas;

            var compras = _context.Compras
                .Where(c => c.ProdutoId == sim.ProdutoId && c.QuantidadeGramas > 0)
                .OrderBy(c => c.DataCompra)
                .ToList();

            foreach (var c in compras)
            {
                if (quantidadeRestante <= 0)
                    break;

                if (c.QuantidadeGramas >= quantidadeRestante)
                {
                    c.QuantidadeGramas -= quantidadeRestante;
                    quantidadeRestante = 0;
                }
                else
                {
                    quantidadeRestante -= c.QuantidadeGramas.Value;
                    c.QuantidadeGramas = 0;
                }
            }

            if (quantidadeRestante > 0)
                return BadRequest("Estoque insuficiente para concluir a venda.");


            // ✅ Marcar simulação como vendida
            sim.Status = false;

           

            if (venda.ValorComissaoVendedora > 0)
            {
                _context.Comissoes.Add(new Comissao
                {
                    VendaId = venda.Id,
                    Profissional = "Vendedora",
                    Valor = venda.ValorComissaoVendedora,
                    Pago = false
                });
            }

            if (venda.ValorComissaoCabeleireira > 0)
            {
                _context.Comissoes.Add(new Comissao
                {
                    VendaId = venda.Id,
                    Profissional = "Cabeleireira",
                    Valor = venda.ValorComissaoCabeleireira,
                    Pago = false
                });
            }
            _context.SaveChanges();
            var mes = DateTime.Now.Month;
            var ano = DateTime.Now.Year;

            return Json(new
            {
                redirectUrl = Url.Action("Index", "Venda", new { mes, ano })
            });
        }
    }
}
