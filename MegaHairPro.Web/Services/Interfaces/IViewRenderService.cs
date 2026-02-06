
using System.Threading.Tasks;

namespace MegaHairPro.Web.Services.Interfaces
{
    public interface IViewRenderService
    {
        Task<string> RenderToStringAsync(string viewName, object model);
    }
}
