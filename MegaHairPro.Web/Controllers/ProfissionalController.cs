
using MegaHairPro.Web.Data;
using MegaHairPro.Web.Models;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace MegaHairPro.Web.Controllers
{
    public class ProfissionalController : Controller
    {
        private readonly AppDbContext _context;

        public ProfissionalController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index() => View(_context.Profissionais.ToList());

        public IActionResult Create() => View();

        [HttpPost]
        public IActionResult Create(Profissional p)
        {
            _context.Add(p);
            _context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Delete(int id)
        {
            var p = _context.Profissionais.Find(id);
            _context.Remove(p);
            _context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }
    }
}
