using ProyectoFisca.Models;

namespace ProyectoFisca.Sources.Interface
{
    public interface IHomicidiosService
    {
        Task<List<HomicidiosViewModel>> ObtenerHomicidiosAsync(int limite = 10000);
        Task<List<object>> ObtenerTotalesPorEntidadAsync();
    }
}
