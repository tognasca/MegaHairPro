using MegaHairPro.Web.Data;
using MegaHairPro.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace MegaHairPro.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ProfissionalController : Controller
    {
        private readonly AppDbContext _context;

        public ProfissionalController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var lista = _context.Profissionais.OrderBy(p => p.Nome).ToList();
            return View(lista);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateAjax(Profissional model)
        {
            if (string.IsNullOrWhiteSpace(model.Nome))
                return BadRequest("Nome obrigatório.");

            _context.Profissionais.Add(model);
            _context.SaveChanges();
            return PartialView("_ProfissionalRow", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditAjax(Profissional model)
        {
            var original = _context.Profissionais.Find(model.Id);
            if (original == null) return NotFound();

            original.Nome         = model.Nome;
            original.Especialidade = model.Especialidade;
            _context.SaveChanges();
            return PartialView("_ProfissionalRow", original);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            var profissional = _context.Profissionais.Find(id);
            if (profissional == null) return Ok();

            // Verifica se tem agendamentos vinculados
            bool temAgendamentos = _context.Agendamentos.Any(a => a.ProfissionalId == id);
            if (temAgendamentos)
                return BadRequest("Profissional possui agendamentos e não pode ser excluído.");

            _context.Profissionais.Remove(profissional);
            _context.SaveChanges();
            return Ok();
        }
    }
}
