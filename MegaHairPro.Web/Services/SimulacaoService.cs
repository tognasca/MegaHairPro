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
            // ================= PRODUTO / ESTOQUE =================
            var compra = _context.Compras
                .FirstOrDefault(p => p.ProdutoId == model.ProdutoId)
                ?? throw new Exception("Produto na compra não encontrado.");

            model.EstoqueAtualGramas = compra.QuantidadeGramas ?? 0;

            if (model.QuantidadeGramas > model.EstoqueAtualGramas)
                throw new Exception("Quantidade maior que o estoque disponível.");

            model.EstoquePosVendaGramas =
                (compra.QuantidadeGramas - model.QuantidadeGramas) ?? 0;

            // ================= CUSTO MÉDIO CABELO =================
            var compras = _context.Compras
                .Where(c => c.ProdutoId == model.ProdutoId &&
                            c.QuantidadeGramas > 0)
                .ToList();

            if (!compras.Any())
                throw new Exception("Produto sem histórico de compras.");

            var totalValor = compras.Sum(c => c.ValorTotal);
            var totalGramas = compras.Sum(c => c.QuantidadeGramas!.Value);

            var custoMedioGrama = totalValor / totalGramas;

            model.CustoCabelo =
                custoMedioGrama * model.QuantidadeGramas;

            // ================= LUCRO CABELO =================
            model.ValorLucroCabelo =
                model.CustoCabelo *
                (model.PercentualLucroCabelo / 100m);

            var precoCabelo =
                model.CustoCabelo +
                model.ValorLucroCabelo;

            // =====================================================
            // 🔥 NOVO BLOCO — CONFECÇÃO POR MÉTODO
            // =====================================================

            decimal valorPorGramaMetodo = 0;

            if (model.MetodoId > 0)
            {
                var metodo = _context.MetodosAplicacao
                    .FirstOrDefault(x => x.Id == model.MetodoId);

                if (metodo != null)
                {
                    valorPorGramaMetodo =
                        metodo.ValorPorGrama;
                }
            }

            model.ValorTotalConfeccao =
                valorPorGramaMetodo *
                model.QuantidadeGramas;

            // =====================================================
            // SERVIÇOS ADICIONAIS (mantido)
            // =====================================================

            model.CustoServico =
                model.MaoDeObra +
                model.Tinta +
                model.CorteEscova;

            // =====================================================
            // PREÇO CHEIO (SEM DESCONTO)
            // =====================================================

            model.PrecoVendaOriginal =
                precoCabelo +
                model.ValorTotalConfeccao +   // 🔥 entrou aqui
                model.CustoServico +
                model.DespesaSalaoRateada;

            model.PrecoVenda =
                model.PrecoVendaOriginal;

            // ================= DESCONTO =================
            if (model.PercentualDesconto > 0)
            {
                model.ValorDesconto =
                    model.PrecoVendaOriginal *
                    (model.PercentualDesconto / 100m);

                model.PrecoVenda =
                    model.PrecoVendaOriginal -
                    model.ValorDesconto;
            }
            else
            {
                model.ValorDesconto = 0;
            }

            // ================= COMISSÕES =================
            model.ValorComissaoVendedora =
                (model.PercentualComissaoVendedora ?? 0) *
                precoCabelo / 100m;

            model.ValorComissaoCabeleireira =
                (model.PercentualComissaoCabeleireira ?? 0) *
                precoCabelo / 100m;

            // ================= CUSTO TOTAL =================
            model.CustoTotal =
                model.CustoCabelo +
                model.ValorTotalConfeccao +   // 🔥 entrou aqui
                model.CustoServico +
                model.DespesaSalaoRateada +
                model.ValorComissaoVendedora.Value +
                model.ValorComissaoCabeleireira.Value;

            // ================= LUCRO CABELO =================
            model.LucroCabelo =
                precoCabelo - model.CustoCabelo;

            // ================= LUCRO VENDA =================
            model.LucroVenda =
                (precoCabelo +
                 model.ValorTotalConfeccao +
                 model.CustoServico) -
                (model.CustoCabelo +
                 model.ValorTotalConfeccao +
                 model.CustoServico);

            // ================= LUCRO LÍQUIDO =================
            model.LucroLiquido =
                model.PrecoVenda -
                model.CustoTotal;

            model.LucroTotal =
                model.LucroLiquido;

            // ================= MARGEM % =================
            if (model.PrecoVenda > 0)
                model.MargemPercentual =
                    (model.LucroLiquido /
                     model.PrecoVenda) * 100m;
            else
                model.MargemPercentual = 0;

            return model;
        }
    }
}
