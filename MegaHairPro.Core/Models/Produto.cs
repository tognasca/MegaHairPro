using System;
using System.Linq;

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
    public DateTime DataCadastro { get; set; } = DateTime.UtcNow;
    public bool Status { get; set; } = true;
    public decimal EstoqueGramas { get; set; }

    public string ObterDescricao() => FormatarDescricao(TipoProduto, Descricao, Origem, Cor, Tamanho);

    public static string FormatarDescricao(TipoProduto tipo, string? descricao, string? origem, string? cor, string? tamanho)
    {
        if (tipo != global::MegaHairPro.Web.Models.TipoProduto.Cabelo)
            return string.IsNullOrWhiteSpace(descricao) ? tipo.ToString() : descricao.Trim();

        if (new[] { origem, cor, tamanho }.All(string.IsNullOrWhiteSpace) && !string.IsNullOrWhiteSpace(descricao))
            return descricao.Trim();

        return string.Join(" ", new[] { "Cabelo", origem, cor, tamanho }
            .Where(valor => !string.IsNullOrWhiteSpace(valor))
            .Select(valor => valor!.Trim()));
    }
}