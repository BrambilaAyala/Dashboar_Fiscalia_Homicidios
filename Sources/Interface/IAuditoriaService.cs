using ProyectoFisca.Models;

namespace ProyectoFisca.Sources.Interface
{
    public interface IAuditoriaService
    {
        Task<AuditoriaResumenDto> ObtenerResumenAuditoriaAsync();
    }
}
