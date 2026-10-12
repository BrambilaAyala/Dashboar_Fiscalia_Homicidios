using ProyectoFisca.Models;

namespace ProyectoFisca.Sources.Interface
{
    public interface IEtlOrchestratorService
    {
        Task<EtlResultDto> ProcesarRangoFechasAsync(DateTime inicio, DateTime fin);
        Task<EtlResultDto> ProcesarArchivoLocalAsync(string rutaPdf, DateTime fecha);
        Task<List<ReporteInconsistencia>> ObtenerInconsistenciasAsync(int limite = 100);
    }
}
