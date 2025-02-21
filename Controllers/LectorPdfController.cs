using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using Microsoft.AspNetCore.Authorization;
using ProyectoFisca.Models;

namespace ProyectoFisca.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [AllowAnonymous]
    public class LectorPdfController : ControllerBase
    {
        private readonly string connectionString = "server=localhost;user=root;password=HolaMundo22;database=fiscalia";

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var datosHomicidios = new List<object>();

            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    // Consulta para obtener los datos de homicidios, incluyendo columnas adicionales
                    var query = @"SELECT 
                        e.Nombre AS Entidad, 
                        (SELECT m.Nombre 
                        FROM Municipio m 
                        JOIN Homicidios h ON h.Municipio_Id = m.Id 
                        WHERE m.Entidad_Id = e.Id 
                        ORDER BY (h.Hombres + h.Mujeres + h.No_Identificado) DESC 
                        LIMIT 1) AS Municipio,
                        SUM(h.Hombres + h.Mujeres + h.No_Identificado) AS Num_Muertos,
                        SUM(h.Hombres) AS Hombres,
                        SUM(h.Mujeres) AS Mujeres,
                        SUM(h.No_Identificado) AS No_Identificado,
                        (SELECT h.Fuente 
                        FROM Homicidios h 
                        JOIN Municipio m ON h.Municipio_Id = m.Id 
                        WHERE m.Entidad_Id = e.Id 
                        ORDER BY (h.Hombres + h.Mujeres + h.No_Identificado) DESC 
                        LIMIT 1) AS Fuente
                    FROM Entidad e
                    JOIN Municipio m ON e.Id = m.Entidad_Id
                    JOIN Homicidios h ON m.Id = h.Municipio_Id
                    GROUP BY e.Id;";

                    using (var command = new MySqlCommand(query, connection))
                    {
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                string? municipio = reader["Municipio"].ToString();
                                string? entidad = reader["Entidad"].ToString();
                                int numMuertos = reader["Num_Muertos"] != DBNull.Value ? Convert.ToInt32(reader["Num_Muertos"]) : 0;
                                int hombres = reader["Hombres"] != DBNull.Value ? Convert.ToInt32(reader["Hombres"]) : 0;
                                int mujeres = reader["Mujeres"] != DBNull.Value ? Convert.ToInt32(reader["Mujeres"]) : 0;
                                int noIdentificado = reader["No_Identificado"] != DBNull.Value ? Convert.ToInt32(reader["No_Identificado"]) : 0;
                                //string? fuente = reader["Fuente"].ToString();

                                datosHomicidios.Add(new
                                {
                                    Municipio = municipio,
                                    Entidad = entidad,
                                    Num_Muertos = numMuertos,
                                    Hombres = hombres,
                                    Mujeres = mujeres,
                                    No_Identificado = noIdentificado,
                                    //Fuente = fuente
                                });
                            }
                        }
                    }
                }

                return Ok(datosHomicidios);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno: {ex.Message}");
            }
        }
    }
}
