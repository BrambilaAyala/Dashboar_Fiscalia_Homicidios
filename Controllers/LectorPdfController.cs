using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProyectoFisca.Sources.Interface;

namespace ProyectoFisca.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [AllowAnonymous]
    public class LectorPdfController : ControllerBase
    {
        private readonly IHomicidiosDataAccess _dataAccess;

        public LectorPdfController(IHomicidiosDataAccess dataAccess)
        {
            _dataAccess = dataAccess;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            try
            {
                var datos = await _dataAccess.ObtenerHomicidiosAsync();
                return Ok(datos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno: {ex.Message}");
            }
        }

        [HttpGet("homicidios-por-trimestre")]
        public async Task<IActionResult> GetHomicidiosPorTrimestre()
        {
            try
            {
                var datos = await _dataAccess.ObtenerHomicidiosPorTrimestreAsync();
                return Ok(datos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno: {ex.Message}");
            }
        }
    }
}