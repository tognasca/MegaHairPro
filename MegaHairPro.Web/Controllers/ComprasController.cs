using MegaHairPro.Web.Data;
using MegaHairPro.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace MegaHairPro.Web.Controllers
{
    public class ComprasController : Controller
    {
        private readonly AppDbContext _context;

        public ComprasController(AppDbContext context)
        {
            _context = context;
        }

        // ✅ LISTAGEM
        [HttpGet]
        public IActionResult Index()
        {
            CarregarViewBags();
            return View(ObterComprasVM());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Index(Compra compra)
        {
            // Parse manual garante valor correto independente do model binding
            if (decimal.TryParse(Request.Form["QuantidadeGramas"],
                System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out var qtd))
                compra.QuantidadeGramas = qtd;

            if (decimal.TryParse(Request.Form["ValorTotal"],
                System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out var valor))
                compra.ValorTotal = valor;

            if (compra.ProdutoId == 0)
                ModelState.AddModelError("", "Selecione um Produto.");
            if (compra.FornecedorId == 0)
                ModelState.AddModelError("", "Selecione um Fornecedor.");
            if (compra.QuantidadeGramas == null || compra.QuantidadeGramas <= 0)
                ModelState.AddModelError("", "Informe a quantidade em gramas.");

            if (!ModelState.IsValid)
            {
                CarregarViewBags();
                return View("Index", ObterComprasVM());
            }

            compra.Ativo = true;
            _context.Compras.Add(compra);
            AtualizarEstoque(compra.ProdutoId, compra.QuantidadeGramas ?? 0, true);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }
        // ✅ EDITAR VIA AJAX
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditAjax(Compra compra)
        {
            var original = _context.Compras.FirstOrDefault(c => c.Id == compra.Id);

            if (original == null)
                return Json(new { success = false });

            // Reverte estoque antigo
            AtualizarEstoque(original.ProdutoId, original.QuantidadeGramas ?? 0, false);

            original.ProdutoId = compra.ProdutoId;
            original.FornecedorId = compra.FornecedorId;
            original.QuantidadeGramas = compra.QuantidadeGramas;
            original.ValorTotal = compra.ValorTotal;
            original.DataCompra = compra.DataCompra;

            // Aplica novo estoque
            AtualizarEstoque(original.ProdutoId, original.QuantidadeGramas ?? 0, true);
            _context.SaveChanges();

            // Retorna ViewModel com nomes incluídos
            var vm = ObterCompraVM(original.Id);
            return PartialView("_CompraRow", vm);
        }

        // ✅ TOGGLE STATUS
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ToggleStatusAjax(int id)
        {
            var compra = _context.Compras.FirstOrDefault(c => c.Id == id);

            if (compra == null)
                return Json(new { success = false });

            compra.Ativo = !compra.Ativo;

            if (compra.Ativo)
                AtualizarEstoque(compra.ProdutoId, compra.QuantidadeGramas ?? 0, true);
            else
                AtualizarEstoque(compra.ProdutoId, compra.QuantidadeGramas ?? 0, false);

            _context.SaveChanges();

            var vm = ObterCompraVM(compra.Id);
            return PartialView("_CompraRow", vm);
        }

        // ✅ EXCLUIR
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            var compra = _context.Compras.Find(id);

            if (compra == null)
                return Ok();

            AtualizarEstoque(compra.ProdutoId, compra.QuantidadeGramas ?? 0, false);
            _context.Compras.Remove(compra);
            _context.SaveChanges();

            return Ok();
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private void CarregarViewBags()
        {
            // Exibe código + nome no combo: "CAB-0001 - Cabelo Natural"
            ViewBag.Produtos = _context.Produtos
                .Where(p => p.Status == true)
                .OrderBy(p => p.Codigo)
                .Select(p => new { p.Id, Nome = p.Codigo + " - " + p.Nome })
                .ToList();

            ViewBag.Fornecedores = _context.Fornecedores
                .Where(f => f.Status == true)
                .OrderBy(f => f.Nome)
                .ToList();
        }

        private CompraViewModel ObterCompraVM(int id)
        {
            return _context.Compras
                .Include(c => c.Produto)
                .Include(c => c.Fornecedor)
                .Where(c => c.Id == id)
                .Select(c => new CompraViewModel
                {
                    Id = c.Id,
                    ProdutoId = c.ProdutoId,
                    FornecedorId = c.FornecedorId,
                    ProdutoNome = c.Produto.Codigo + " - " + c.Produto.Nome,
                    FornecedorNome = c.Fornecedor.Nome,
                    QuantidadeGramas = c.QuantidadeGramas ?? 0,
                    ValorTotal = c.ValorTotal,
                    DataCompra = c.DataCompra,
                    Ativo = c.Ativo
                })
                .First();
        }

        private List<CompraViewModel> ObterComprasVM()
        {
            return _context.Compras
                .Include(c => c.Produto)
                .Include(c => c.Fornecedor)
                .OrderByDescending(c => c.DataCompra)
                .Select(c => new CompraViewModel
                {
                    Id = c.Id,
                    ProdutoId = c.ProdutoId,
                    FornecedorId = c.FornecedorId,
                    ProdutoNome = c.Produto.Codigo + " - " + c.Produto.Nome,
                    FornecedorNome = c.Fornecedor.Nome,
                    QuantidadeGramas = c.QuantidadeGramas ?? 0,
                    ValorTotal = c.ValorTotal,
                    DataCompra = c.DataCompra,
                    Ativo = c.Ativo
                })
                .ToList();
        }

        private void AtualizarEstoque(int produtoId, decimal quantidade, bool adicionar)
        {
            var estoque = _context.Estoques.FirstOrDefault(e => e.ProdutoId == produtoId);
            var produto = _context.Produtos.FirstOrDefault(p => p.Id == produtoId);

            if (estoque == null)
            {
                estoque = new Estoque { ProdutoId = produtoId, QuantidadeGramas = 0 };
                _context.Estoques.Add(estoque);
            }

            estoque.QuantidadeGramas += adicionar ? quantidade : -quantidade;
            if (estoque.QuantidadeGramas < 0) estoque.QuantidadeGramas = 0;

            if (produto != null)
            {
                produto.EstoqueGramas += adicionar ? quantidade : -quantidade;
                if (produto.EstoqueGramas < 0) produto.EstoqueGramas = 0;
            }
        }
    }
}