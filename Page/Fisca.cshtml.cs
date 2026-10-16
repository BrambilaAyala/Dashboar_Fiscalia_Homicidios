using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProyectoFisca.Models;
using ProyectoFisca.Sources.Interface;

namespace ProyectoFisca.Pages
{
    public class FiscaModel : PageModel
    {
        private readonly IHomicidiosDataAccess _dataAccess;
        private readonly IEtlOrchestratorService _etlService;

        public FiscaModel(IHomicidiosDataAccess dataAccess, IEtlOrchestratorService etlService)
        {
            _dataAccess = dataAccess;
            _etlService = etlService;
        }

        [BindProperty(SupportsGet = true)]
        public string? Buscar { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? EntidadFiltro { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? AnioFiltro { get; set; } = "TODOS";

        [BindProperty(SupportsGet = true)]
        public int Pagina { get; set; } = 1;

        public int TamanoPagina { get; set; } = 30;
        public int TotalRegistros { get; set; }
        public int TotalPaginas { get; set; }

        public int TotalMuertos { get; set; }
        public int TotalHombres { get; set; }
        public int TotalMujeres { get; set; }
        public int TotalNoIdentificado { get; set; }
        public double PorcentajeHombres { get; set; }
        public double PorcentajeMujeres { get; set; }
        public double PorcentajeNoIdentificado { get; set; }
        public int TotalEntidadesRepresentadas { get; set; }

        public List<string> EntidadesDisponibles { get; set; } = new();
        public List<HomicidioDetalleDto> RegistrosPaginados { get; set; } = new();

        public string TopMunicipiosJson { get; set; } = "[]";

        [TempData]
        public string? MensajeEtl { get; set; }

        [TempData]
        public string? TipoMensajeEtl { get; set; }

        public async Task OnGetAsync()
        {
            var todos = await _dataAccess.ObtenerHomicidiosAsync();

            EntidadesDisponibles = todos.Select(h => h.Entidad)
                .Where(e => !string.IsNullOrEmpty(e)).Distinct().OrderBy(e => e).ToList();

            var filtrados = todos.AsQueryable();

            if (!string.IsNullOrWhiteSpace(Buscar))
            {
                string term = Buscar.Trim().ToUpperInvariant();
                filtrados = filtrados.Where(h =>
                    h.Municipio.ToUpperInvariant().Contains(term) ||
                    h.Entidad.ToUpperInvariant().Contains(term) ||
                    h.Fuente.ToUpperInvariant().Contains(term) ||
                    h.Fecha.Contains(term));
            }

            if (!string.IsNullOrWhiteSpace(EntidadFiltro) && EntidadFiltro != "TODAS") 
                filtrados = filtrados.Where(h => h.Entidad.Equals(EntidadFiltro, StringComparison.OrdinalIgnoreCase));
            if (!string.IsNullOrWhiteSpace(AnioFiltro) && AnioFiltro != "TODOS") 
                filtrados = filtrados.Where(h => h.Fecha.StartsWith(AnioFiltro));
            
            var listaFiltrada = filtrados.ToList();
            TotalRegistros = listaFiltrada.Count;

            TotalMuertos = listaFiltrada.Sum(h => h.Num_Muertos);
            TotalHombres = listaFiltrada.Sum(h => h.Hombres);
            TotalMujeres = listaFiltrada.Sum(h => h.Mujeres);
            TotalNoIdentificado = listaFiltrada.Sum(h => h.No_Identificado);

            int totalGenero = TotalHombres + TotalMujeres + TotalNoIdentificado;
            if (totalGenero > 0)
            {
                PorcentajeHombres = Math.Round((double)TotalHombres / totalGenero * 100, 1);
                PorcentajeMujeres = Math.Round((double)TotalMujeres / totalGenero * 100, 1);
                PorcentajeNoIdentificado = Math.Round((double)TotalNoIdentificado / totalGenero * 100, 1);
            }

            TotalEntidadesRepresentadas = listaFiltrada.Select(h => h.Entidad).Where(e => !string.IsNullOrEmpty(e)).Distinct().Count();

            // Preparar datos visuales para la gráfica de municipios críticos
            var topMun = listaFiltrada
                .GroupBy(h => new { h.Municipio, h.Entidad })
                .Select(g => new {
                    nombre = $"{g.Key.Municipio} ({g.Key.Entidad})",
                    muertos = g.Sum(x => x.Num_Muertos)
                })
                .OrderByDescending(x => x.muertos).Take(8).ToList();
            TopMunicipiosJson = JsonSerializer.Serialize(topMun);

            TotalPaginas = (int)Math.Ceiling((double)TotalRegistros / TamanoPagina);
            if (Pagina < 1) Pagina = 1;
            if (TotalPaginas > 0 && Pagina > TotalPaginas) Pagina = TotalPaginas;

            RegistrosPaginados = listaFiltrada.Skip((Pagina - 1) * TamanoPagina).Take(TamanoPagina).ToList();
        }

        public async Task<IActionResult> OnPostProcesarEtlAsync([FromForm] string? fechaInicio, [FromForm] string? fechaFin)
        {
            if (string.IsNullOrWhiteSpace(fechaInicio) || string.IsNullOrWhiteSpace(fechaFin))
            {
                MensajeEtl = "Debe especificar fechas de inicio y fin válidas.";
                TipoMensajeEtl = "danger";
                return RedirectToPage();
            }

            var formatos = new[] { "yyyy-MM-dd", "dd/MM/yyyy", "dd-MM-yyyy", "yyyy/MM/dd", "MM/dd/yyyy" };
            if (!DateTime.TryParseExact(fechaInicio.Trim(), formatos, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out var fIni) &&
                !DateTime.TryParse(fechaInicio.Trim(), out fIni))
            {
                MensajeEtl = $"Formato de fecha de inicio no reconocido: '{fechaInicio}'. Utilice AAAA-MM-DD.";
                TipoMensajeEtl = "danger";
                return RedirectToPage();
            }

            if (!DateTime.TryParseExact(fechaFin.Trim(), formatos, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out var fFin) &&
                !DateTime.TryParse(fechaFin.Trim(), out fFin))
            {
                MensajeEtl = $"Formato de fecha de fin no reconocido: '{fechaFin}'. Utilice AAAA-MM-DD.";
                TipoMensajeEtl = "danger";
                return RedirectToPage();
            }

            try
            {
                var resultado = await _etlService.ProcesarRangoFechasAsync(fIni, fFin);
                MensajeEtl = resultado.Mensaje;
                TipoMensajeEtl = resultado.Exitoso ? "success" : "warning";
            }
            catch (Exception ex)
            {
                MensajeEtl = $"Error en extracción ETL: {ex.Message}";
                TipoMensajeEtl = "danger";
            }
            return RedirectToPage();
        }
    }
}
