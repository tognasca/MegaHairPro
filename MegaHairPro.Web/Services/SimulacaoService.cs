using MegaHairPro.Web.Data;
using MegaHairPro.Web.Models;
using MegaHairPro.Web.Services.Interfaces;
using System;
using System.Linq;

namespace MegaHairPro.Web.Services
{
    public class SimulacaoService : ISimulacaoService
    {
        private readonly AppDbContext _context;

        public SimulacaoService(AppDbContext context)
        {
            _context = context;
        }

        public SimulacaoVenda Calcular(SimulacaoVenda model)
        {
            // ================= ESTOQUE =================
            var produto = _context.Produtos.FirstOrDefault(p => p.Id == model.ProdutoId)
                ?? throw new Exception("Produto não encontrado.");

            model.EstoqueAtualGramas = produto.EstoqueGramas;
            model.EstoquePosVendaGramas = produto.EstoqueGramas - model.QuantidadeGramas;

            if (model.QuantidadeGramas > model.EstoqueAtualGramas)
                throw new Exception("Quantidade maior que o estoque disponível.");

            // ================= CUSTO MÉDIO CABELO =================
            var compras = _context.Compras
                .Where(c => c.ProdutoId == model.ProdutoId && c.QuantidadeGramas > 0)
                .ToList();

            if (!compras.Any())
                throw new Exception("Produto sem histórico de compras.");

            var totalValor = compras.Sum(c => c.ValorTotal);
            var totalGramas = compras.Sum(c => c.QuantidadeGramas!.Value);

            var custoMedioGrama = totalValor / totalGramas;
            model.CustoCabelo = custoMedioGrama * model.QuantidadeGramas;

            // ================= LUCRO CABELO =================
            model.ValorLucroCabelo = model.CustoCabelo * (model.PercentualLucroCabelo / 100m);

            var precoCabelo = model.CustoCabelo + model.ValorLucroCabelo;

            // ================= CONFECÇÃO =================
            decimal valorPorGramaMetodo = 0;
            if (model.MetodoId > 0)
            {
                var metodo = _context.MetodosAplicacao.FirstOrDefault(x => x.Id == model.MetodoId);
                if (metodo != null)
                    valorPorGramaMetodo = metodo.ValorPorGrama;
            }
            model.ValorTotalConfeccao = valorPorGramaMetodo * model.QuantidadeGramas;

            // ================= SERVIÇOS ADICIONAIS =================
            model.CustoServico = model.MaoDeObra + model.Tinta + model.CorteEscova;

            // ================= PREÇO CHEIO =================
            model.PrecoVendaOriginal =
                precoCabelo +
                model.ValorTotalConfeccao +
                model.CustoServico +
                model.DespesaSalaoRateada;

            // ================= DESCONTO =================
            if (model.PercentualDesconto > 0)
            {
                model.ValorDesconto = model.PrecoVendaOriginal * (model.PercentualDesconto / 100m);
                model.PrecoVenda = model.PrecoVendaOriginal - model.ValorDesconto;
            }
            else
            {
                model.ValorDesconto = 0;
                model.PrecoVenda = model.PrecoVendaOriginal;
            }

            // ================= COMISSÕES (sobre PrecoVenda) =================
            model.ValorComissaoVendedora =
                (model.PercentualComissaoVendedora ?? 0) * model.PrecoVenda / 100m;

            model.ValorComissaoCabeleireira =
                (model.PercentualComissaoCabeleireira ?? 0) * model.PrecoVenda / 100m;

            // ================= CUSTO TOTAL =================
            model.CustoTotal =
                model.CustoCabelo +
                model.ValorTotalConfeccao +
                model.CustoServico +
                model.DespesaSalaoRateada +
                model.ValorComissaoVendedora.Value +
                model.ValorComissaoCabeleireira.Value;

            // ================= LUCRO =================
            model.LucroCabelo = precoCabelo - model.CustoCabelo;   // lucro só do cabelo
            model.LucroLiquido = model.PrecoVenda - model.CustoTotal;
            model.LucroTotal = model.LucroLiquido;
            model.LucroVenda = model.LucroLiquido;

            // ================= MARGEM % =================
            model.MargemPercentual = model.PrecoVenda > 0
                ? (model.LucroLiquido / model.PrecoVenda) * 100m
                : 0;

            return model;
        }
    }
}