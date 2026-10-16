using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using MySql.Data.MySqlClient;
using ProyectoFisca.Models;
using ProyectoFisca.Sources.Interface;

namespace ProyectoFisca.Sources.Services
{
    public class EtlOrchestratorService : IEtlOrchestratorService
    {
        private readonly IDbConnectionFactory _connectionFactory;
        private readonly IPdfPipelineService _pdfService;
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<EtlOrchestratorService> _logger;

        public EtlOrchestratorService(IDbConnectionFactory connectionFactory, IPdfPipelineService pdfService, 
            IWebHostEnvironment env,  ILogger<EtlOrchestratorService> logger)
        {
            _connectionFactory = connectionFactory;
            _pdfService = pdfService;
            _env = env;
            _logger = logger;
        }

        public async Task<List<ReporteInconsistencia>> ObtenerInconsistenciasAsync(int limite = 100)
        {
            var lista = new List<ReporteInconsistencia>();
            try
            {
                using var conn = (MySqlConnection)_connectionFactory.CreateConnection();
                await conn.OpenAsync();

                // Asegurar que la tabla exista
                using (var cmdInit = new MySqlCommand(@"
                    CREATE TABLE IF NOT EXISTS reportesinconsistencias (
                        id INT AUTO_INCREMENT PRIMARY KEY,
                        fecha DATETIME,
                        entidad_detectada VARCHAR(255),
                        municipio_detectado VARCHAR(255),
                        num_muertos INT,
                        fuente VARCHAR(500),
                        motivo_inconsistencia VARCHAR(500),
                        fecha_registro DATETIME DEFAULT CURRENT_TIMESTAMP
                    );", conn))
                {
                    await cmdInit.ExecuteNonQueryAsync();
                }

                using var cmd = new MySqlCommand(
                    "SELECT id, fecha, entidad_detectada, municipio_detectado, num_muertos, fuente, motivo_inconsistencia, fecha_registro FROM reportesinconsistencias ORDER BY fecha_registro DESC LIMIT @limite;", 
                    conn);
                cmd.Parameters.AddWithValue("@limite", limite);

                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())//
                {
                    lista.Add(new ReporteInconsistencia
                    {
                        Id = reader.GetInt32(0),
                        Fecha = reader.IsDBNull(1) ? DateTime.MinValue : reader.GetDateTime(1),
                        EntidadDetectada = reader.IsDBNull(2) ? null : reader.GetString(2),
                        MunicipioDetectado = reader.IsDBNull(3) ? null : reader.GetString(3),
                        NumMuertos = reader.IsDBNull(4) ? 0 : reader.GetInt32(4),
                        Fuente = reader.IsDBNull(5) ? null : reader.GetString(5),
                        MotivoInconsistencia = reader.IsDBNull(6) ? null : reader.GetString(6),
                        FechaRegistro = reader.IsDBNull(7) ? DateTime.Now : reader.GetDateTime(7)
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener inconsistencias desde la base de datos.");
            }
            return lista;
        }

        public async Task<EtlResultDto> ProcesarRangoFechasAsync(DateTime inicio, DateTime fin)
        {
            var resultado = new EtlResultDto { Exitoso = true };
            string carpetaPdf = Path.Combine(_env.ContentRootPath, "DataPdf");

            if (inicio > fin) 
                (inicio, fin) = (fin, inicio);
            if ((fin - inicio).TotalDays > 366)
            {
                var finOriginal = fin;
                fin = inicio.AddDays(366);
                resultado.Detalles.Add($"Aviso: El rango solicitado ({inicio:yyyy-MM-dd} a {finOriginal:yyyy-MM-dd}) excede 366 días. Se procesó el bloque de 366 días ({inicio:yyyy-MM-dd} a {fin:yyyy-MM-dd}) para garantizar la estabilidad de la conexión. Puede continuar con el siguiente periodo a partir de {fin.AddDays(1):yyyy-MM-dd}.");
            }

            _logger.LogInformation("Iniciando ETL para rango {Inicio} a {Fin}", inicio.ToString("yyyy-MM-dd"), fin.ToString("yyyy-MM-dd"));

            // 1. Pre-descarga paralela de alto rendimiento para días pendientes
            var fechasPendientes = new List<DateTime>();
            for (DateTime f = inicio; f <= fin; f = f.AddDays(1))
            {
                string pathEsperado = Path.Combine(carpetaPdf, $"homicidios_{f:ddMMyyyy}.pdf");
                if (!File.Exists(pathEsperado) || new FileInfo(pathEsperado).Length <= 1000) 
                    fechasPendientes.Add(f);    
            }

            if (fechasPendientes.Count > 0)
            {
                _logger.LogInformation("Descargando {Count} boletines pendientes desde portal oficial...", fechasPendientes.Count);
                await Parallel.ForEachAsync(fechasPendientes, new ParallelOptions { MaxDegreeOfParallelism = 2 }, async (f, ct) =>
                {
                    try
                    {
                        await _pdfService.DescargarPdfAsync(f, carpetaPdf);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogDebug("Error al descargar {Fecha}: {Msg}", f.ToString("yyyy-MM-dd"), ex.Message);
                    }
                });
            }

            // 2. Procesamiento e ingesta analítica en base de datos
            int diasTotal = (int)(fin - inicio).TotalDays + 1;
            int diasOmitidosImagen = 0;
            int diasNoDisponibles = 0;

            for (DateTime fechaActual = inicio; fechaActual <= fin; fechaActual = fechaActual.AddDays(1))
            {
                try
                {
                    string rutaEsperada = Path.Combine(carpetaPdf, $"homicidios_{fechaActual:ddMMyyyy}.pdf");
                    string? pdfDescargado = File.Exists(rutaEsperada) && new FileInfo(rutaEsperada).Length > 1000 
                        ? rutaEsperada : await _pdfService.DescargarPdfAsync(fechaActual, carpetaPdf);

                    if (string.IsNullOrEmpty(pdfDescargado))
                    {
                        diasNoDisponibles++;
                        resultado.Detalles.Add($"{fechaActual:yyyy-MM-dd}: No disponible o no publicado en portal oficial CNS.");

                        try
                        {
                            using var connNoPub = (MySqlConnection)_connectionFactory.CreateConnection();
                            await connNoPub.OpenAsync();
                            using var cmdNoPub = new MySqlCommand(@"
                                INSERT INTO reportesinconsistencias (fecha, entidad_detectada, municipio_detectado, num_muertos, fuente, motivo_inconsistencia, fecha_registro)
                                VALUES (@f, 'PORTAL OFICIAL CNS / SSPC', @m, 0, 'Portal Oficial CNS', 'Boletín no disponible o no publicado en servidor oficial CNS.', NOW());", connNoPub);
                            cmdNoPub.Parameters.AddWithValue("@f", fechaActual);
                            cmdNoPub.Parameters.AddWithValue("@m", $"homicidios_{fechaActual:ddMMyyyy}.pdf");
                            await cmdNoPub.ExecuteNonQueryAsync();
                        }
                        catch { }

                        continue;
                    }
                    resultado.ArchivosDescargados++;

                    var subResultado = await ProcesarArchivoInternoAsync(pdfDescargado, fechaActual);
                    if (subResultado.TotalRegistrosExtraidos == 0 && subResultado.Detalles.Any(d => d.Contains("imagen escaneada", StringComparison.OrdinalIgnoreCase))) 
                        diasOmitidosImagen++;
                    
                    resultado.ArchivosProcesados++;
                    resultado.TotalRegistrosExtraidos += subResultado.TotalRegistrosExtraidos;
                    resultado.TotalRegistrosGuardados += subResultado.TotalRegistrosGuardados;
                    resultado.TotalInconsistencias += subResultado.TotalInconsistencias;
                    resultado.Detalles.AddRange(subResultado.Detalles);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al procesar fecha {Fecha}", fechaActual.ToString("yyyy-MM-dd"));
                    resultado.Detalles.Add($"{fechaActual:yyyy-MM-dd}: Error - {ex.Message}");
                }
            }
            resultado.Mensaje = $"Proceso finalizado. {resultado.ArchivosProcesados} archivos procesados ({resultado.TotalRegistrosGuardados} nuevos registros guardados, {diasOmitidosImagen} boletines en imagen escaneada omitidos, {diasNoDisponibles} días no publicados en servidor CNS).";
            return resultado;
        }

        public async Task<EtlResultDto> ProcesarArchivoLocalAsync(string rutaPdf, DateTime fecha)
        {
            return await ProcesarArchivoInternoAsync(rutaPdf, fecha);
        }

        private async Task<EtlResultDto> ProcesarArchivoInternoAsync(string rutaPdf, DateTime fecha)
        {
            var res = new EtlResultDto { Exitoso = true };
            var registrosExtraidos = _pdfService.ExtraerRegistrosDePdf(rutaPdf, fecha);
            res.TotalRegistrosExtraidos = registrosExtraidos.Count;

            if (registrosExtraidos.Count == 0)
            {
                bool esImagenEscaneada = false;
                try
                {
                    using var reader = new PdfReader(rutaPdf);
                    using var pdfDoc = new PdfDocument(reader);
                    if (pdfDoc.GetNumberOfPages() > 0)
                    {
                        string p1 = PdfTextExtractor.GetTextFromPage(pdfDoc.GetPage(1), new LocationTextExtractionStrategy()) ?? "";
                        if (p1.Trim().Length < 50) esImagenEscaneada = true;
                    }
                }
                catch { }

                if (esImagenEscaneada)
                {
                    res.Detalles.Add($"{fecha:yyyy-MM-dd}: Boletín oficial emitido por el gobierno como imagen escaneada (sin texto vectorial). Omitido para mantener la integridad forense.");
                    try
                    {
                        using var connImg = (MySqlConnection)_connectionFactory.CreateConnection();
                        await connImg.OpenAsync();
                        using var cmdIncImg = new MySqlCommand(@"
                            INSERT INTO reportesinconsistencias (fecha, entidad_detectada, municipio_detectado, num_muertos, fuente, motivo_inconsistencia, fecha_registro)
                            VALUES (@f, @e, @m, @num, @fue, @mot, @reg);", connImg);
                        cmdIncImg.Parameters.AddWithValue("@f", fecha);
                        cmdIncImg.Parameters.AddWithValue("@e", "BOLETÍN OFICIAL CNS / SSPC");
                        cmdIncImg.Parameters.AddWithValue("@m", Path.GetFileName(rutaPdf));
                        cmdIncImg.Parameters.AddWithValue("@num", 0);
                        cmdIncImg.Parameters.AddWithValue("@fue", "Secretaría de Seguridad y Protección Ciudadana (SSPC)");
                        cmdIncImg.Parameters.AddWithValue("@mot", "Boletín emitido como imagen escaneada (sin capa de texto digital). Omitido para mantener la integridad forense.");
                        cmdIncImg.Parameters.AddWithValue("@reg", DateTime.Now);
                        await cmdIncImg.ExecuteNonQueryAsync();
                    }
                    catch (Exception exLog)
                    {
                        _logger.LogWarning("No se pudo registrar boletín escaneado en reportesinconsistencias: {Msg}", exLog.Message);
                    }
                }
                else
                {
                    res.Detalles.Add($"{fecha:yyyy-MM-dd}: 0 registros encontrados o formato no reconocido.");
                }
                return res;
            }

            using var conn = (MySqlConnection)_connectionFactory.CreateConnection();
            await conn.OpenAsync();

            // 1. Asegurar tabla de inconsistencias
            using (var cmdInit = new MySqlCommand(@"
                CREATE TABLE IF NOT EXISTS reportesinconsistencias (
                    id INT AUTO_INCREMENT PRIMARY KEY,
                    fecha DATETIME,
                    entidad_detectada VARCHAR(255),
                    municipio_detectado VARCHAR(255),
                    num_muertos INT,
                    fuente VARCHAR(500),
                    motivo_inconsistencia VARCHAR(500),
                    fecha_registro DATETIME DEFAULT CURRENT_TIMESTAMP
                );", conn))
            {
                await cmdInit.ExecuteNonQueryAsync();
            }

            // 2. Cache local de Entidades (Nombre Mayúsculas -> Id)
            var entidadesDb = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            using (var cmdEnt = new MySqlCommand("SELECT id, UPPER(nombre) FROM entidad;", conn))
            using (var rEnt = await cmdEnt.ExecuteReaderAsync())
            {
                while (await rEnt.ReadAsync())
                {
                    entidadesDb[rEnt.GetString(1).Trim()] = rEnt.GetInt32(0);
                }
            }

            // 3. Cache local de Municipios (EntidadId + "_" + NombreMun -> Id)
            var municipiosDb = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            using (var cmdMun = new MySqlCommand("SELECT id, UPPER(nombre), entidad_id FROM municipio;", conn))
            using (var rMun = await cmdMun.ExecuteReaderAsync())
            {
                while (await rMun.ReadAsync())
                {
                    string key = $"{rMun.GetInt32(2)}_{rMun.GetString(1).Trim()}";
                    municipiosDb[key] = rMun.GetInt32(0);
                }
            }

            // 4. Registros ya existentes para esta fecha (para evitar duplicados exactos)
            var registrosExistentes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            using (var cmdExist = new MySqlCommand("SELECT municipio_id, num_muertos, COALESCE(fuente, '') FROM homicidios WHERE fecha = @fecha;", conn))
            {
                cmdExist.Parameters.AddWithValue("@fecha", fecha.Date);
                using var rExist = await cmdExist.ExecuteReaderAsync();
                while (await rExist.ReadAsync())
                {
                    int mId = rExist.GetInt32(0);
                    int nMuertos = rExist.GetInt32(1);
                    string fuente = rExist.GetString(2);
                    registrosExistentes.Add($"{mId}_{nMuertos}_{fuente}");
                }
            }

            int guardados = 0;
            int inconsistencias = 0;

            foreach (var reg in registrosExtraidos)
            {
                try
                {
                    bool esInconsistenciaOCorreccion = reg.TuvoCorreccion || reg.RequiereRevision || 
                        reg.Entidad == "NO IDENTIFICADA" || !string.IsNullOrWhiteSpace(reg.MotivoCorreccion);

                    if (esInconsistenciaOCorreccion)
                    {
                        inconsistencias++;
                        try
                        {
                            using var cmdInc = new MySqlCommand(@"
                                INSERT INTO reportesinconsistencias (fecha, entidad_detectada, municipio_detectado, num_muertos, fuente, motivo_inconsistencia, fecha_registro)
                                VALUES (@f, @e, @m, @num, @fue, @mot, @reg);", conn);
                            cmdInc.Parameters.AddWithValue("@f", reg.Fecha);
                            cmdInc.Parameters.AddWithValue("@e", (object?)reg.EntidadRaw ?? reg.Entidad);
                            cmdInc.Parameters.AddWithValue("@m", (object?)reg.MunicipioRaw ?? reg.Municipio);
                            cmdInc.Parameters.AddWithValue("@num", reg.NumMuertos);
                            cmdInc.Parameters.AddWithValue("@fue", (object?)reg.Fuente ?? DBNull.Value);
                            cmdInc.Parameters.AddWithValue("@mot", (object?)reg.MotivoCorreccion ?? reg.DetalleInferencia ?? "Corrección requerida");
                            cmdInc.Parameters.AddWithValue("@reg", DateTime.Now);
                            await cmdInc.ExecuteNonQueryAsync();
                        }
                        catch (Exception exInc)
                        {
                            _logger.LogWarning("No se pudo registrar inconsistencia: {Msg}", exInc.Message);
                        }
                    }

                    // A) Resolver o crear Entidad
                    string nombreEnt = string.IsNullOrWhiteSpace(reg.Entidad) ? "NO ESPECIFICADO" : reg.Entidad.Trim().ToUpperInvariant();
                    if (!entidadesDb.TryGetValue(nombreEnt, out int entidadId))
                    {
                        using var cmdInsEnt = new MySqlCommand("INSERT INTO entidad (nombre) VALUES (@nombre); SELECT LAST_INSERT_ID();", conn);
                        cmdInsEnt.Parameters.AddWithValue("@nombre", nombreEnt);
                        entidadId = Convert.ToInt32(await cmdInsEnt.ExecuteScalarAsync());
                        entidadesDb[nombreEnt] = entidadId;
                    }

                    // B) Resolver o crear Municipio
                    string nombreMun = string.IsNullOrWhiteSpace(reg.Municipio) ? "NO ESPECIFICADO" : reg.Municipio.Trim().ToUpperInvariant();
                    string munKey = $"{entidadId}_{nombreMun}";
                    if (!municipiosDb.TryGetValue(munKey, out int municipioId))
                    {
                        using var cmdInsMun = new MySqlCommand("INSERT INTO municipio (nombre, entidad_id) VALUES (@nombre, @entId); SELECT LAST_INSERT_ID();", conn);
                        cmdInsMun.Parameters.AddWithValue("@nombre", nombreMun);
                        cmdInsMun.Parameters.AddWithValue("@entId", entidadId);
                        municipioId = Convert.ToInt32(await cmdInsMun.ExecuteScalarAsync());
                        municipiosDb[munKey] = municipioId;
                    }

                    // C) Evitar duplicados
                    string fuenteStr = reg.Fuente ?? string.Empty;
                    string existKey = $"{municipioId}_{reg.NumMuertos}_{fuenteStr}";

                    if (!registrosExistentes.Contains(existKey))
                    {
                        using var cmdInsHom = new MySqlCommand(@"
                            INSERT INTO homicidios (municipio_id, fecha, num_muertos, hombres, mujeres, no_identificado, fuente)
                            VALUES (@mId, @fecha, @muertos, @hombres, @mujeres, @noId, @fuente);", conn);
                        cmdInsHom.Parameters.AddWithValue("@mId", municipioId);
                        cmdInsHom.Parameters.AddWithValue("@fecha", reg.Fecha.Date);
                        cmdInsHom.Parameters.AddWithValue("@muertos", reg.NumMuertos);
                        cmdInsHom.Parameters.AddWithValue("@hombres", reg.Hombres);
                        cmdInsHom.Parameters.AddWithValue("@mujeres", reg.Mujeres);
                        cmdInsHom.Parameters.AddWithValue("@noId", reg.NoIdentificado);
                        cmdInsHom.Parameters.AddWithValue("@fuente", (object?)reg.Fuente ?? DBNull.Value);

                        await cmdInsHom.ExecuteNonQueryAsync();
                        registrosExistentes.Add(existKey);
                        guardados++;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al persistir registro: {Entidad} - {Municipio}", reg.Entidad, reg.Municipio);
                }
            }

            res.TotalRegistrosGuardados = guardados;
            res.TotalInconsistencias = inconsistencias;
            res.Detalles.Add($"{fecha:yyyy-MM-dd}: {registrosExtraidos.Count} extraídos, {guardados} nuevos guardados, {inconsistencias} anomalías registradas.");
            return res;
        }
    }
}
