
namespace MegaHairPro.Web.Services.Interfaces
{
    public interface IPdfService
    {
        byte[] GerarPdf(string html);
    }
}
