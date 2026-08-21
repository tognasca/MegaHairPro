using MegaHairPro.Web.Data;
using MegaHairPro.Web.Services.Interfaces;
using System.Collections.Generic;
using System.Linq;

public class CustoMedioService : ICustoMedioService
{
    private readonly AppDbContext _context;

    public CustoMedioService(AppDbContext context) 
    {
        _context = context;
    }
    public List<Dictionary<string, object>> CalculaCustoCabelo(int produtoId, decimal quantidadeGramas, decimal percentualLucro)
    {
        if (produtoId <= 0 || quantidadeGramas <= 0)
            return new List<Dictionary<string, object>>();

        var compras = _context.Compras
            .Where(c => c.ProdutoId == produtoId && c.QuantidadeGramas > 0)
            .ToList();

        if (!compras.Any()) return new List<Dictionary<string, object>>();

        var custoMedio = compras.Sum(c => c.ValorTotal) / compras.Sum(c => c.QuantidadeGramas!.Value);
        var custo = custoMedio * quantidadeGramas;
        var lucro = custo * (percentualLucro / 100m);

        return [new Dictionary<string, object> { ["custo"] = custo, ["lucro"] = lucro }] ;
    }
}
