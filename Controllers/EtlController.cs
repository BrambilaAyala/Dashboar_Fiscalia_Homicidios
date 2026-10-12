using Microsoft.AspNetCore.Mvc;
using ProyectoFisca.Models;
using ProyectoFisca.Sources.Interface;

namespace ProyectoFisca.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EtlController : ControllerBase
    {
        private readonly IEtlOrchestratorService _etlService;
        private readonly INormalizadorGeoService _normalizador;
        private readonly IPdfPipelineService _pdfService;
        private readonly IWebHostEnvironment _env;

        public EtlController(IEtlOrchestratorService etlService, INormalizadorGeoService normalizador, IPdfPipelineService pdfService,
            IWebHostEnvironment env)
        {
            _etlService = etlService;
            _normalizador = normalizador;
            _pdfService = pdfService;
            _env = env;
        }

        [HttpPost("procesar-rango")]
        public async Task<IActionResult> ProcesarRango([FromBody] RangoFechasRequest request)
        {
            if (request.FechaInicio == default || request.FechaFin == default)
            {
                return BadRequest("Las fechas de inicio y fin son obligatorias.");
            }

            var resultado = await _etlService.ProcesarRangoFechasAsync(request.FechaInicio, request.FechaFin);
            return Ok(resultado);
        }

        [HttpPost("procesar-dia")]
        public async Task<IActionResult> ProcesarDia([FromQuery] string fecha)
        {
            if (!DateTime.TryParse(fecha, out var fechaParsed))
            {
                return BadRequest("Formato de fecha inválido. Utilice yyyy-MM-dd.");
            }

            var resultado = await _etlService.ProcesarRangoFechasAsync(fechaParsed, fechaParsed);
            return Ok(resultado);
        }

        [HttpGet("inconsistencias")]
        public async Task<IActionResult> ObtenerInconsistencias([FromQuery] int limite = 50)
        {
            var lista = await _etlService.ObtenerInconsistenciasAsync(limite);
            return Ok(lista);
        }

        [HttpGet("probar-normalizacion")]
        public IActionResult ProbarNormalizacion([FromQuery] string entidad, [FromQuery] string municipio, [FromQuery] string? fuente = null)
        {
            var (entidadRes, munRes, revision, detalle) = _normalizador.Resolver(entidad, municipio, fuente ?? string.Empty);
            return Ok(new
            {
                Entrada = new { Entidad = entidad, Municipio = municipio, Fuente = fuente },
                Resultado = new
                {
                    EntidadNormalizada = entidadRes,
                    MunicipioNormalizado = munRes,
                    RequiereRevision = revision,
                    DetalleInferencia = detalle
                }
            });
        }

        [HttpPost("procesar-archivo-muestra")]
        public async Task<IActionResult> ProcesarMuestra([FromQuery] string? nombreArchivo = null)
        {
            string archivo = nombreArchivo ?? "test_homicidios.pdf";
            string rutaCompleta = Path.Combine(_env.ContentRootPath, archivo);

            if (!System.IO.File.Exists(rutaCompleta))
            {
                return NotFound($"El archivo de prueba {archivo} no existe en la raíz del proyecto.");
            }

            var fecha = DateTime.Now.Date;
            var resultado = await _etlService.ProcesarArchivoLocalAsync(rutaCompleta, fecha);
            return Ok(resultado);
        }

        [HttpGet("entidades-oficiales")]
        public IActionResult ObtenerEntidades()
        {
            var entidades = _normalizador.ObtenerEntidadesOficiales();
            return Ok(entidades);
        }
    }
}
