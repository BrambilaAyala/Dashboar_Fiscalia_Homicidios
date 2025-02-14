using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoFisca.Data;
using ProyectoFisca;
using ProyectoFisca.Models;

[ApiController]
[Route("[controller]")]
public class HomicidiosController : ControllerBase
{
    private readonly HomicidiosService _service;

    public HomicidiosController(HomicidiosService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetHomicidios()
    {
        try
        {
            var datos = await _service.ObtenerHomicidiosAsync();
            Console.WriteLine("El endpoint /homicidios fue llamado correctamente");

            return Ok(datos); // Asegúrate de que este 'datos' sea un objeto JSON
            //Console.WriteLine("El endpoint /homicidios fue llamado correctamente");
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error en el controlador: {ex.Message}");
            return StatusCode(500, "Error interno del servidor");
        }
        
    }
}



