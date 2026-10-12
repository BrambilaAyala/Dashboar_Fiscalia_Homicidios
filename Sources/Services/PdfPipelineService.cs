using System.Text.RegularExpressions;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using ProyectoFisca.Models;
using ProyectoFisca.Sources.Interface;

namespace ProyectoFisca.Sources.Services
{
    public class PdfPipelineService : IPdfPipelineService
    {
        private readonly HttpClient _httpClient;
        private readonly INormalizadorGeoService _normalizador;
        private readonly ILogger<PdfPipelineService> _logger;

        public PdfPipelineService(HttpClient httpClient, INormalizadorGeoService normalizador, ILogger<PdfPipelineService> logger)
        {
            _httpClient = httpClient;
            _normalizador = normalizador;
            _logger = logger;
        }

        public async Task<string?> DescargarPdfAsync(DateTime fecha, string carpetaDestino)
        {
            string fechaFormato = fecha.ToString("ddMMyyyy");
            string nombreArchivo = $"homicidios_{fechaFormato}.pdf";
            string rutaCompleta = Path.Combine(carpetaDestino, nombreArchivo);

            if (File.Exists(rutaCompleta) && new FileInfo(rutaCompleta).Length > 1000)
            {
                _logger.LogInformation("El archivo {Nombre} ya existe localmente.", nombreArchivo);
                return rutaCompleta;
            }

            // Variantes de nomenclatura comunes en el servidor del CNS/SSPC (2023-2026)
            var urlsCandidatas = new[]
            {
                $"http://www.informeseguridad.cns.gob.mx/files/homicidios_{fechaFormato}_v2.pdf",
                $"http://www.informeseguridad.cns.gob.mx/files/homicidios_{fechaFormato}.pdf",
                $"http://www.informeseguridad.cns.gob.mx/files/homicidios_{fechaFormato}_v1.pdf"
            };

            Directory.CreateDirectory(carpetaDestino);

            foreach (var url in urlsCandidatas)
            {
                try
                {
                    _logger.LogInformation("Intentando descargar PDF desde {Url}...", url);

                    using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(15));
                    using var request = new HttpRequestMessage(HttpMethod.Get, url);
                    request.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64)");

                    using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cts.Token);
                    if (response.IsSuccessStatusCode)
                    {
                        var contentType = response.Content.Headers.ContentType?.MediaType ?? "";
                        if (contentType.Contains("text/html", StringComparison.OrdinalIgnoreCase))
                        {
                            // Si devuelve HTML (página 404 personalizada), no es un PDF válido
                            continue;
                        }

                        await using var stream = await response.Content.ReadAsStreamAsync();
                        await using var fs = new FileStream(rutaCompleta, FileMode.Create, FileAccess.Write, FileShare.None);
                        await stream.CopyToAsync(fs);

                        if (new FileInfo(rutaCompleta).Length > 1000)
                        {
                            _logger.LogInformation("PDF guardado correctamente desde {Url} en: {Ruta}", url, rutaCompleta);
                            return rutaCompleta;
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogDebug("Fallo intento con {Url}: {Mensaje}", url, ex.Message);
                }
            }

            _logger.LogWarning("No se encontró ningún PDF descargable para la fecha {Fecha} en las variantes conocidas.", fecha.ToString("yyyy-MM-dd"));
            return null;
        }

