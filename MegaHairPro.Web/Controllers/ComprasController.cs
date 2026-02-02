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

            var comprasVM = _context.Compras
                .Include(c => c.Produto)
                .Include(c => c.Fornecedor)
                .OrderByDescending(c => c.DataCompra)
                .Select(c => new CompraViewModel
                {
                    Id = c.Id,
                    ProdutoId = c.ProdutoId,
                    FornecedorId = c.FornecedorId,
                    ProdutoNome = c.Produto.Nome,
                    FornecedorNome = c.Fornecedor.Nome,
                    QuantidadeGramas = c.QuantidadeGramas ?? 0,
                    ValorTotal = c.ValorTotal,
                    DataCompra = c.DataCompra,
                    Ativo = c.Ativo
                })
                .ToList();

            return View(comprasVM);
        }

        // ✅ CREATE CORRETO (não é Index)
        [HttpPost]
        //[ValidateAntiForgeryToken]
        public IActionResult Index(Compra compra)
        {
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
        public IActionResult EditAjax(Compra compra)
        {
            var original = _context.Compras.FirstOrDefault(c => c.Id == compra.Id);

            if (original == null)
                return Json(new { success = false });

            // Remove estoque antigo
            AtualizarEstoque(original.ProdutoId, original.QuantidadeGramas ?? 0, false);

            // Atualiza dados
            original.ProdutoId = compra.ProdutoId;
            original.FornecedorId = compra.FornecedorId;
            original.QuantidadeGramas = compra.QuantidadeGramas;
            original.ValorTotal = compra.ValorTotal;
            original.DataCompra = compra.DataCompra;

            // Aplica novo estoque
            AtualizarEstoque(original.ProdutoId, original.QuantidadeGramas ?? 0, true);

            _context.SaveChanges();

            var vm = new CompraViewModel
            {
                Id = original.Id,

                ProdutoId = original.ProdutoId,
                FornecedorId = original.FornecedorId,

                ProdutoNome = _context.Produtos.Find(original.ProdutoId)?.Nome,
                FornecedorNome = _context.Fornecedores.Find(original.FornecedorId)?.Nome,

                QuantidadeGramas = original.QuantidadeGramas ?? 0,
                ValorTotal = original.ValorTotal,
                DataCompra = original.DataCompra,
                Ativo = original.Ativo
            };

            return PartialView("_CompraRow", vm);
        }

        // ✅ ATIVAR/DESATIVAR
        [HttpPost]
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

            return Json(new { success = true, ativo = compra.Ativo });
        }

        // ✅ EXCLUIR
        [HttpPost]
        public IActionResult Delete(int id)
        {
            var compra = _context.Compras.Find(id);

            if (compra == null)
                return RedirectToAction("Index");

            AtualizarEstoque(compra.ProdutoId, compra.QuantidadeGramas ?? 0, false);

            _context.Compras.Remove(compra);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        // ==========================
        // ✅ HELPERS
        // ==========================

        private void CarregarViewBags()
        {
            ViewBag.Produtos = _context.Produtos.Where(p => p.Status == true).ToList();
            ViewBag.Fornecedores = _context.Fornecedores.Where(f => f.Status == true).ToList();
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

                    ProdutoNome = c.Produto.Nome,
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

            if (estoque == null)
            {
                estoque = new Estoque
                {
                    ProdutoId = produtoId,
                    QuantidadeGramas = 0
                };
                _context.Estoques.Add(estoque);
            }

            estoque.QuantidadeGramas += adicionar ? quantidade : -quantidade;
            if (estoque.QuantidadeGramas < 0)
                estoque.QuantidadeGramas = 0;
        }
    }
}
