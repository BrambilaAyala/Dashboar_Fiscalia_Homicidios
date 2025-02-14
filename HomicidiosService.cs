using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoFisca.Data;
using ProyectoFisca.Models;
using System.Threading.Tasks;
using System.Collections.Generic;

public class HomicidiosService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<HomicidiosService> _logger;

    public HomicidiosService(ApplicationDbContext context, ILogger<HomicidiosService> logger)
    {
        _context = context;
        _logger = logger;
    }
    [HttpGet]
    public async Task<List<HomicidiosViewModel>> ObtenerHomicidiosAsync()
    {
        try
        {
            var query = _context.Homicidios3
                ?.Include(h => h.Municipio)
                .ThenInclude(m => m.Entidad)
                .Take(20); // Limitar los resultados a 20 registros

            if (query == null)
            {
                throw new InvalidOperationException("Homicidios3 is null.");
            }

            return await query.Select(h => new HomicidiosViewModel
            {
                Entidad = h.Municipio.Entidad.Nombre,
                Municipio = h.Municipio.Nombre,
                Num_Muertos = h.Hombres + h.Mujeres + h.No_Identificado
            }).ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al consultar datos");
            throw;
        }
    }

}