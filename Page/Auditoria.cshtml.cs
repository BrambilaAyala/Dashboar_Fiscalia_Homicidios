using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProyectoFisca.Models;
using ProyectoFisca.Sources.Interface;

namespace ProyectoFisca.Pages
{
    public class AuditoriaModel : PageModel
    {
        private readonly IAuditoriaService _auditoriaService;

        public AuditoriaModel(IAuditoriaService auditoriaService)
        {
            _auditoriaService = auditoriaService;
        }

        public AuditoriaResumenDto Resumen { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string? Buscar { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? FiltroMotivo { get; set; } = "TODOS";

        [BindProperty(SupportsGet = true)]
        public int Pagina { get; set; } = 1;

        public int TamanoPagina { get; set; } = 15;
        public int TotalRegistrosFiltrados { get; set; }
        public int TotalPaginas { get; set; }
        public List<ReporteInconsistencia> RegistrosPaginados { get; set; } = new();

        public string JsonEstadoArchivos { get; set; } = "[]";
        public string JsonMotivosInconsistencias { get; set; } = "[]";
        public string JsonInconsistenciasPorEntidad { get; set; } = "[]";
        public string JsonInconsistenciasPorAnio { get; set; } = "[]";

        public async Task OnGetAsync()
        {
            Resumen = await _auditoriaService.ObtenerResumenAuditoriaAsync();

            // Preparar JSON para las gráficas interactivas
            JsonEstadoArchivos = JsonSerializer.Serialize(Resumen.EstadoArchivos);
            JsonMotivosInconsistencias = JsonSerializer.Serialize(Resumen.MotivosInconsistencias);
            JsonInconsistenciasPorEntidad = JsonSerializer.Serialize(Resumen.InconsistenciasPorEntidad);
            JsonInconsistenciasPorAnio = JsonSerializer.Serialize(Resumen.InconsistenciasPorAnio);

            // Filtrado de la tabla de auditoría
            var query = Resumen.InconsistenciasDetalle.AsQueryable();

            if (!string.IsNullOrWhiteSpace(Buscar))
            {
                string b = Buscar.Trim().ToUpperInvariant();
                query = query.Where(i =>
                    (i.EntidadDetectada ?? "").ToUpperInvariant().Contains(b) ||
                    (i.MunicipioDetectado ?? "").ToUpperInvariant().Contains(b) ||
                    (i.MotivoInconsistencia ?? "").ToUpperInvariant().Contains(b) ||
                    (i.Fuente ?? "").ToUpperInvariant().Contains(b) ||
                    i.Fecha.ToString("yyyy-MM-dd").Contains(b));
            }

            if (!string.IsNullOrWhiteSpace(FiltroMotivo) && FiltroMotivo != "TODOS") 
                query = query.Where(i => (i.MotivoInconsistencia ?? "").Contains(FiltroMotivo, StringComparison.OrdinalIgnoreCase));
            
            var filtrada = query.ToList();
            TotalRegistrosFiltrados = filtrada.Count;
            TotalPaginas = (int)Math.Ceiling((double)TotalRegistrosFiltrados / TamanoPagina);
            if (Pagina < 1) Pagina = 1;
            if (TotalPaginas > 0 && Pagina > TotalPaginas) Pagina = TotalPaginas;

            RegistrosPaginados = filtrada.Skip((Pagina - 1) * TamanoPagina).Take(TamanoPagina).ToList();
        }
    }
}
