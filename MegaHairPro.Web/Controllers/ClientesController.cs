            using MegaHairPro.Web.Data;
using MegaHairPro.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System.IO;
using System.Linq;

namespace MegaHairPro.Web.Controllers
{
    public class ClientesController : Controller
    {
        private readonly AppDbContext _context;
        private readonly ICompositeViewEngine _viewEngine;

        public ClientesController(AppDbContext context, ICompositeViewEngine viewEngine)
        {
            _context = context;
            _viewEngine = viewEngine;
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
        [ValidateAntiForgeryToken]
        public IActionResult Index(Cliente cliente)
        {
            // server-side required fields
            if (cliente == null
                || string.IsNullOrWhiteSpace(cliente.Nome)
                || string.IsNullOrWhiteSpace(cliente.CPF)
                || string.IsNullOrWhiteSpace(cliente.WhatsApp))
            {
                TempData["Error"] = "Preencha Nome, CPF e WhatsApp";
                return RedirectToAction("Index");
            }

            // validar CPF (algoritmo) e WhatsApp (10 ou 11 dígitos)
            if (!IsValidCPF(cliente.CPF))
            {
                TempData["Error"] = "CPF inválido";
                return RedirectToAction("Index");
            }

            var whatsDigits = OnlyDigits(cliente.WhatsApp);
            if (whatsDigits.Length < 10 || whatsDigits.Length > 11)
            {
                TempData["Error"] = "WhatsApp inválido";
                return RedirectToAction("Index");
            }

            cliente.Status = true;

            _context.Clientes.Add(cliente);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        private static string OnlyDigits(string s)
        {
            if (string.IsNullOrEmpty(s)) return string.Empty;
            var arr = s.Where(char.IsDigit).ToArray();
            return new string(arr);
        }

        private static bool IsValidCPF(string cpf)
        {
            var d = OnlyDigits(cpf);
            if (d.Length != 11) return false;
            if (System.Text.RegularExpressions.Regex.IsMatch(d, "^(\\d)\\1{10}$")) return false;
            var nums = d.Select(c => c - '0').ToArray();
            int sum = 0;
            for (int i = 0; i < 9; i++) sum += nums[i] * (10 - i);
            int r = (sum * 10) % 11;
            if (r == 10) r = 0;
            if (r != nums[9]) return false;
            sum = 0;
            for (int i = 0; i < 10; i++) sum += nums[i] * (11 - i);
            r = (sum * 10) % 11;
            if (r == 10) r = 0;
            return r == nums[10];
        }

        // ✅ EDIT AJAX
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditAjax(Cliente cliente)
        {
            // basic server-side validation
            if (cliente == null
                || string.IsNullOrWhiteSpace(cliente.Nome)
                || string.IsNullOrWhiteSpace(cliente.CPF)
                || string.IsNullOrWhiteSpace(cliente.WhatsApp))
            {
                return BadRequest(new { success = false, message = "Preencha Nome, CPF e WhatsApp" });
            }

            if (!IsValidCPF(cliente.CPF))
                return BadRequest(new { success = false, message = "CPF inválido" });

            var whatsDigits = OnlyDigits(cliente.WhatsApp);
            if (whatsDigits.Length < 10 || whatsDigits.Length > 11)
                return BadRequest(new { success = false, message = "WhatsApp inválido" });

            var original = _context.Clientes.Find(cliente.Id);

            if (original == null)
                return BadRequest(new { success = false, message = "Cliente não encontrado" });

            original.Nome = cliente.Nome;
            original.Telefone = cliente.Telefone;
            original.WhatsApp = cliente.WhatsApp;
            original.Email = cliente.Email;
            original.CPF = cliente.CPF;
            original.DataNascimento = cliente.DataNascimento;
            original.Endereco = cliente.Endereco;
            original.Observacoes = cliente.Observacoes;

            _context.SaveChanges();

            // render updated row
            var html = RenderPartialViewToString("_ClienteRow", original);
            return Json(new { success = true, message = "Cliente editado com sucesso", html });
        }

        private string RenderPartialViewToString(string viewName, object model)
        {
            ViewData.Model = model;
            using (var sw = new StringWriter())
            {
                var viewResult = _viewEngine.FindView(ControllerContext, viewName, false);
                if (viewResult.View == null)
                {
                    throw new FileNotFoundException($"View {viewName} not found.");
                }

                var viewContext = new ViewContext(ControllerContext, viewResult.View, ViewData, TempData, sw, new HtmlHelperOptions());
                viewResult.View.RenderAsync(viewContext).GetAwaiter().GetResult();
                return sw.GetStringBuilder().ToString();
            }
        }

        // ✅ ATIVAR / INATIVAR
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ToggleStatusAjax(int id)
        {
            if (id <= 0)
                return BadRequest("Id inválido");

            var cliente = _context.Clientes.Find(id);

            if (cliente == null)
                return BadRequest("Cliente não encontrado");

            cliente.Status = !cliente.Status;
            _context.SaveChanges();

            return Json(new { success = true });
        }

        // ✅ DELETE
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            if (id <= 0)
                return BadRequest("Id inválido");

            var cliente = _context.Clientes.Find(id);

            if (cliente == null)
                return BadRequest("Cliente não encontrado");

            _context.Clientes.Remove(cliente);
            _context.SaveChanges();

            return Json(new { success = true });
        }
    }
}
