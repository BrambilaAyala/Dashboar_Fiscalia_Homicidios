using MySql.Data.MySqlClient;
using ProyectoFisca.Models;
using ProyectoFisca.Sources.Interface;

namespace ProyectoFisca.DataAccess
{
    public class HomicidiosDataAccess : IHomicidiosDataAccess
    {
        private readonly IDbConnectionFactory _connectionFactory;
        private readonly ILogger<HomicidiosDataAccess> _logger;

        public HomicidiosDataAccess(IDbConnectionFactory connectionFactory, ILogger<HomicidiosDataAccess> logger)
        {
            _connectionFactory = connectionFactory;
            _logger = logger;
        }

        public async Task<List<HomicidioDetalleDto>> ObtenerHomicidiosAsync()
        {
            var lista = new List<HomicidioDetalleDto>();

            try
            {
                using (var connection = (MySqlConnection)_connectionFactory.CreateConnection())
                {
                    await connection.OpenAsync();

                    var query = @"SELECT 
                        h.id AS Id,
                        COALESCE(m.nombre, 'NO ESPECIFICADO') AS Municipio,
                        COALESCE(e.nombre, 'NO ESPECIFICADO') AS Entidad, 
                        COALESCE(h.num_muertos, (h.hombres + h.mujeres + h.no_identificado)) AS Num_Muertos,
                        COALESCE(h.hombres, 0) AS Hombres,
                        COALESCE(h.mujeres, 0) AS Mujeres,
                        COALESCE(h.no_identificado, 0) AS No_Identificado,
                        COALESCE(h.fuente, '') AS Fuente,
                        h.fecha AS Fecha
                    FROM Homicidios h
                    LEFT JOIN Municipio m ON h.municipio_id = m.id
                    LEFT JOIN Entidad e ON m.entidad_id = e.id
                    ORDER BY h.fecha DESC, h.id DESC;";

                    using (var command = new MySqlCommand(query, connection))
                    {
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                int id = reader["Id"] != DBNull.Value ? Convert.ToInt32(reader["Id"]) : 0;
                                string municipio = reader["Municipio"]?.ToString() ?? "NO ESPECIFICADO";
                                string entidad = reader["Entidad"]?.ToString() ?? "NO ESPECIFICADO";
                                int numMuertos = reader["Num_Muertos"] != DBNull.Value ? Convert.ToInt32(reader["Num_Muertos"]) : 0;
                                int hombres = reader["Hombres"] != DBNull.Value ? Convert.ToInt32(reader["Hombres"]) : 0;
                                int mujeres = reader["Mujeres"] != DBNull.Value ? Convert.ToInt32(reader["Mujeres"]) : 0;
                                int noIdentificado = reader["No_Identificado"] != DBNull.Value ? Convert.ToInt32(reader["No_Identificado"]) : 0;
                                string fuente = reader["Fuente"]?.ToString() ?? string.Empty;
                                DateTime fecha = reader["Fecha"] != DBNull.Value ? Convert.ToDateTime(reader["Fecha"]) : DateTime.MinValue;

                                lista.Add(new HomicidioDetalleDto
                                {
                                    Id = id,
                                    Municipio = municipio,
                                    Entidad = entidad,
                                    Num_Muertos = numMuertos,
                                    Hombres = hombres,
                                    Mujeres = mujeres,
                                    No_Identificado = noIdentificado,
                                    Fuente = fuente,
                                    Fecha = fecha.ToString("yyyy-MM-dd")
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en DataAccess al obtener lista completa de homicidios.");
                throw;
            }
            return lista;
        }

        public async Task<List<HomicidioTrimestreDto>> ObtenerHomicidiosPorTrimestreAsync()
        {
            var lista = new List<HomicidioTrimestreDto>();

            try
            {
                using (var connection = (MySqlConnection)_connectionFactory.CreateConnection())
                {
                    await connection.OpenAsync();

                    var query = @"SELECT 
                        CONCAT(
                            YEAR(h.fecha), ' - ', 
                            CASE 
                                WHEN QUARTER(h.fecha) = 1 THEN 'Enero-Marzo'
                                WHEN QUARTER(h.fecha) = 2 THEN 'Abril-Junio'
                                WHEN QUARTER(h.fecha) = 3 THEN 'Julio-Septiembre'
                                WHEN QUARTER(h.fecha) = 4 THEN 'Octubre-Diciembre'
                            END
                        ) AS Trimestre,
                        SUM(COALESCE(NULLIF(h.num_muertos, 0), (h.hombres + h.mujeres + h.no_identificado))) AS Num_Muertos,
                        SUM(COALESCE(h.hombres, 0)) AS Hombres,
                        SUM(COALESCE(h.mujeres, 0)) AS Mujeres,
                        SUM(CASE WHEN (COALESCE(h.hombres,0) + COALESCE(h.mujeres,0) + COALESCE(h.no_identificado,0)) = 0 THEN COALESCE(h.num_muertos, 0) ELSE COALESCE(h.no_identificado, 0) END) AS No_Identificado,
                        MIN(h.fecha) AS Fecha
                    FROM Homicidios h
                    GROUP BY YEAR(h.fecha), QUARTER(h.fecha), Trimestre
                    ORDER BY YEAR(h.fecha), QUARTER(h.fecha);";

                    using (var command = new MySqlCommand(query, connection))
                    {
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                string trimestre = reader["Trimestre"]?.ToString() ?? string.Empty;
                                int numMuertos = reader["Num_Muertos"] != DBNull.Value ? Convert.ToInt32(reader["Num_Muertos"]) : 0;
                                int hombres = reader["Hombres"] != DBNull.Value ? Convert.ToInt32(reader["Hombres"]) : 0;
                                int mujeres = reader["Mujeres"] != DBNull.Value ? Convert.ToInt32(reader["Mujeres"]) : 0;
                                int noIdentificado = reader["No_Identificado"] != DBNull.Value ? Convert.ToInt32(reader["No_Identificado"]) : 0;
                                DateTime fecha = reader["Fecha"] != DBNull.Value ? Convert.ToDateTime(reader["Fecha"]) : DateTime.MinValue;

                                lista.Add(new HomicidioTrimestreDto
                                {
                                    Trimestre = trimestre,
                                    Num_Muertos = numMuertos,
                                    Hombres = hombres,
                                    Mujeres = mujeres,
                                    No_Identificado = noIdentificado,
                                    Fecha = fecha.ToString("yyyy-MM-dd")
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en DataAccess al obtener homicidios por trimestre.");
                throw;
            }
            return lista;
        }

        public async Task<List<TotalesEntidadDto>> ObtenerTotalesPorEntidadAsync()
        {
            var lista = new List<TotalesEntidadDto>();

            try
            {
                using (var connection = (MySqlConnection)_connectionFactory.CreateConnection())
                {
                    await connection.OpenAsync();

                    var query = @"SELECT 
                        COALESCE(e.nombre, 'NO ESPECIFICADO') AS Entidad,
                        SUM(COALESCE(NULLIF(h.num_muertos, 0), (h.hombres + h.mujeres + h.no_identificado))) AS Num_Muertos,
                        SUM(COALESCE(h.hombres, 0)) AS Hombres,
                        SUM(COALESCE(h.mujeres, 0)) AS Mujeres,
                        SUM(COALESCE(h.no_identificado, 0)) AS No_Identificado
                    FROM Homicidios h
                    LEFT JOIN Municipio m ON h.municipio_id = m.id
                    LEFT JOIN Entidad e ON m.entidad_id = e.id
                    GROUP BY COALESCE(e.nombre, 'NO ESPECIFICADO')
                    ORDER BY Num_Muertos DESC;";

                    using (var command = new MySqlCommand(query, connection))
                    {
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                string entidad = reader["Entidad"]?.ToString() ?? "NO ESPECIFICADO";
                                int numMuertos = reader["Num_Muertos"] != DBNull.Value ? Convert.ToInt32(reader["Num_Muertos"]) : 0;
                                int hombres = reader["Hombres"] != DBNull.Value ? Convert.ToInt32(reader["Hombres"]) : 0;
                                int mujeres = reader["Mujeres"] != DBNull.Value ? Convert.ToInt32(reader["Mujeres"]) : 0;
                                int noIdentificado = reader["No_Identificado"] != DBNull.Value ? Convert.ToInt32(reader["No_Identificado"]) : 0;

                                lista.Add(new TotalesEntidadDto
                                {
                                    Entidad = entidad,
                                    Num_Muertos = numMuertos,
                                    Hombres = hombres,
                                    Mujeres = mujeres,
                                    No_Identificado = noIdentificado
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en DataAccess al obtener totales por entidad.");
                throw;
            }
            return lista;
        }

        public async Task<List<HomicidiosViewModel>> ObtenerHomicidiosViewModelAsync(int limite = 10000)
        {
            var lista = new List<HomicidiosViewModel>();

            try
            {
                using (var connection = (MySqlConnection)_connectionFactory.CreateConnection())
                {
                    await connection.OpenAsync();

                    var query = $@"SELECT 
                        COALESCE(e.nombre, 'NO ESPECIFICADO') AS Entidad,
                        COALESCE(m.nombre, 'NO ESPECIFICADO') AS Municipio,
                        COALESCE(h.num_muertos, (h.hombres + h.mujeres + h.no_identificado)) AS Num_Muertos
                    FROM Homicidios h
                    LEFT JOIN Municipio m ON h.municipio_id = m.id
                    LEFT JOIN Entidad e ON m.entidad_id = e.id
                    ORDER BY h.fecha DESC, h.id DESC
                    {(limite > 0 ? $"LIMIT {limite}" : "")};";

                    using (var command = new MySqlCommand(query, connection))
                    {
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                lista.Add(new HomicidiosViewModel
                                {
                                    Entidad = reader["Entidad"]?.ToString() ?? "NO ESPECIFICADO",
                                    Municipio = reader["Municipio"]?.ToString() ?? "NO ESPECIFICADO",
                                    Num_Muertos = reader["Num_Muertos"] != DBNull.Value ? Convert.ToInt32(reader["Num_Muertos"]) : 0
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en DataAccess al obtener HomicidiosViewModel.");
                throw;
            }
            return lista;
        }
    }
}
