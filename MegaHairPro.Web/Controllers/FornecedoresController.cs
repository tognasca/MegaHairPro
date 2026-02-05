using MegaHairPro.Web.Data;
using MegaHairPro.Web.Models;
using Microsoft.AspNetCore.Mvc;
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

        // ✅ LISTAGEM
        [HttpGet]
        public IActionResult Index()
        {
            var fornecedores = _context.Fornecedores
                .OrderByDescending(f => f.Id)
                .ToList();

            return View(fornecedores);
        }

        // ✅ CREATE
        [HttpPost]
        ////[ValidateAntiForgeryToken]
        public IActionResult Index(Fornecedor fornecedor)
        {
            fornecedor.Status = true;

            _context.Fornecedores.Add(fornecedor);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        // ✅ EDIT AJAX COMPLETO
        [HttpPost]
        public IActionResult EditAjax(Fornecedor fornecedor)
        {
            var original = _context.Fornecedores.Find(fornecedor.Id);

            if (original == null)
                return Json(new { success = false });

            // ✅ Dados principais
            original.Nome = fornecedor.Nome;
            original.Contato = fornecedor.Contato;
            original.CNPJ = fornecedor.CNPJ;
            original.InscricaoEstadual = fornecedor.InscricaoEstadual;
            original.RazaoSocial = fornecedor.RazaoSocial;
            original.NomeFantasia = fornecedor.NomeFantasia;

            // ✅ Endereço
            original.Logradouro = fornecedor.Logradouro;
            original.Numero = fornecedor.Numero;
            original.Complemento = fornecedor.Complemento;
            original.Bairro = fornecedor.Bairro;
            original.Cidade = fornecedor.Cidade;
            original.UF = fornecedor.UF;
            original.CEP = fornecedor.CEP;

            // ✅ Dados bancários
            original.Banco = fornecedor.Banco;
            original.Agencia = fornecedor.Agencia;
            original.Conta = fornecedor.Conta;
            original.TipoConta = fornecedor.TipoConta;

            // ✅ Observações
            original.Observacoes = fornecedor.Observacoes;

            _context.SaveChanges();

            return PartialView("_FornecedorRow", original);
        }

        // ✅ ATIVAR / INATIVAR
        [HttpPost]
        public IActionResult ToggleStatusAjax(int id)
        {
            var fornecedor = _context.Fornecedores.Find(id);

            if (fornecedor == null)
                return Json(new { success = false });

            fornecedor.Status = !fornecedor.Status;
            _context.SaveChanges();

            return Json(new { success = true });
        }

        // ✅ DELETE
        [HttpPost]
        public IActionResult Delete(int id)
        {
            var fornecedor = _context.Fornecedores.Find(id);

            if (fornecedor == null)
                return RedirectToAction("Index");

            _context.Fornecedores.Remove(fornecedor);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }
    }
}
