using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace MegaHairPro.Web.Models
{
    public class SimulacaoVenda
    {
        public int Id { get; set; }
        // ================= CLIENTES =================
        public int ClienteId { get; set; }
        public string ClienteNome { get; set; } = "";
        public string ProdutoNome { get; set; } = "";

        // ================= PRODUTO / ESTOQUE =================
        public int ProdutoId { get; set; }
        public decimal QuantidadeGramas { get; set; }

        // ESTOQUE VEM DO PRODUTO (SERVICE)
        public decimal EstoqueAtualGramas { get; set; }
        public decimal EstoquePosVendaGramas { get; set; }

        // ================= CABELO =================
        public decimal? CustoCabelo { get; set; }

        public decimal CustoServico { get; set; }


        // LUCRO SOMENTE NO CABELO
        public decimal PercentualLucroCabelo { get; set; }
        public decimal? ValorLucroCabelo { get; set; }

        // ================= DESPESAS DO SERVIÇO =================
        public decimal MaoDeObra { get; set; }
        public decimal Tinta { get; set; }
        public decimal CorteEscova { get; set; }

        // ================= DESPESA DO SALÃO =================
        public decimal DespesaSalaoRateada { get; set; }

        // ================= COMISSÕES =================
        public decimal? PercentualComissaoVendedora { get; set; }
        public decimal? ValorComissaoVendedora { get; set; }
        public decimal? PercentualComissaoCabeleireira { get; set; }
        public decimal? ValorComissaoCabeleireira { get; set; }

        // ================= RESULTADOS =================
        public decimal? PrecoVenda { get; set; }
        public decimal? CustoTotal { get; set; }
        public decimal? LucroCabelo { get; set; }

        public decimal? LucroVenda { get; set; }



        public decimal? LucroLiquido { get; set; }
        public decimal? LucroTotal { get; set; }
        public DateTime DataCadastro { get; set; } = DateTime.Now;
        public bool Status { get; set; } = true;
        [NotMapped]
        public List<int> DespesasSelecionadas { get; set; } = [];

        [NotMapped]
        public decimal TotalDespesasSelecionadas { get; set; }

        public decimal? PercentualDesconto { get; set; }
        public decimal? ValorDesconto { get; set; }
        public decimal? PrecoVendaOriginal { get; set; }
        public decimal? MargemPercentual { get; set; }

        public string QrCodePixBase64 { get; set; }

        public string LogoCaminho { get; set; }

        public decimal? ValorTotalConfeccao { get; set; }

        public int MetodoId { get; set; }
        public string MetodoNome { get; set; }

        public DateTime DataValidadePix { get; set; } = DateTime.Now.AddDays(7);
        [NotMapped]
        public string Cabelo { get; set; }
        [NotMapped]
        public string Cor { get; set; }
        [NotMapped]
        public string Tamanho { get; set; }

        [NotMapped]
        public string UrlImagem { get; set; }
    }
}
