using MegaHairPro.Web.Data;
using MegaHairPro.Web.Models;
using MegaHairPro.Web.Services.Interfaces;
using Microsoft.Identity.Client;
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
            // ================= PRODUTO / ESTOQUE =================
            var compra = _context.Compras
                .FirstOrDefault(p => p.ProdutoId == model.ProdutoId)
                ?? throw new Exception("Produto na compra não encontrado.");

            model.EstoqueAtualGramas = compra.QuantidadeGramas ?? 0;

            if (model.QuantidadeGramas > model.EstoqueAtualGramas)
                throw new Exception("Quantidade maior que o estoque disponível.");

            model.EstoquePosVendaGramas =
                (compra.QuantidadeGramas - model.QuantidadeGramas) ?? 0;

            // ================= CUSTO DO CABELO (CORRETO) =================
            var compras = _context.Compras
                .Where(c => c.ProdutoId == model.ProdutoId && c.QuantidadeGramas > 0)
                .ToList();

            if (!compras.Any())
                throw new Exception("Produto sem histórico de compras.");

            var totalValor = compras.Sum(c => c.ValorTotal);
            var totalGramas = compras.Sum(c => c.QuantidadeGramas!.Value);

            var custoMedioGrama = totalValor / totalGramas;

            var custoCabelo = custoMedioGrama * model.QuantidadeGramas;
            model.CustoCabelo = custoCabelo;

            // ================= LUCRO DO CABELO =================
            var lucroCabelo = custoCabelo * (model.PercentualLucroCabelo / 100m);
            model.ValorLucroCabelo = lucroCabelo;

            var precoCabelo = custoCabelo + lucroCabelo;

            // ================= DESPESAS DO SERVIÇO =================
            var custoServico =
                model.MaoDeObra +
                model.Tinta +
                model.CorteEscova;

            // ================= PREÇO DE VENDA =================
            model.PrecoVenda =
                precoCabelo +
                custoServico +
                model.DespesaSalaoRateada;

            // ================= COMISSÕES =================
            model.ValorComissaoVendedora =
                (model.PercentualComissaoVendedora ?? 0) * model.PrecoVenda / 100m;

            model.ValorComissaoCabeleireira =
                (model.PercentualComissaoCabeleireira ?? 0) * model.PrecoVenda / 100m;

            // ================= CUSTO TOTAL =================
            model.CustoTotal =
                custoCabelo +
                custoServico +
                model.DespesaSalaoRateada +
                model.ValorComissaoVendedora.Value +
                model.ValorComissaoCabeleireira.Value;

            // ================= LUCRO LÍQUIDO =================
            model.LucroLiquido =
                model.PrecoVenda - model.CustoTotal;

            model.LucroTotal = model.LucroLiquido;

            return model;
        }
    }
}
