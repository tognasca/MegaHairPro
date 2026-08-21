
using MegaHairPro.Web.Models;
namespace MegaHairPro.Web.Services.Interfaces;
public interface ICompraService
{
    void Criar(Compra compra);
    void Editar(Compra compra);
    void Excluir(int id);
}
