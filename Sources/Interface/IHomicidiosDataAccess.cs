using ProyectoFisca.Models;

namespace ProyectoFisca.Sources.Interface
{
    public interface IHomicidiosDataAccess
    {
        Task<List<HomicidioDetalleDto>> ObtenerHomicidiosAsync();
        Task<List<HomicidioTrimestreDto>> ObtenerHomicidiosPorTrimestreAsync();
        Task<List<TotalesEntidadDto>> ObtenerTotalesPorEntidadAsync();
        Task<List<HomicidiosViewModel>> ObtenerHomicidiosViewModelAsync(int limite = 10000);
    }
}
