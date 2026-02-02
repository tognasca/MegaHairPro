
using System;
namespace MegaHairPro.Web.Domain.Rules;
public static class EstoqueRules
{
    public static void Validar(decimal estoqueAtual, decimal quantidade)
    {
        if (quantidade <= 0)
            throw new Exception("Quantidade inválida.");
        if (estoqueAtual < quantidade)
            throw new Exception("Estoque insuficiente.");
    }
}
