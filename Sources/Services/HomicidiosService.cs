using ProyectoFisca.Models;
using ProyectoFisca.Sources.Interface;

namespace ProyectoFisca.Sources.Services
{
    public class HomicidiosService : IHomicidiosService
    {
        private readonly IHomicidiosDataAccess _dataAccess;
        private readonly ILogger<HomicidiosService> _logger;

        public HomicidiosService(IHomicidiosDataAccess dataAccess, ILogger<HomicidiosService> logger)
        {
            _dataAccess = dataAccess;
            _logger = logger;
        }

        public async Task<List<HomicidiosViewModel>> ObtenerHomicidiosAsync(int limite = 10000)
        {
            try
            {
                return await _dataAccess.ObtenerHomicidiosViewModelAsync(limite);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en HomicidiosService al obtener lista de homicidios.");
                throw;
            }
        }

        public async Task<List<object>> ObtenerTotalesPorEntidadAsync()
        {
            try
            {
                var totales = await _dataAccess.ObtenerTotalesPorEntidadAsync();
                var result = new List<object>();
                foreach (var t in totales)
                {
                    result.Add(new
                    {
                        Entidad = t.Entidad,
                        Num_Muertos = t.Num_Muertos,
                        Hombres = t.Hombres,
                        Mujeres = t.Mujeres,
                        No_Identificado = t.No_Identificado
                    });
                }
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en HomicidiosService al obtener totales por entidad.");
                throw;
            }
        }
    }
}
