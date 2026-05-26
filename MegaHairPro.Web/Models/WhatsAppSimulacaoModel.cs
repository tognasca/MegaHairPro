using System.Collections.Generic;

namespace MegaHairPro.Web.Models
{
    public class WhatsAppSimulacaoModel
    {
        public string ClienteNome { get; set; }
        public string ClienteTelefone { get; set; }
        public int ProdutoId { get; set; }
        public int MetodoId { get; set; }
        public decimal QuantidadeGramas
        {
            get;
            set;
        }
        public List<int> DespesasSelecionadas
        {
            get;
            set;
        }
    }
}
