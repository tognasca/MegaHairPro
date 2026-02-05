using Microsoft.AspNetCore.Mvc;

namespace MegaHairPro.Web.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
