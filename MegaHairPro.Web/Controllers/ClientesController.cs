using MegaHairPro.Web.Data;
using MegaHairPro.Web.Models;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace MegaHairPro.Web.Controllers
{
    public class ClientesController : Controller
    {
        private readonly AppDbContext _context;

        public ClientesController(AppDbContext context)
        {
            _context = context;
        }

        // ✅ LISTAGEM
        [HttpGet]
        public IActionResult Index()
        {
            var clientes = _context.Clientes
                .OrderByDescending(c => c.Id)
                .ToList();

            return View(clientes);
        }

        // ✅ CREATE
        [HttpPost]
        //[ValidateAntiForgeryToken]
        public IActionResult Index(Cliente cliente)
        {
            cliente.Status = true;

            _context.Clientes.Add(cliente);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        // ✅ EDIT AJAX
        [HttpPost]
        public IActionResult EditAjax(Cliente cliente)
        {
            var original = _context.Clientes.Find(cliente.Id);

            if (original == null)
                return Json(new { success = false });

            original.Nome = cliente.Nome;
            original.Telefone = cliente.Telefone;
            original.WhatsApp = cliente.WhatsApp;
            original.Email = cliente.Email;
            original.CPF = cliente.CPF;
            original.DataNascimento = cliente.DataNascimento;
            original.Endereco = cliente.Endereco;
            original.Observacoes = cliente.Observacoes;

            _context.SaveChanges();

            return PartialView("_ClienteRow", original);
        }

        // ✅ ATIVAR / INATIVAR
        [HttpPost]
        public IActionResult ToggleStatusAjax(int id)
        {
            var cliente = _context.Clientes.Find(id);

            if (cliente == null)
                return Json(new { success = false });

            cliente.Status = !cliente.Status;
            _context.SaveChanges();

            return Json(new { success = true });
        }

        // ✅ DELETE
        [HttpPost]
        public IActionResult Delete(int id)
        {
            var cliente = _context.Clientes.Find(id);

            if (cliente == null)
                return RedirectToAction("Index");

            _context.Clientes.Remove(cliente);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }
    }
}
