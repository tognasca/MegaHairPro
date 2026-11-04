using MegaHairPro.Web.Data;
using MegaHairPro.Web.Models;
using MegaHairPro.Web.Models.Booking;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MegaHairPro.Web.Services;

public class BookingConfigService
{
    private readonly AppDbContext _context;
    private static readonly BookingConfigDto Defaults = new();
    private BookingConfigDto? _cache;
    private DateTime _cacheAt;

    public BookingConfigService(AppDbContext context) => _context = context;

    public Task EnsureSchemaAsync()
    {
        // ConfiguracoesBooking is part of the EF Core model and must be created by
        // the PostgreSQL migration. Runtime DDL is intentionally not executed here.
        return Task.CompletedTask;
    }

    public async Task<BookingConfigDto> GetAsync(bool force = false)
    {
        if (!force && _cache != null && (DateTime.UtcNow - _cacheAt).TotalMinutes < 1)
            return _cache;

        await EnsureSchemaAsync();

        var rows = await _context.ConfiguracoesBooking.AsNoTracking().ToListAsync();
        var cfg = new BookingConfigDto
        {
            PercentualSinal = Defaults.PercentualSinal,
            DiasLembreteAntes = Defaults.DiasLembreteAntes,
            PrazoAlteracaoHoras = Defaults.PrazoAlteracaoHoras,
        };

        foreach (var row in rows)
        {
            if (decimal.TryParse(row.Valor, out var d))
            {
                switch (row.Chave)
                {
                    case nameof(BookingConfigDto.PercentualSinal): cfg.PercentualSinal = d; break;
                    case nameof(BookingConfigDto.DiasLembreteAntes): cfg.DiasLembreteAntes = (int)d; break;
                    case nameof(BookingConfigDto.PrazoAlteracaoHoras): cfg.PrazoAlteracaoHoras = (int)d; break;
                }
            }
        }

        _cache = cfg;
        _cacheAt = DateTime.UtcNow;
        return cfg;
    }

    public async Task<BookingConfigDto> UpdateAsync(BookingConfigDto updates)
    {
        await EnsureSchemaAsync();

        async Task Set(string chave, string valor)
        {
            var row = await _context.ConfiguracoesBooking.FindAsync(chave);
            if (row == null)
                _context.ConfiguracoesBooking.Add(new ConfiguracaoBooking { Chave = chave, Valor = valor });
            else
            {
                row.Valor = valor;
                row.AtualizadoEm = DateTime.UtcNow;
            }
        }

        await Set(nameof(BookingConfigDto.PercentualSinal), updates.PercentualSinal.ToString());
        await Set(nameof(BookingConfigDto.DiasLembreteAntes), updates.DiasLembreteAntes.ToString());
        await Set(nameof(BookingConfigDto.PrazoAlteracaoHoras), updates.PrazoAlteracaoHoras.ToString());
        await _context.SaveChangesAsync();
        return await GetAsync(force: true);
    }

    public async Task<bool> PodeAlterarAsync(DateTime dataHora)
    {
        var cfg = await GetAsync();
        var horas = (dataHora - DateTime.Now).TotalHours;
        return horas >= cfg.PrazoAlteracaoHoras;
    }
}
