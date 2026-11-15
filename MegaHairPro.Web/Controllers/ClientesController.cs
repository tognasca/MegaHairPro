using MegaHairPro.Web.Data;
using MegaHairPro.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
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

        [HttpGet]
        public IActionResult Index()
        {
            var clientes = _context.Clientes
                .AsNoTracking()
                .OrderByDescending(c => c.Id)
                .ToList();

            return View(clientes);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Index(Cliente cliente)
        {
            Normalizar(cliente);

            if (!ValidarCliente(cliente, out var erro))
            {
                TempData["Error"] = erro;
                var lista = _context.Clientes
                    .AsNoTracking()
                    .OrderByDescending(c => c.Id)
                    .ToList();
                return View(lista);
            }

            cliente.Status = true;
            _context.Clientes.Add(cliente);
            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        private static string OnlyDigits(string? s)
        {
            if (string.IsNullOrEmpty(s)) return string.Empty;
            var arr = s.Where(char.IsDigit).ToArray();
            return new string(arr);
        }

        private static bool IsValidCPF(string? cpf)
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditAjax(Cliente cliente)
        {
            Normalizar(cliente);

            if (!ValidarCliente(cliente, out var erro))
                return BadRequest(new { success = false, message = erro });

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

            var html = RenderPartialViewToString("_ClienteRow", cliente);
            return Json(new { success = true, html });
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

        private static void Normalizar(Cliente c)
        {
            c.Nome = VazioParaNulo(c.Nome);
            c.Telefone = DigitosOuNulo(c.Telefone);
            c.WhatsApp = DigitosOuNulo(c.WhatsApp);
            c.Email = VazioParaNulo(c.Email);
            c.CPF = DigitosOuNulo(c.CPF);
            c.Endereco = VazioParaNulo(c.Endereco);
            c.Observacoes = VazioParaNulo(c.Observacoes);
        }

        private static bool ValidarCliente(Cliente cliente, out string erro)
        {
            if (cliente == null || string.IsNullOrWhiteSpace(cliente.Nome))
            {
                erro = "Preencha o nome";
                return false;
            }

            if (string.IsNullOrWhiteSpace(cliente.CPF) || !IsValidCPF(cliente.CPF))
            {
                erro = "CPF invalido";
                return false;
            }

            var whats = OnlyDigits(cliente.WhatsApp);
            if (whats.Length < 10 || whats.Length > 11)
            {
                erro = "WhatsApp invalido";
                return false;
            }

            if (!string.IsNullOrWhiteSpace(cliente.Email) &&
                !new EmailAddressAttribute().IsValid(cliente.Email))
            {
                erro = "E-mail invalido";
                return false;
            }

            erro = "";
            return true;
        }

        private static string? VazioParaNulo(string? valor)
        {
            var t = valor?.Trim();
            return string.IsNullOrEmpty(t) ? null : t;
        }

        private static string? DigitosOuNulo(string? valor)
        {
            var d = OnlyDigits(valor);
            return d.Length == 0 ? null : d;
        }
    }
}
