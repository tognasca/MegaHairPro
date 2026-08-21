using System;
using System.Collections.Generic;

namespace MegaHairPro.Web.Models;

public class Produto
{
    public int Id { get; set; }
    // 👉 CÓDIGO DO PRODUTO (SKU / Código interno)
    public string? Codigo { get; set; }
    public string? Descricao { get; set; }
    public TipoProduto TipoProduto { get; set; }
    public UnidadeMedida UnidadeMedida { get; set; } = UnidadeMedida.Grama;
    // ===== CAMPOS ESPECÍFICOS DE CABELO =====
    public string? Origem { get; set; }
    public string? Tamanho { get; set; }
    public string? Cor { get; set; }
    // ===== FORNECEDOR =====
    public int? FornecedorId { get; set; }
    public Fornecedor? Fornecedor { get; set; }
    public DateTime DataCadastro { get; set; } = DateTime.Now;
    public bool Status { get; set; } = true;
    public decimal EstoqueGramas { get; set; }
}