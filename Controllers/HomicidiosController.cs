using Microsoft.AspNetCore.Mvc;
using ProyectoFisca.Sources.Interface;

namespace ProyectoFisca.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HomicidiosController : ControllerBase
    {
        private readonly IHomicidiosService _service;

        public HomicidiosController(IHomicidiosService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetHomicidios()
        {
            try
            {
                var datos = await _service.ObtenerHomicidiosAsync();
                return Ok(datos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }

        [HttpGet("totales-por-entidad")]
        public async Task<IActionResult> GetTotalesPorEntidad()
        {
            try
            {
                var datos = await _service.ObtenerTotalesPorEntidadAsync();
                return Ok(datos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }
    }
}
