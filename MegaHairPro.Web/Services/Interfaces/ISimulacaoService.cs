using MegaHairPro.Web.Models;

namespace MegaHairPro.Web.Services.Interfaces;
public interface ISimulacaoService 
{ 
    SimulacaoVenda Calcular(SimulacaoVenda model); 
}