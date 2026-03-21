using MegaHairPro.Web.Data;
using MegaHairPro.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using System.Linq;

namespace MegaHairPro.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ServicoController : Controller
    {
        private readonly AppDbContext _context;

        public ServicoController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var lista = _context.Servicos.OrderBy(s => s.Nome).ToList();
            return View(lista);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateAjax(Servico model)
        {
            if (Request.Form.TryGetValue("Valor", out var valorStr))
                if (decimal.TryParse(valorStr, NumberStyles.Any, CultureInfo.InvariantCulture, out var v))
                    model.Valor = v;

            if (Request.Form.TryGetValue("ValorPromocional", out var promStr) && !string.IsNullOrWhiteSpace(promStr))
                if (decimal.TryParse(promStr, NumberStyles.Any, CultureInfo.InvariantCulture, out var vp))
                    model.ValorPromocional = vp > 0 ? vp : null;

            _context.Servicos.Add(model);
            _context.SaveChanges();
            return PartialView("_ServicoRow", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditAjax(Servico model)
        {
            var original = _context.Servicos.Find(model.Id);
            if (original == null) return NotFound();

            original.Nome          = model.Nome;
            original.Descricao     = model.Descricao;
            original.Categoria     = model.Categoria;
            original.DuracaoMinutos = model.DuracaoMinutos;

            if (Request.Form.TryGetValue("Valor", out var valorStr))
                if (decimal.TryParse(valorStr, NumberStyles.Any, CultureInfo.InvariantCulture, out var v))
                    original.Valor = v;

            if (Request.Form.TryGetValue("ValorPromocional", out var promStr) && !string.IsNullOrWhiteSpace(promStr))
                if (decimal.TryParse(promStr, NumberStyles.Any, CultureInfo.InvariantCulture, out var vp))
                    original.ValorPromocional = vp > 0 ? vp : null;
                else
                    original.ValorPromocional = null;

            _context.SaveChanges();
            return PartialView("_ServicoRow", original);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            var servico = _context.Servicos.Find(id);
            if (servico == null) return Ok();

            bool temAgendamentos = _context.AgendamentoServicos.Any(a => a.ServicoId == id);
            if (temAgendamentos)
                return BadRequest("Serviço possui agendamentos e não pode ser excluído.");

            _context.Servicos.Remove(servico);
            _context.SaveChanges();
            return Ok();
        }
    }
}
