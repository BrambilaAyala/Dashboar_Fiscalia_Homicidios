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
                        LIMIT 1) AS Fuente,
                        (SELECT h.Fecha 
                        FROM Homicidios h 
                        JOIN Municipio m ON h.Municipio_Id = m.Id 
                        WHERE m.Entidad_Id = e.Id 
                        ORDER BY (h.Hombres + h.Mujeres + h.No_Identificado) DESC 
                        LIMIT 1) AS Fecha
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
                                string? fuente = reader["Fuente"].ToString();
                                DateTime fecha = reader["Fecha"] != DBNull.Value ? Convert.ToDateTime(reader["Fecha"]) : DateTime.MinValue;

                                datosHomicidios.Add(new
                                {
                                    Municipio = municipio,
                                    Entidad = entidad,
                                    Num_Muertos = numMuertos,
                                    Hombres = hombres,
                                    Mujeres = mujeres,
                                    No_Identificado = noIdentificado,
                                    Fuente = fuente,
                                    Fecha = fecha.ToString("yyyy-MM-dd") // Formatear la fecha
                                });
                            }
                        }
                    }
                }

                // Ordenar los datos por fecha
                var datosOrdenados = datosHomicidios.Cast<dynamic>().OrderBy(d => DateTime.Parse(d.Fecha)).ToList();

                return Ok(datosOrdenados);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno: {ex.Message}");
            }
        }

        [HttpGet("homicidios-por-trimestre")]
        public async Task<IActionResult> GetHomicidiosPorTrimestre()
        {
            var datosHomicidiosPorTrimestre = new List<object>();

            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    // Nueva consulta SQL para obtener los datos de homicidios por mes
                    var query = @"SELECT 
    CONCAT(
        YEAR(h.Fecha), ' - ', 
        CASE 
            WHEN QUARTER(h.Fecha) = 1 THEN 'Enero-Marzo'
            WHEN QUARTER(h.Fecha) = 2 THEN 'Abril-Junio'
            WHEN QUARTER(h.Fecha) = 3 THEN 'Julio-Septiembre'
            WHEN QUARTER(h.Fecha) = 4 THEN 'Octubre-Diciembre'
        END
    ) AS Trimestre,
    SUM(h.Hombres + h.Mujeres + h.No_Identificado) AS Num_Muertos,
    SUM(h.Hombres) AS Hombres,
    SUM(h.Mujeres) AS Mujeres,
    SUM(h.No_Identificado) AS No_Identificado,
    MIN(h.Fecha) AS Fecha -- puedes usar MIN o MAX según prefieras una fecha representativa
FROM Homicidios h
GROUP BY Trimestre
ORDER BY Fecha;
";

                    using (var command = new MySqlCommand(query, connection))
                    {
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                //string? mes = reader["Mes"].ToString();
                                string? trimestre = reader["Trimestre"].ToString();
                                int numMuertos = reader["Num_Muertos"] != DBNull.Value ? Convert.ToInt32(reader["Num_Muertos"]) : 0;
                                int hombres = reader["Hombres"] != DBNull.Value ? Convert.ToInt32(reader["Hombres"]) : 0;
                                int mujeres = reader["Mujeres"] != DBNull.Value ? Convert.ToInt32(reader["Mujeres"]) : 0;
                                int noIdentificado = reader["No_Identificado"] != DBNull.Value ? Convert.ToInt32(reader["No_Identificado"]) : 0;
                                DateTime fecha = reader["Fecha"] != DBNull.Value ? Convert.ToDateTime(reader["Fecha"]) : DateTime.MinValue;

                                datosHomicidiosPorTrimestre.Add(new
                                {
                                    Trimestre = trimestre,
                                    Num_Muertos = numMuertos,
                                    Hombres = hombres,
                                    Mujeres = mujeres,
                                    No_Identificado = noIdentificado,
                                    Fecha = fecha.ToString("yyyy-MM-dd") // Formatear la fecha
                                });
                            }
                        }
                    }
                }
                var datosOrdenados = datosHomicidiosPorTrimestre.Cast<dynamic>().OrderBy(d => DateTime.Parse(d.Fecha)).ToList();
                return Ok(datosOrdenados);

                //return Ok(datosHomicidiosPorTrimestre);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno: {ex.Message}");
            }
        }
    }
}