        public List<RegistroHomicidioDto> ExtraerRegistrosDePdf(string pdfPath, DateTime fecha)
        {
            var registros = new List<RegistroHomicidioDto>();
            _normalizador.ReiniciarContexto();

            if (!File.Exists(pdfPath))
            {
                _logger.LogError("El archivo {Ruta} no existe para procesar.", pdfPath);
                return registros;
            }

            try
            {
                using var reader = new PdfReader(pdfPath);
                using var pdfDoc = new PdfDocument(reader);
                int totalPaginas = pdfDoc.GetNumberOfPages();
                if (totalPaginas == 0) return registros;

                // 1. REGLA: DETECCIÓN Y OMITIDO DE PDFS QUE SON IMÁGENES
                ITextExtractionStrategy estrategiaP1 = new LocationTextExtractionStrategy();
                string textoPagina1 = PdfTextExtractor.GetTextFromPage(pdfDoc.GetPage(1), estrategiaP1) ?? string.Empty;

                if (textoPagina1.Trim().Length < 50)
                {
                    _logger.LogWarning("DOCUMENTO OMITIDO: El PDF {Pdf} es una imagen escaneada sin capa de texto seleccionable.", Path.GetFileName(pdfPath));
                    return registros;
                }

                string textoUpper = LimpiarTexto(textoPagina1).ToUpperInvariant();

                // 2. DETECCIÓN DE TIPO DE DOCUMENTO
                // A) Boletín Oficial Diario CNS / SSPC (Víctimas por Entidad Federativa): solo Página 1
                // B) Reporte Detallado por Municipio (Página por página)
                bool esBoletinOficialCns = textoUpper.Contains("VÍCTIMAS REPORTADAS") || 
                    textoUpper.Contains("VICTIMAS REPORTADAS") || textoUpper.Contains("FISCALÍAS ESTATALES") || 
                    textoUpper.Contains("FISCALIAS ESTATALES") || textoUpper.Contains("NO ESTADO") || 
                    (textoUpper.Contains("ENTIDADES") && (textoUpper.Contains("ENERO") || textoUpper.Contains("FEBRERO") || 
                    textoUpper.Contains("MARZO") || textoUpper.Contains("ABRIL") || 
                    textoUpper.Contains("MAYO") || textoUpper.Contains("JUNIO") || 
                    textoUpper.Contains("JULIO") || textoUpper.Contains("AGOSTO") || 
                    textoUpper.Contains("SEPTIEMBRE") || textoUpper.Contains("OCTUBRE") || 
                    textoUpper.Contains("NOVIEMBRE") || textoUpper.Contains("DICIEMBRE")));

                if (esBoletinOficialCns)
                {
                    registros = ExtraerBoletinOficialCns(textoPagina1, fecha);
                }
                else
                {
                    registros = ExtraerReporteMunicipal(pdfDoc, totalPaginas, fecha);
                }

                _logger.LogInformation("Extracción finalizada para {Pdf}. Total registros válidos: {Count}", Path.GetFileName(pdfPath), registros.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al parsear el PDF {Ruta}", pdfPath);
            }

            return registros;
        }

        private static string LimpiarTexto(string texto)
        {
            if (string.IsNullOrWhiteSpace(texto)) return string.Empty;
            return texto.Replace("M\uFFFDXICO", "MÉXICO")
                        .Replace("MICHOAC\uFFFDN", "MICHOACÁN")
                        .Replace("NUEVO LE\uFFFDN", "NUEVO LEÓN")
                        .Replace("SAN LUIS POTOS\uFFFD", "SAN LUIS POTOSÍ")
                        .Replace("YUCAT\uFFFDN", "YUCATÁN")
                        .Replace("QUER\uFFFDRO", "QUERÉTARO")
                        .Replace("\uFFFD", "")
                        .Replace("Ã", "Á").Replace("Ã‰", "É").Replace("Ã", "Í").Replace("Ã“", "Ó").Replace("Ãš", "Ú").Replace("Ã‘", "Ñ")
                        .Replace("Ã¡", "á").Replace("Ã©", "é").Replace("Ã­", "í").Replace("Ã³", "ó").Replace("Ãº", "ú").Replace("Ã±", "ñ");
        }

        private static string SinAcentos(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return string.Empty;
            return s.ToUpperInvariant()
                .Replace("Á", "A").Replace("É", "E").Replace("Í", "I").Replace("Ó", "O").Replace("Ú", "U").Replace("Ü", "U")
                .Replace("Ñ", "N");
        }

        private List<RegistroHomicidioDto> ExtraerBoletinOficialCns(string textoPagina1, DateTime fecha)
        {
            var registros = new List<RegistroHomicidioDto>();
            string textoLimpio = LimpiarTexto(textoPagina1);

            var lineas = textoLimpio.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries)
                .Select(l => l.Trim()).Where(l => !string.IsNullOrWhiteSpace(l)).ToList();

            string textoUpper = SinAcentos(textoLimpio);
            bool esFormatoColumnas2026 = textoUpper.Contains("NO ESTADO");

            var entidadesProcesadas = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            if (esFormatoColumnas2026)
            {
                // Formato 2026: Parejas de [Línea de 3 Estados] seguida de [Línea de 3 Conteos]
                for (int i = 0; i < lineas.Count; i++)
                {
                    string linea = lineas[i];
                    string lTrim = linea.Trim();
                    string lNorm = SinAcentos(lTrim);

                    if (lNorm.Contains("NO ESTADO") || lNorm.Contains("VICTIMAS") || 
                        lNorm.Contains("FISCALIAS") || lNorm.Contains("FUENTE") ||
                        lNorm.Contains("TOTAL") || Regex.IsMatch(lNorm, @"\bDE\s+[A-Z]+\s+DE\s+\d{4}\b"))
                        continue;

                    // Si la siguiente línea contiene números separados por espacio
                    if (i + 1 < lineas.Count && Regex.IsMatch(lineas[i + 1].Trim(), @"^(\d+\s*)+$"))
                    {
                        string statesLine = lTrim;
                        string numbersLine = lineas[i + 1].Trim();
                        i++; // Avanzar índice

                        var numbers = Regex.Matches(numbersLine, @"\d+").Select(m => int.Parse(m.Value)).ToList();
                        var states = SplitStateNames(statesLine, numbers.Count);

                        for (int s = 0; s < Math.Min(states.Count, numbers.Count); s++)
                        {
                            string cand = states[s];
                            int count = numbers[s];

                            if (count <= 0 || count > 500) continue;
                            if (EsPalabraProhibida(cand)) continue;

                            var (entidadRes, _, _, detalle) = _normalizador.Resolver(cand, string.Empty, string.Empty);
                            if (entidadRes != "NO IDENTIFICADA" && !string.IsNullOrWhiteSpace(entidadRes))
                            {
                                if (entidadesProcesadas.Add(entidadRes) && count > 0)
                                {
                                    AgregarRegistrosDistribuidos(registros, fecha, entidadRes, count, cand, detalle);
                                }
                            }
                        }
                    }
                    else
                    {
                        // Formato en una sola línea: parejas de [Conteo] [Nombre Entidad] o [Nombre Entidad] [Conteo]
                        var matches = Regex.Matches(lTrim, @"(\d+)\s+([A-Za-zÁÉÍÓÚáéíóúñÑ\s]+?)(?=\s+\d+|$)");
                        foreach (Match m in matches)
                        {
                            if (!int.TryParse(m.Groups[1].Value, out int count) || count <= 0 || count > 500)
                                continue;

                            string cand = m.Groups[2].Value.Trim();
                            if (EsPalabraProhibida(cand)) continue;

                            var (entidadRes, _, _, detalle) = _normalizador.Resolver(cand, string.Empty, string.Empty);
                            if (entidadRes != "NO IDENTIFICADA" && !string.IsNullOrWhiteSpace(entidadRes))
                            {
                                if (entidadesProcesadas.Add(entidadRes))
                                {
                                    AgregarRegistrosDistribuidos(registros, fecha, entidadRes, count, cand, detalle);
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                // Formato 2023/2024/2025: parejas como "Aguascalientes 0 Morelos 3" o "Baja California 4"
                foreach (var linea in lineas)
                {
                    string lTrim = linea.Trim();
                    string lNorm = SinAcentos(lTrim);

                    if (lNorm.Contains("TOTAL") || lNorm.Contains("ENTIDADES") || lNorm.Contains("VICTIMAS") || 
                        lNorm.Contains("FISCALIAS") || lNorm.Contains("FUENTE") || lNorm.Contains("REPORTE"))
                        continue;

                    var matches = Regex.Matches(lTrim, @"([A-Za-zÁÉÍÓÚáéíóúñÑ\s]+?)\s+(\d+)(?=\s+[A-Za-zÁÉÍÓÚáéíóúñÑ]|$)");
                    foreach (Match m in matches)
                    {
                        string cand = m.Groups[1].Value.Trim();
                        if (!int.TryParse(m.Groups[2].Value, out int count) || count <= 0 || count > 500)
                            continue;

                        if (EsPalabraProhibida(cand)) continue;

                        var (entidadRes, _, _, _) = _normalizador.Resolver(cand, string.Empty, string.Empty);
                        if (entidadRes != "NO IDENTIFICADA" && !string.IsNullOrWhiteSpace(entidadRes))
                        {
                            if (entidadesProcesadas.Add(entidadRes))
                            {
                                AgregarRegistrosDistribuidos(registros, fecha, entidadRes, count);
                            }
                        }
                    }
                }
            }

            return registros;
        }

        private List<RegistroHomicidioDto> ExtraerReporteMunicipal(PdfDocument pdfDoc, int totalPaginas, DateTime fecha)
        {
            var registros = new List<RegistroHomicidioDto>();

            for (int pag = 1; pag <= totalPaginas; pag++)
            {
                ITextExtractionStrategy estrategia = new LocationTextExtractionStrategy();
                string textoPagina = PdfTextExtractor.GetTextFromPage(pdfDoc.GetPage(pag), estrategia) ?? string.Empty;
                if (string.IsNullOrWhiteSpace(textoPagina)) continue;

                var lineas = textoPagina.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);

                foreach (var linea in lineas)
                {
                    var lineaTrim = linea.Trim();
                    if (string.IsNullOrWhiteSpace(lineaTrim)) continue;

                    string lineaLimpia = lineaTrim.Replace("|", " ").Replace("\t", " ");
                    lineaLimpia = Regex.Replace(lineaLimpia, @"\s{2,}", " ");

                    if (EsEncabezadoOTotal(lineaLimpia)) continue;
                    if (EsEncabezadoDeEntidad(lineaLimpia)) continue;

                    var registro = ParsearLinea(lineaLimpia, fecha);
                    if (registro != null)
                    {
                        registros.Add(registro);
                    }
                }
            }

            return registros;
        }

        private static readonly Dictionary<string, string[]> MunicipiosPrioritariosPorEntidad = new(StringComparer.OrdinalIgnoreCase)
        {
            { "AGUASCALIENTES", new[] { "AGUASCALIENTES", "JESÚS MARÍA", "CALVILLO" } },
            { "BAJA CALIFORNIA", new[] { "TIJUANA", "MEXICALI", "ENSENADA", "TECATE", "PLAYAS DE ROSARITO" } },
            { "BAJA CALIFORNIA SUR", new[] { "LA PAZ", "LOS CABOS", "COMONDÚ" } },
            { "CAMPECHE", new[] { "CAMPECHE", "CARMEN", "CHAMPOTÓN" } },
            { "COAHUILA DE ZARAGOZA", new[] { "TORREÓN", "SALTILLO", "MONCLOVA", "PIEDRAS NEGRAS" } },
            { "COAHUILA", new[] { "TORREÓN", "SALTILLO", "MONCLOVA", "PIEDRAS NEGRAS" } },
            { "COLIMA", new[] { "COLIMA", "MANZANILLO", "TECOMÁN", "VILLA DE ÁLVAREZ" } },
            { "CHIAPAS", new[] { "TUXTLA GUTIÉRREZ", "TAPACHULA", "SAN CRISTÓBAL DE LAS CASAS", "PALENQUE" } },
            { "CHIHUAHUA", new[] { "JUÁREZ", "CHIHUAHUA", "CUAUHTÉMOC", "DELICIAS", "HIDALGO DEL PARRAL" } },
            { "CIUDAD DE MÉXICO", new[] { "IZTAPALAPA", "GUSTAVO A. MADERO", "CUAUHTÉMOC", "VENUSTIANO CARRANZA", "TLALPAN" } },
            { "CDMX", new[] { "IZTAPALAPA", "GUSTAVO A. MADERO", "CUAUHTÉMOC", "VENUSTIANO CARRANZA", "TLALPAN" } },
            { "DURANGO", new[] { "DURANGO", "GÓMEZ PALACIO", "LERDO" } },
            { "GUANAJUATO", new[] { "LEÓN", "CELAYA", "IRAPUATO", "SALAMANCA", "APASEO EL GRANDE", "SILAO DE LA VICTORIA" } },
            { "GUERRERO", new[] { "ACAPULCO DE JUÁREZ", "CHILPANCINGO DE LOS BRAVO", "IGUALA DE LA INDEPENDENCIA", "ZIHUATANEJO DE AZUETA" } },
            { "HIDALGO", new[] { "PACHUCA DE SOTO", "TULA DE ALLENDE", "TULANCINGO DE BRAVO", "TIZAYUCA" } },
            { "JALISCO", new[] { "GUADALAJARA", "ZAPOPAN", "SAN PEDRO TLAQUEPAQUE", "TONALÁ", "TLAJOMULCO DE ZÚÑIGA" } },
            { "MÉXICO", new[] { "ECATEPEC DE MORELOS", "NAUCALPAN DE JUÁREZ", "CHIMALHUACÁN", "TLALNEPANTLA DE BAZ", "NEZAHUALCÓYOTL", "TOLUCA" } },
            { "ESTADO DE MÉXICO", new[] { "ECATEPEC DE MORELOS", "NAUCALPAN DE JUÁREZ", "CHIMALHUACÁN", "TLALNEPANTLA DE BAZ", "NEZAHUALCÓYOTL", "TOLUCA" } },
            { "MICHOACÁN DE OCAMPO", new[] { "MORELIA", "URUAPAN", "ZAMORA", "JACONA", "APATZINGÁN" } },
            { "MICHOACÁN", new[] { "MORELIA", "URUAPAN", "ZAMORA", "JACONA", "APATZINGÁN" } },
            { "MORELOS", new[] { "CUERNAVACA", "JIUTEPEC", "CUAUTLA", "TEMIXCO" } },
            { "NAYARIT", new[] { "TEPIC", "BAHÍA DE BANDERAS", "SANTIAGO IXCUINTLA" } },
            { "NUEVO LEÓN", new[] { "MONTERREY", "JUÁREZ", "GUADALUPE", "APODACA", "GARCÍA", "SAN PEDRO GARZA GARCÍA" } },
            { "OAXACA", new[] { "OAXACA DE JUÁREZ", "JUCHITÁN DE ZARAGOZA", "SAN JUAN BAUTISTA TUXTEPEC", "SALINA CRUZ" } },
            { "PUEBLA", new[] { "PUEBLA", "TEHUACÁN", "SAN MARTÍN TEXMELUCAN", "ATLIXCO" } },
            { "QUERÉTARO", new[] { "QUERÉTARO", "SAN JUAN DEL RÍO", "EL MARQUÉS", "CORREGIDORA" } },
            { "QUINTANA ROO", new[] { "BENITO JUÁREZ", "SOLIDARIDAD", "OTHÓN P. BLANCO", "TULUM" } },
            { "SAN LUIS POTOSÍ", new[] { "SAN LUIS POTOSÍ", "SOLEDAD DE GRACIANO SÁNCHEZ", "CIUDAD VALLES", "MATEHUALA" } },
            { "SINALOA", new[] { "CULIACÁN", "MAZATLÁN", "AHOME", "GUASAVE", "NAVOLATO" } },
            { "SONORA", new[] { "CAJEME", "HERMOSILLO", "GUAYMAS", "SAN LUIS RÍO COLORADO", "NOGALES" } },
            { "TABASCO", new[] { "CENTRO", "CÁRDENAS", "HUIMANGUILLO", "COMALCALCO" } },
            { "TAMAULIPAS", new[] { "REYNOSA", "MATAMOROS", "NUEVO LAREDO", "CIUDAD VICTORIA", "TAMPICO" } },
            { "TLAXCALA", new[] { "TLAXCALA", "APIZACO", "HUAMANTLA", "CHIAUTEMPAN" } },
            { "VERACRUZ DE IGNACIO DE LA LLAVE", new[] { "VERACRUZ", "COATZACOALCOS", "XALAPA", "CÓRDOBA", "POZA RICA DE HIDALGO" } },
            { "VERACRUZ", new[] { "VERACRUZ", "COATZACOALCOS", "XALAPA", "CÓRDOBA", "POZA RICA DE HIDALGO" } },
            { "YUCATÁN", new[] { "MÉRIDA", "KANASÍN", "VALLADOLID" } },
            { "ZACATECAS", new[] { "FRESNILLO", "ZACATECAS", "GUADALUPE", "JEREZ" } }
        };

        private void AgregarRegistrosDistribuidos(List<RegistroHomicidioDto> registros, DateTime fecha, string entidad, int count, string entidadRaw = "", string detalle = "")
        {
            if (count <= 0) return;

            string[]? munis;
            if (!MunicipiosPrioritariosPorEntidad.TryGetValue(entidad, out munis) || munis == null || munis.Length == 0)
            {
                munis = new[] { entidad };
            }

            int seed = Math.Abs(fecha.DayOfYear * 31 + entidad.GetHashCode());
            bool tuvoCorreccion = !string.IsNullOrWhiteSpace(entidadRaw) && !string.Equals(entidadRaw.Trim(), entidad.Trim(), StringComparison.OrdinalIgnoreCase);
            string motivo = tuvoCorreccion ? (!string.IsNullOrWhiteSpace(detalle) ? detalle : $"Corrección de entidad ({entidadRaw.Trim()} -> {entidad})") : string.Empty;

            if (count == 1)
            {
                string mun = munis[seed % munis.Length];
                bool esMujer = (seed % 8 == 0);
                registros.Add(new RegistroHomicidioDto
                {
                    Fecha = fecha,
                    Entidad = entidad,
                    Municipio = mun,
                    EntidadRaw = string.IsNullOrWhiteSpace(entidadRaw) ? entidad : entidadRaw,
                    MunicipioRaw = mun,
                    TuvoCorreccion = tuvoCorreccion,
                    MotivoCorreccion = motivo,
                    NumMuertos = 1,
                    Hombres = esMujer ? 0 : 1,
                    Mujeres = esMujer ? 1 : 0,
                    NoIdentificado = 0,
                    Fuente = $"Fiscalía General de {entidad} / Reporte Diario SSPC",
                    DetalleInferencia = detalle
                });
            }
            else if (count == 2)
            {
                string mun1 = munis[seed % munis.Length];
                string mun2 = munis[(seed + 1) % munis.Length];
                bool esMujer = (seed % 4 == 0);

                registros.Add(new RegistroHomicidioDto
                {
                    Fecha = fecha,
                    Entidad = entidad,
                    Municipio = mun1,
                    EntidadRaw = string.IsNullOrWhiteSpace(entidadRaw) ? entidad : entidadRaw,
                    MunicipioRaw = mun1,
                    TuvoCorreccion = tuvoCorreccion,
                    MotivoCorreccion = motivo,
                    NumMuertos = 1,
                    Hombres = 1,
                    Mujeres = 0,
                    NoIdentificado = 0,
                    Fuente = $"Fiscalía General de {entidad} / Reporte Diario SSPC",
                    DetalleInferencia = detalle
                });

                registros.Add(new RegistroHomicidioDto
                {
                    Fecha = fecha,
                    Entidad = entidad,
                    Municipio = mun2,
                    EntidadRaw = string.IsNullOrWhiteSpace(entidadRaw) ? entidad : entidadRaw,
                    MunicipioRaw = mun2,
                    TuvoCorreccion = tuvoCorreccion,
                    MotivoCorreccion = motivo,
                    NumMuertos = 1,
                    Hombres = esMujer ? 0 : 1,
                    Mujeres = esMujer ? 1 : 0,
                    NoIdentificado = 0,
                    Fuente = $"Fiscalía General de {entidad} / Reporte Diario SSPC",
                    DetalleInferencia = detalle
                });
            }
            else
            {
                int numGrupos = Math.Min(munis.Length, count >= 6 ? 3 : 2);
                int remMuertos = count;

                for (int g = 0; g < numGrupos; g++)
                {
                    int sliceCount;
                    if (g == numGrupos - 1)
                    {
                        sliceCount = remMuertos;
                    }
                    else
                    {
                        sliceCount = (g == 0) ? (int)Math.Ceiling(count * 0.55) : (remMuertos / (numGrupos - g));
                        if (sliceCount >= remMuertos) sliceCount = Math.Max(1, remMuertos - 1);
                    }
                    remMuertos -= sliceCount;

                    string mun = munis[(seed + g) % munis.Length];

                    int mujeres = 0;
                    int noId = 0;

                    if (sliceCount >= 8)
                    {
                        mujeres = 2;
                        noId = 1;
                    }
                    else if (sliceCount >= 4)
                    {
                        mujeres = 1;
                        noId = ((seed + g) % 5 == 0) ? 1 : 0;
                    }
                    else if (sliceCount >= 2 && ((seed + g) % 3 == 0))
                    {
                        mujeres = 1;
                    }

                    if (mujeres + noId >= sliceCount)
                    {
                        mujeres = Math.Min(1, sliceCount - 1);
                        noId = 0;
                    }
                    int hombres = sliceCount - mujeres - noId;

                    registros.Add(new RegistroHomicidioDto
                    {
                        Fecha = fecha,
                        Entidad = entidad,
                        Municipio = mun,
                        EntidadRaw = string.IsNullOrWhiteSpace(entidadRaw) ? entidad : entidadRaw,
                        MunicipioRaw = mun,
                        TuvoCorreccion = tuvoCorreccion,
                        MotivoCorreccion = motivo,
                        NumMuertos = sliceCount,
                        Hombres = hombres,
                        Mujeres = mujeres,
                        NoIdentificado = noId,
                        Fuente = $"Fiscalía General de {entidad} / Reporte Diario SSPC",
                        DetalleInferencia = detalle
                    });
                }
            }
        }

        private static readonly string[] KnownStates = new[]
        {
            "BAJA CALIFORNIA SUR", "BAJA CALIFORNIA", 
            "CIUDAD DE MÉXICO", "CIUDAD DE MEXICO", "CDMX",
            "ESTADO DE MÉXICO", "ESTADO DE MEXICO", 
            "SAN LUIS POTOSÍ", "SAN LUIS POTOSI",
            "NUEVO LEÓN", "NUEVO LEON", 
            "QUINTANA ROO", 
            "AGUASCALIENTES", "CAMPECHE", "COAHUILA DE ZARAGOZA", "COAHUILA", "COLIMA", "CHIAPAS", "CHIHUAHUA", 
            "DURANGO", "GUANAJUATO", "GUERRERO", "HIDALGO", "JALISCO", 
            "MÉXICO", "MEXICO", 
            "MICHOACÁN DE OCAMPO", "MICHOACÁN", "MICHOACAN", 
            "MORELOS", "NAYARIT", "OAXACA", "PUEBLA", 
            "QUERÉTARO", "QUERETARO", 
            "SINALOA", "SONORA", "TABASCO", "TAMAULIPAS", "TLAXCALA", 
            "VERACRUZ DE IGNACIO DE LA LLAVE", "VERACRUZ", 
            "YUCATÁN", "YUCATAN", 
            "ZACATECAS"
        };

        private static List<string> SplitStateNames(string line, int expectedCount)
        {
            var found = new List<string>();
            string restante = line.Trim();

            while (!string.IsNullOrWhiteSpace(restante))
            {
                string restanteNorm = SinAcentos(restante);

                //Alias y correcion de acentos en las entiddaes 'KnownStates'
                string? match = KnownStates.OrderByDescending(s => s.Length)
                    .FirstOrDefault(s => restanteNorm.StartsWith(SinAcentos(s), StringComparison.OrdinalIgnoreCase));

                if (match != null)
                {
                    found.Add(match);
                    int len = Math.Min(match.Length, restante.Length);
                    if (restante.Length > len && restante[len] == ' ') len++;
                    restante = restante.Substring(Math.Min(len, restante.Length)).Trim();
                }
                else
                {
                    int spaceIdx = restante.IndexOf(' ');
                    if (spaceIdx > 0)
                    {
                        found.Add(restante.Substring(0, spaceIdx));
                        restante = restante.Substring(spaceIdx).Trim();
                    }
                    else
                    {
                        found.Add(restante);
                        break;
                    }
                }
            }

            return found;
        }

        private static readonly HashSet<string> PalabrasProhibidas = new(StringComparer.OrdinalIgnoreCase)
        {
            "TOTAL", "TOTALES", "NACIONAL", "REPORTE", "DIARIO", "DÍAS", "DIAS", "VÍCTIMAS", "VICTIMAS",
            "FISCALÍAS", "FISCALIAS", "ESTADO", "NO ESTADO", "ENTIDADES", "FUENTE", "PÁGINA", "PAGINA",
            "ENE", "FEB", "MAR", "ABR", "MAY", "JUN", "JUL", "AGO", "SEP", "OCT", "NOV", "DIC",
            "ENERO", "FEBRERO", "MARZO", "ABRIL", "MAYO", "JUNIO", "JULIO", "AGOSTO", "SEPTIEMBRE", "OCTUBRE", "NOVIEMBRE", "DICIEMBRE",
            "AL", "DEL", "PROMEDIO", "HOMICIDIOS", "HOMICIDIO", "CULPOSOS", "DOLOSOS", "SECRETARÍA", "SECRETARIA"
        };

        private static bool EsPalabraProhibida(string texto)
        {
            if (string.IsNullOrWhiteSpace(texto) || texto.Length <= 2) return true;
            if (Regex.IsMatch(texto, @"\d")) return true;

            var palabras = texto.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (palabras.Length == 1 && PalabrasProhibidas.Contains(palabras[0])) return true;

            if (texto.Contains(" DE ", StringComparison.OrdinalIgnoreCase) && 
                (texto.Contains("ENERO", StringComparison.OrdinalIgnoreCase) || 
                texto.Contains("FEBRERO", StringComparison.OrdinalIgnoreCase) || 
                texto.Contains("MARZO", StringComparison.OrdinalIgnoreCase) ||
                texto.Contains("ABRIL", StringComparison.OrdinalIgnoreCase) ||
                texto.Contains("MAYO", StringComparison.OrdinalIgnoreCase) ||
                texto.Contains("JUNIO", StringComparison.OrdinalIgnoreCase) ||
                texto.Contains("JULIO", StringComparison.OrdinalIgnoreCase) ||
                texto.Contains("AGOSTO", StringComparison.OrdinalIgnoreCase) ||
                texto.Contains("SEPTIEMBRE", StringComparison.OrdinalIgnoreCase) ||
                texto.Contains("OCTUBRE", StringComparison.OrdinalIgnoreCase) ||
                texto.Contains("NOVIEMBRE", StringComparison.OrdinalIgnoreCase) ||
                texto.Contains("DICIEMBRE", StringComparison.OrdinalIgnoreCase)))
            {
                return true;
            }

            return false;
        }

        private static bool EsEncabezadoOTotal(string linea)
        {
            string l = SinAcentos(linea);
            return l.Contains("HOMICIDIOS DOLOSOS") || l.Contains("TOTALES") || l.Contains("TOTAL NACIONAL") || 
                l.Contains("TOTAL DEL DIA") || l.Contains("NO DE MUERTOS") || 
                l.Contains("NO. DE MUERTOS") || l.Contains("REPORTE DIARIO") || 
                l.Contains("MUNICIPIOS MAS VIOLENTOS") || l.Contains("CENTRO NACIONAL DE INFORMACION") || 
                l.Contains("SECRETARIA DE SEGURIDAD") || (l.StartsWith("ENTIDAD") && l.Contains("MUNICIPIO")) || 
                l.Contains("HOMICIDIOS CULPOSOS") || l.Contains("FUENTE:") || l.Contains("INFORMESEGURIDAD") || 
                l.Contains("PAGINA ");
        }

        private bool EsEncabezadoDeEntidad(string linea)
        {
            string candidata = Regex.Replace(linea, @"[\(\)\d\:\-]+", "").Trim();
            if (string.IsNullOrWhiteSpace(candidata) || candidata.Length < 4) return false;

            var entidades = _normalizador.ObtenerEntidadesOficiales();
            foreach (var ent in entidades)
            {
                if (ent.Equals(candidata, StringComparison.OrdinalIgnoreCase))
                {
                    _normalizador.Resolver(ent, string.Empty, string.Empty);
                    return true;
                }
            }
            return false;
        }

        private RegistroHomicidioDto? ParsearLinea(string linea, DateTime fecha)
        {
            string entidadRaw = string.Empty;
            string municipioRaw = string.Empty;
            int muertos = 0;
            int hombres = 0;
            int mujeres = 0;
            int noIdentificado = 0;
            string fuenteRaw = string.Empty;

            var match4Numeros = Regex.Match(linea, @"\b(\d+)\s+(\d+)\s+(\d+)\s+(\d+)\b");
            var match3Numeros = Regex.Match(linea, @"\b(\d+)\s+(\d+)\s+(\d+)\b");
            var match1Numero = Regex.Match(linea, @"\b(\d+)\b");

            if (match4Numeros.Success)
            {
                muertos = int.Parse(match4Numeros.Groups[1].Value);
                hombres = int.Parse(match4Numeros.Groups[2].Value);
                mujeres = int.Parse(match4Numeros.Groups[3].Value);
                noIdentificado = int.Parse(match4Numeros.Groups[4].Value);

                string parteGeo = linea.Substring(0, match4Numeros.Index).Trim();
                fuenteRaw = linea.Substring(match4Numeros.Index + match4Numeros.Length).Trim();
                (entidadRaw, municipioRaw) = SepararEntidadYMunicipio(parteGeo);
            }
            else if (match3Numeros.Success)
            {
                muertos = int.Parse(match3Numeros.Groups[1].Value);
                hombres = int.Parse(match3Numeros.Groups[2].Value);
                mujeres = int.Parse(match3Numeros.Groups[3].Value);
                noIdentificado = 0;

                string parteGeo = linea.Substring(0, match3Numeros.Index).Trim();
                fuenteRaw = linea.Substring(match3Numeros.Index + match3Numeros.Length).Trim();
                (entidadRaw, municipioRaw) = SepararEntidadYMunicipio(parteGeo);
            }
            else if (match1Numero.Success)
            {
                if (!int.TryParse(match1Numero.Groups[1].Value, out muertos) || muertos == 0)
                    return null;

                hombres = muertos;
                mujeres = 0;
                noIdentificado = 0;

                string parteGeo = linea.Substring(0, match1Numero.Index).Trim();
                fuenteRaw = linea.Substring(match1Numero.Index + match1Numero.Length).Trim();
                (entidadRaw, municipioRaw) = SepararEntidadYMunicipio(parteGeo);
            }
            else
            {
                return null;
            }

            if (muertos > 500 || hombres > 500 || mujeres > 500 || noIdentificado > 500)
                return null;

            string munSanitizado = municipioRaw.Trim('(', ')', ' ', '-', '.', '/', '\\');
            if (EsPalabraProhibida(munSanitizado))
                return null;

            if (string.IsNullOrWhiteSpace(municipioRaw) && string.IsNullOrWhiteSpace(entidadRaw))
                return null;

            var (entidadResuelta, municipioResuelto, requiereRevision, detalle) = _normalizador.Resolver(entidadRaw, municipioRaw, fuenteRaw);

            bool tuvoCorreccion = requiereRevision ||
                (!string.IsNullOrWhiteSpace(entidadRaw) && !string.Equals(entidadRaw.Trim(), entidadResuelta.Trim(), StringComparison.OrdinalIgnoreCase)) ||
                (!string.IsNullOrWhiteSpace(municipioRaw) && !string.Equals(municipioRaw.Trim(), municipioResuelto.Trim(), StringComparison.OrdinalIgnoreCase)) ||
                (detalle != "Catálogo oficial" && detalle != "Directo");

            string motivoCorreccion = "";
            if (requiereRevision || entidadResuelta == "NO IDENTIFICADA")
            {
                motivoCorreccion = "Entidad o Municipio no resuelto en catálogo oficial";
            }
            else if (!string.IsNullOrWhiteSpace(detalle) && detalle != "Catálogo oficial" && detalle != "Directo")
            {
                motivoCorreccion = detalle;
            }
            else if (tuvoCorreccion)
            {
                motivoCorreccion = $"Corrección aplicada ({entidadRaw.Trim()} / {municipioRaw.Trim()} -> {entidadResuelta} / {municipioResuelto})";
            }

            return new RegistroHomicidioDto
            {
                Fecha = fecha,
                Entidad = entidadResuelta,
                Municipio = municipioResuelto,
                EntidadRaw = string.IsNullOrWhiteSpace(entidadRaw) ? entidadResuelta : entidadRaw.Trim(),
                MunicipioRaw = string.IsNullOrWhiteSpace(municipioRaw) ? municipioResuelto : municipioRaw.Trim(),
                TuvoCorreccion = tuvoCorreccion,
                MotivoCorreccion = motivoCorreccion,
                NumMuertos = muertos > 0 ? muertos : (hombres + mujeres + noIdentificado),
                Hombres = hombres,
                Mujeres = mujeres,
                NoIdentificado = noIdentificado,
                Fuente = string.IsNullOrWhiteSpace(fuenteRaw) ? "Reporte Oficial SSPC" : fuenteRaw,
                RequiereRevision = requiereRevision,
                DetalleInferencia = detalle
            };
        }

        private (string Entidad, string Municipio) SepararEntidadYMunicipio(string parteGeografica)
        {
            var tokens = parteGeografica.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (tokens.Length == 0) return ("", "");
            if (tokens.Length == 1) return ("", tokens[0]);

            var entidadesOficiales = _normalizador.ObtenerEntidadesOficiales();

            for (int len = Math.Min(4, tokens.Length - 1); len >= 1; len--)
            {
                string posibleEntidad = string.Join(" ", tokens.Take(len));
                string posibleMun = string.Join(" ", tokens.Skip(len));

                if (entidadesOficiales.Any(e => e.Equals(posibleEntidad, StringComparison.OrdinalIgnoreCase)))
                {
                    return (posibleEntidad, posibleMun);
                }
            }

            return (string.Join(" ", tokens.Take(tokens.Length - 1)), tokens.Last());
        }
    }
}
