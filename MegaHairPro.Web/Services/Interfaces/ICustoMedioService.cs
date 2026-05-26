
using MegaHairPro.Web.Models;
using System.Collections.Generic;
namespace MegaHairPro.Web.Services.Interfaces;

public interface ICustoMedioService
{
    List<Dictionary<string, object>> CalculaCustoCabelo(int produtoId, decimal quantidadeGramas, decimal percentualLucro);
}