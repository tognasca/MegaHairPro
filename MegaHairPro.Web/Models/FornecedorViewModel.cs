using MegaHairPro.Web.Models;
using System.Collections.Generic;
namespace MegaHairPro.Web.Models
{
    public class FornecedorViewModel
    {
        public List<Fornecedor> Fornecedores { get; set; } = new();
        public Fornecedor NovoFornecedor { get; set; } = new();
        public string SearchTerm { get; set; } = "";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalPages { get; set; } = 1;
    }
}