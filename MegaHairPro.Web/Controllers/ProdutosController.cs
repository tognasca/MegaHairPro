using MegaHairPro.Web.Data;
using MegaHairPro.Web.Models;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace MegaHairPro.Web.Controllers
{
    public class ProdutosController : Controller
    {
        private readonly AppDbContext _context;

        public ProdutosController(AppDbContext context)
        {
            _context = context;
        }

        // ✅ INDEX GET
        [HttpGet]
        public IActionResult Index()
        {
            var produtos = _context.Produtos
                .OrderByDescending(p => p.Id)
                .ToList();

            return View(produtos);
        }

        // ✅ CREATE CORRETO
        [HttpPost]
        ////[ValidateAntiForgeryToken]
        public IActionResult Index(Produto produto)
        {
            produto.Status = true;
            produto.Codigo = GerarCodigoProduto(produto.TipoProduto);

            _context.Produtos.Add(produto);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        // ✅ EDIT AJAX
        [HttpPost]
        public IActionResult EditAjax(Produto produto)
        {
            var original = _context.Produtos.Find(produto.Id);

            if (original == null)
                return Json(new { success = false });

            original.Nome = produto.Nome;
            original.TipoProduto = produto.TipoProduto;
            original.Origem = produto.Origem;
            original.Tamanho = produto.Tamanho;
            original.Cor = produto.Cor;

            _context.SaveChanges();

            return PartialView("_ProdutoRow", original);
        }

        // ✅ ATIVAR / INATIVAR
        [HttpPost]
        public IActionResult ToggleStatusAjax(int id)
        {
            var produto = _context.Produtos.Find(id);

            if (produto == null)
                return Json(new { success = false });

            produto.Status = !produto.Status;
            _context.SaveChanges();

            return Json(new { success = true });
        }

        // ✅ DELETE
        [HttpPost]
        public IActionResult Delete(int id)
        {
            var produto = _context.Produtos.Find(id);

            if (produto == null)
                return RedirectToAction("Index");

            _context.Produtos.Remove(produto);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        // ✅ GERAR CÓDIGO
        private string GerarCodigoProduto(TipoProduto tipo)
        {
            string prefixo = tipo switch
            {
                TipoProduto.Cabelo => "CAB",
                TipoProduto.Acessorio => "ACE",
                _ => "GEN"
            };

            var ultimoCodigo = _context.Produtos
                .Where(p => p.TipoProduto == tipo && p.Codigo.StartsWith(prefixo + "-"))
                .OrderByDescending(p => p.Codigo)
                .Select(p => p.Codigo)
                .FirstOrDefault();

            int numero = 1;

            if (!string.IsNullOrEmpty(ultimoCodigo))
            {
                var partes = ultimoCodigo.Split('-');

                if (partes.Length == 2 && int.TryParse(partes[1], out int n))   
                    numero = n + 1;
            }

            return $"{prefixo}-{numero:D4}";
        }
    }
}
