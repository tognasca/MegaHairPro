using MegaHairPro.Web.Data;
using MegaHairPro.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace MegaHairPro.Web.Controllers
{
    public class FornecedoresController : Controller
    {
        private readonly AppDbContext _context;

        public FornecedoresController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var fornecedores = _context.Fornecedores
                .AsNoTracking()
                .OrderByDescending(f => f.Id)
                .ToList();

            return View(fornecedores);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Index(Fornecedor fornecedor)
        {
            Normalizar(fornecedor);

            if (!TryValidateModel(fornecedor))
            {
                var lista = _context.Fornecedores
                    .AsNoTracking()
                    .OrderByDescending(f => f.Id)
                    .ToList();

                return View(lista);
            }

            fornecedor.Status = true;
            _context.Fornecedores.Add(fornecedor);
            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditAjax(Fornecedor fornecedor)
        {
            Normalizar(fornecedor);

            if (!TryValidateModel(fornecedor))
                return BadRequest(ModelState);

            var original = _context.Fornecedores.Find(fornecedor.Id);
            if (original == null)
                return NotFound();

            original.Nome = fornecedor.Nome;
            original.Contato = fornecedor.Contato;
            original.CNPJ = fornecedor.CNPJ;
            original.InscricaoEstadual = fornecedor.InscricaoEstadual;
            original.RazaoSocial = fornecedor.RazaoSocial;
            original.NomeFantasia = fornecedor.NomeFantasia;
            original.Logradouro = fornecedor.Logradouro;
            original.Numero = fornecedor.Numero;
            original.Complemento = fornecedor.Complemento;
            original.Bairro = fornecedor.Bairro;
            original.Cidade = fornecedor.Cidade;
            original.UF = fornecedor.UF;
            original.CEP = fornecedor.CEP;
            original.Banco = fornecedor.Banco;
            original.Agencia = fornecedor.Agencia;
            original.Conta = fornecedor.Conta;
            original.TipoConta = fornecedor.TipoConta;
            original.Observacoes = fornecedor.Observacoes;

            _context.SaveChanges();

            return PartialView("_FornecedorRow", original);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ToggleStatusAjax(int id)
        {
            var fornecedor = _context.Fornecedores.Find(id);
            if (fornecedor == null)
                return NotFound();

            fornecedor.Status = !fornecedor.Status;
            _context.SaveChanges();

            return PartialView("_FornecedorRow", fornecedor);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            var fornecedor = _context.Fornecedores.Find(id);
            if (fornecedor == null)
                return NotFound();

            _context.Fornecedores.Remove(fornecedor);
            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        private static void Normalizar(Fornecedor f)
        {
            f.Nome = f.Nome?.Trim() ?? "";
            f.Contato = VazioParaNulo(f.Contato) ?? "";
            f.CNPJ = SomenteDigitosOuNulo(f.CNPJ) ?? "";
            f.InscricaoEstadual = VazioParaNulo(f.InscricaoEstadual);
            f.RazaoSocial = VazioParaNulo(f.RazaoSocial);
            f.NomeFantasia = VazioParaNulo(f.NomeFantasia);
            f.Telefone = VazioParaNulo(f.Telefone);
            f.Email = VazioParaNulo(f.Email);
            f.Logradouro = VazioParaNulo(f.Logradouro);
            f.Numero = VazioParaNulo(f.Numero);
            f.Complemento = VazioParaNulo(f.Complemento);
            f.Bairro = VazioParaNulo(f.Bairro);
            f.Cidade = VazioParaNulo(f.Cidade);
            f.UF = VazioParaNulo(f.UF)?.ToUpperInvariant();
            f.CEP = SomenteDigitosOuNulo(f.CEP);
            f.Banco = VazioParaNulo(f.Banco);
            f.Agencia = VazioParaNulo(f.Agencia);
            f.Conta = VazioParaNulo(f.Conta);
            f.TipoConta = VazioParaNulo(f.TipoConta);
            f.Observacoes = VazioParaNulo(f.Observacoes);
        }

        private static string? VazioParaNulo(string? valor)
        {
            var t = valor?.Trim();
            return string.IsNullOrEmpty(t) ? null : t;
        }

        private static string? SomenteDigitosOuNulo(string? valor)
        {
            if (string.IsNullOrWhiteSpace(valor))
                return null;

            var digitos = new string(valor.Where(char.IsDigit).ToArray());
            return digitos.Length == 0 ? null : digitos;
        }
    }
}
