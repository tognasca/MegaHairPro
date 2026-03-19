using MegaHairPro.Web.Data;
using MegaHairPro.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MegaHairPro.Web.Controllers;

public class AnamneseController : Controller
{
    private readonly AppDbContext _context;

    public AnamneseController(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(int clienteId)
    {
        var cliente = await _context.Clientes.FindAsync(clienteId);
        if (cliente == null) return NotFound();

        ViewBag.Cliente = cliente;

        var lista = await _context.Anamneses
            .Where(a => a.ClienteId == clienteId && a.Ativa)
            .OrderByDescending(a => a.DataCadastro)
            .ToListAsync();

        return View(lista);
    }

    [HttpGet]
    public async Task<IActionResult> Create(int clienteId)
    {
        var cliente = await _context.Clientes.FindAsync(clienteId);
        if (cliente == null) return NotFound();

        ViewBag.Cliente = cliente;
        var model = new Anamnese { ClienteId = clienteId };
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Anamnese model)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Cliente = await _context.Clientes.FindAsync(model.ClienteId);
            return View(model);
        }

        model.DataCadastro = DateTime.Now;
        model.Ativa = true;

        _context.Anamneses.Add(model);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index), new { clienteId = model.ClienteId });
    }

    public async Task<IActionResult> Details(int id)
    {
        var anamnese = await _context.Anamneses
            .Include(a => a.Cliente)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (anamnese == null) return NotFound();

        return View(anamnese);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var anamnese = await _context.Anamneses.FindAsync(id);
        if (anamnese == null) return NotFound();

        anamnese.Ativa = false;
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index), new { clienteId = anamnese.ClienteId });
    }
}

