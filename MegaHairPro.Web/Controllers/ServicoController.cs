using MegaHairPro.Web.Data;
using MegaHairPro.Web.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Linq;

namespace MegaHairPro.Web.Controllers
{
    public class ServicoController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public ServicoController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        public IActionResult Index()
        {
            return View(_context.Servicos.ToList());
        }

        public IActionResult Create() => View();

        [HttpPost]
        public IActionResult Create(Servico model, IFormFile imagem)
        {
            if (!ModelState.IsValid)
                return View(model);

            if (imagem != null && imagem.Length > 0)
            {
                var pasta = Path.Combine(_env.WebRootPath, "uploads");
                Directory.CreateDirectory(pasta);

                var nome = Guid.NewGuid() + Path.GetExtension(imagem.FileName);

                var caminho = Path.Combine(pasta, nome);

                using (var stream = new FileStream(caminho, FileMode.Create))
                    imagem.CopyTo(stream);

                model.ImagemUrl = "/uploads/" + nome;
            }

            _context.Add(model);
            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Edit(int id)
        {
            return View(_context.Servicos.Find(id));
        }

        [HttpPost]
        public IActionResult Edit(Servico model, IFormFile imagem)
        {
            if (imagem != null)
            {
                var pasta = Path.Combine(_env.WebRootPath, "uploads");
                var caminho = Path.Combine(pasta, imagem.FileName);

                using (var stream = new FileStream(caminho, FileMode.Create))
                    imagem.CopyTo(stream);

                model.ImagemUrl = "/uploads/" + imagem.FileName;
            }

            _context.Update(model);
            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Delete(int id)
        {
            var s = _context.Servicos.Find(id);
            _context.Remove(s);
            _context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }
    }
}
