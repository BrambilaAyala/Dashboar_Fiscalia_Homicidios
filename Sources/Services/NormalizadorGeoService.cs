using System.Text;
using System.Text.RegularExpressions;
using ProyectoFisca.Sources.Interface;

namespace ProyectoFisca.Sources.Services
{
    public class NormalizadorGeoService : INormalizadorGeoService
    {
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<NormalizadorGeoService> _logger;

        private readonly Dictionary<string, string> _entidadesPorClave = new();
        private readonly Dictionary<string, string> _entidadesOficiales = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, string> _aliasEntidades = new(StringComparer.OrdinalIgnoreCase);
        
        // Mapeo: Nombre Municipio Normalizado -> Lista de (ClaveEntidad, NombreEntidad)
        private readonly Dictionary<string, List<(string ClaveEntidad, string NombreEntidad)>> _municipiosPorNombre = new(StringComparer.OrdinalIgnoreCase);
        
        // Mapeo: ClaveEntidad -> Lista de Nombres de Municipios de esa entidad
        private readonly Dictionary<string, HashSet<string>> _municipiosPorEntidad = new(StringComparer.OrdinalIgnoreCase);

        // Estado contextual de arrastre (Forward Fill)
        private string? _ultimaEntidadActiva = null;
        private bool _inicializado = false;
        private readonly object _lock = new();

        public NormalizadorGeoService(IWebHostEnvironment env, ILogger<NormalizadorGeoService> logger)
        {
            _env = env;
            _logger = logger;
            Inicializar();
        }

        public void ReiniciarContexto()
        {
            _ultimaEntidadActiva = null;
        }

        public List<string> ObtenerEntidadesOficiales()
        {
            return _entidadesOficiales.Values.Distinct().OrderBy(e => e).ToList();
        }

        public void Inicializar()
        {
            if (_inicializado) return;

            lock (_lock)
            {
                if (_inicializado) return;

                CargarAliasConocidos();
                CargarCatalogosCsv();
                _inicializado = true;
            }
        }

        private void CargarAliasConocidos()
        {
            _aliasEntidades["CDMX"] = "CIUDAD DE MÉXICO";
            _aliasEntidades["CIUDAD DE MEXICO"] = "CIUDAD DE MÉXICO";
            _aliasEntidades["DF"] = "CIUDAD DE MÉXICO";
            _aliasEntidades["D.F."] = "CIUDAD DE MÉXICO";
            _aliasEntidades["EDOMEX"] = "MÉXICO";
            _aliasEntidades["ESTADO DE MEXICO"] = "MÉXICO";
            _aliasEntidades["ESTADO DE MÉXICO"] = "MÉXICO";
            _aliasEntidades["MICHOACAN"] = "MICHOACÁN DE OCAMPO";
            _aliasEntidades["MICHOACÁN"] = "MICHOACÁN DE OCAMPO";
            _aliasEntidades["MICHOACAN DE OCAMPO"] = "MICHOACÁN DE OCAMPO";
            _aliasEntidades["DE OCAMPO"] = "MICHOACÁN DE OCAMPO";
            _aliasEntidades["MICHOACÁN DEOCAMPO"] = "MICHOACÁN DE OCAMPO";
            _aliasEntidades["VERACRUZ"] = "VERACRUZ DE IGNACIO DE LA LLAVE";
            _aliasEntidades["VERACRUZ DE"] = "VERACRUZ DE IGNACIO DE LA LLAVE";
            _aliasEntidades["IGNACIO DE LA LLAVE"] = "VERACRUZ DE IGNACIO DE LA LLAVE";
            _aliasEntidades["LLAVE"] = "VERACRUZ DE IGNACIO DE LA LLAVE";
            _aliasEntidades["VERACRUZ DE IGNACIO"] = "VERACRUZ DE IGNACIO DE LA LLAVE";
            _aliasEntidades["COAHUILA"] = "COAHUILA DE ZARAGOZA";
            _aliasEntidades["COAHUILA DE"] = "COAHUILA DE ZARAGOZA";
            _aliasEntidades["DE ZARAGOZA"] = "COAHUILA DE ZARAGOZA";
            _aliasEntidades["COAHUILA DEZARAGOZA"] = "COAHUILA DE ZARAGOZA";
            _aliasEntidades["NUEVO LEON"] = "NUEVO LEÓN";
            _aliasEntidades["NUEVOLEON"] = "NUEVO LEÓN";
            _aliasEntidades["NUEVOLEÓN"] = "NUEVO LEÓN";
            _aliasEntidades["SAN LUIS POTOSI"] = "SAN LUIS POTOSÍ";
            _aliasEntidades["SANLUIS POTOSÍ"] = "SAN LUIS POTOSÍ";
            _aliasEntidades["SANLUISPOTOSÍ"] = "SAN LUIS POTOSÍ";
            _aliasEntidades["QUINTANA ROO"] = "QUINTANA ROO";
            _aliasEntidades["QUINTANAROO"] = "QUINTANA ROO";
            _aliasEntidades["BAJA CALIFORNIA"] = "BAJA CALIFORNIA";
            _aliasEntidades["BAJACALIFORNIA"] = "BAJA CALIFORNIA";
            _aliasEntidades["BAJA CALIFORNIA SUR"] = "BAJA CALIFORNIA SUR";
            _aliasEntidades["BAJACALIFORNIASUR"] = "BAJA CALIFORNIA SUR";
            _aliasEntidades["BAJA CALIFORNIASUR"] = "BAJA CALIFORNIA SUR";
        }

        private void CargarCatalogosCsv()
        {
            try
            {
                string basePath = _env.ContentRootPath;
                string archivoEntidades = Path.Combine(basePath, "ENTIDAD_FEDERATIVA_201602.csv");
                string archivoMunicipios = Path.Combine(basePath, "MUNICIPIOS_202408.csv");

                if (!File.Exists(archivoEntidades) || !File.Exists(archivoMunicipios))
                {
                    _logger.LogWarning("Archivos CSV de catálogos no encontrados en {BasePath}", basePath);
                    return;
                }

                // 1. Cargar Entidades: CATALOG_KEY,ENTIDAD_FEDERATIVA,ABREVIATURA
                var lineasEnt = File.ReadAllLines(archivoEntidades, Encoding.UTF8);
                foreach (var linea in lineasEnt.Skip(1))
                {
                    if (string.IsNullOrWhiteSpace(linea)) continue;
                    var partes = linea.Split(',');
                    if (partes.Length >= 2)
                    {
                        string clave = partes[0].Trim();
                        string nombre = partes[1].Trim().ToUpperInvariant();
                        if (clave == "00") continue; // Excluir 'NO ESPECIFICADO'

                        _entidadesPorClave[clave] = nombre;
                        _entidadesOficiales[nombre] = nombre;
                        _entidadesOficiales[NormalizarSinAcentos(nombre)] = nombre;
                        
                        if (partes.Length >= 3 && !string.IsNullOrWhiteSpace(partes[2]))
                        {
                            string abrev = partes[2].Trim().ToUpperInvariant();
                            _aliasEntidades[abrev] = nombre;
                        }

                        _municipiosPorEntidad[clave] = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    }
                }

                // 2. Cargar Municipios: EFE_KEY,CATALOG_KEY,MUNICIPIO
                var lineasMun = File.ReadAllLines(archivoMunicipios, Encoding.UTF8);
                foreach (var linea in lineasMun.Skip(1))
                {
                    if (string.IsNullOrWhiteSpace(linea)) continue;
                    var partes = linea.Split(',');
                    if (partes.Length >= 3)
                    {
                        string claveEnt = partes[0].Trim();
                        string nombreMun = partes[2].Trim().ToUpperInvariant();
                        string nombreMunNormalizado = LimpiarTexto(nombreMun);

                        if (!_entidadesPorClave.TryGetValue(claveEnt, out string? nombreEntidad))
                        {
                            nombreEntidad = "DESCONOCIDO";
                        }

                        if (!_municipiosPorNombre.ContainsKey(nombreMunNormalizado))
                        {
                            _municipiosPorNombre[nombreMunNormalizado] = new List<(string, string)>();
                        }
                        _municipiosPorNombre[nombreMunNormalizado].Add((claveEnt, nombreEntidad));

                        if (_municipiosPorEntidad.ContainsKey(claveEnt))
                        {
                            _municipiosPorEntidad[claveEnt].Add(nombreMunNormalizado);
                        }
                    }
                }

                _logger.LogInformation("Catálogos cargados exitosamente: {Entidades} entidades y {Municipios} nombres de municipios",
                    _entidadesOficiales.Count, _municipiosPorNombre.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar catálogos de entidades y municipios.");
            }
        }

        public (string Entidad, string Municipio, bool RequiereRevision, string Detalle) Resolver(string entidadRaw, string municipioRaw, string fuenteRaw)
        {
            string entLimpia = LimpiarTexto(entidadRaw);
            string munLimpia = LimpiarTexto(municipioRaw);
            string fuenteLimpia = LimpiarTexto(fuenteRaw);

            string? entidadResuelta = null;
            string detalle = "Directo";

            // PASO 1: Comprobar alias directo
            if (!string.IsNullOrWhiteSpace(entLimpia) && _aliasEntidades.TryGetValue(entLimpia, out var aliasMatch))
            {
                entidadResuelta = aliasMatch;
                detalle = $"Alias ({entidadRaw.Trim()} -> {aliasMatch})";
            }
            // PASO 2: Comprobar entidad oficial exacta o sin acentos
            else if (!string.IsNullOrWhiteSpace(entLimpia) && _entidadesOficiales.TryGetValue(entLimpia, out var oficialMatch))
            {
                entidadResuelta = oficialMatch;
                detalle = "Catálogo oficial";
            }
            else if (!string.IsNullOrWhiteSpace(entLimpia) && _entidadesOficiales.TryGetValue(NormalizarSinAcentos(entLimpia), out var acentoMatch))
            {
                entidadResuelta = acentoMatch;
                detalle = $"Normalización acentos ({entidadRaw.Trim()} -> {acentoMatch})";
            }
            else if (!string.IsNullOrWhiteSpace(entLimpia))
            {
                // PASO 3: Fuzzy Matching con las 32 entidades oficiales
                var mejorFuzzy = _entidadesOficiales.Values.Distinct()
                    .Select(e => new { Entidad = e, Similitud = CalcularSimilitudJaroWinkler(entLimpia, e) })
                    .OrderByDescending(x => x.Similitud).FirstOrDefault();

                if (mejorFuzzy != null && mejorFuzzy.Similitud >= 0.82)
                {
                    entidadResuelta = mejorFuzzy.Entidad;
                    detalle = $"Fuzzy ({mejorFuzzy.Similitud:P0}) ({entidadRaw.Trim()} -> {mejorFuzzy.Entidad})";
                }
            }

            // PASO 4: Si no se identificó la entidad o viene vacía/truncada -> RESOLUCIÓN INVERSA POR MUNICIPIO
            if (string.IsNullOrWhiteSpace(entidadResuelta) && !string.IsNullOrWhiteSpace(munLimpia))
            {
                if (_municipiosPorNombre.TryGetValue(munLimpia, out var listaEstados))
                {
                    if (listaEstados.Count == 1)
                    {
                        // Municipio unívoco en todo México
                        entidadResuelta = listaEstados[0].NombreEntidad;
                        detalle = $"Inferido por Municipio Unívoco ({munLimpia})";
                    }
                    else if (listaEstados.Count > 1)
                    {
                        // Homónimo: Desambiguar por contexto de la última entidad leída
                        var matchContexto = listaEstados.FirstOrDefault(e => e.NombreEntidad.Equals(_ultimaEntidadActiva, StringComparison.OrdinalIgnoreCase));
                        if (matchContexto != default)
                        {
                            entidadResuelta = matchContexto.NombreEntidad;
                            detalle = $"Desambiguado por Contexto Previo ({entidadResuelta})";
                        }
                        else
                        {
                            // Desambiguar por mención en la fuente periodística
                            var matchFuente = listaEstados.FirstOrDefault(e => fuenteLimpia.Contains(NormalizarSinAcentos(e.NombreEntidad)));
                            if (matchFuente != default)
                            {
                                entidadResuelta = matchFuente.NombreEntidad;
                                detalle = $"Desambiguado por Fuente ({entidadResuelta})";
                            }
                            else
                            {
                                // Tomar contexto previo si existe
                                entidadResuelta = _ultimaEntidadActiva ?? listaEstados[0].NombreEntidad;
                                detalle = "Desambiguación heurística";
                            }
                        }
                    }
                }
            }

            // PASO 5: Si aún no se resuelve, aplicar Carry-forward de la última entidad
            if (string.IsNullOrWhiteSpace(entidadResuelta) && !string.IsNullOrWhiteSpace(_ultimaEntidadActiva))
            {
                entidadResuelta = _ultimaEntidadActiva;
                detalle = "Arrastre contextual (Forward-fill)";
            }

            if (string.IsNullOrWhiteSpace(entidadResuelta))
            {
                entidadResuelta = "NO IDENTIFICADA";
                detalle = "No resuelto";
            }
            else
            {
                _ultimaEntidadActiva = entidadResuelta;
            }

            // PASO 6: Normalización del Municipio dentro de la Entidad resuelta
            string municipioFinal = munLimpia;
            bool requiereRevision = entidadResuelta == "NO IDENTIFICADA" || string.IsNullOrWhiteSpace(municipioFinal);

            // Validar si el municipio pertenece a la entidad o si requiere ajuste
            string claveEntidad = _entidadesPorClave.FirstOrDefault(x => x.Value.Equals(entidadResuelta, StringComparison.OrdinalIgnoreCase)).Key;
            if (!string.IsNullOrEmpty(claveEntidad) && _municipiosPorEntidad.TryGetValue(claveEntidad, out var municipiosDeEstaEntidad))
            {
                if (!municipiosDeEstaEntidad.Contains(munLimpia) && !string.IsNullOrWhiteSpace(munLimpia))
                {
                    // Buscar coincidencia difusa del municipio dentro del estado
                    var mejorMunFuzzy = municipiosDeEstaEntidad
                        .Select(m => new { Municipio = m, Similitud = CalcularSimilitudJaroWinkler(munLimpia, m) })
                        .OrderByDescending(x => x.Similitud).FirstOrDefault();

                    if (mejorMunFuzzy != null && mejorMunFuzzy.Similitud >= 0.85)
                    {
                        municipioFinal = mejorMunFuzzy.Municipio;
                        detalle += $" | MunFuzzy ({mejorMunFuzzy.Similitud:P0}) ({municipioRaw.Trim()} -> {mejorMunFuzzy.Municipio})";
                    }
                }
            }

            return (entidadResuelta, municipioFinal, requiereRevision, detalle);
        }

        public static string LimpiarTexto(string? input)
        {
            if (string.IsNullOrWhiteSpace(input)) return string.Empty;
            string texto = Regex.Replace(input, @"\([^)]*\)", "");   // Quitar (1), (2), etc.
            texto = Regex.Replace(texto, @"[\r\n\t]+", " ");         // Unificar saltos de línea a espacio
            texto = Regex.Replace(texto, @"\s+", " ");               // Unificar espacios repetidos
            texto = texto.Trim(' ', '-', ',', ';', '.');
            return texto.ToUpperInvariant();
        }

        public static string NormalizarSinAcentos(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return string.Empty;
            string normalizada = input.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();
            foreach (char c in normalizada)
            {
                if (System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c) != System.Globalization.UnicodeCategory.NonSpacingMark)
                {
                    sb.Append(c);
                }
            }
            return sb.ToString().Normalize(NormalizationForm.FormC).ToUpperInvariant();
        }

        public static double CalcularSimilitudJaroWinkler(string s1, string s2)
        {
            if (string.IsNullOrEmpty(s1) || string.IsNullOrEmpty(s2)) return 0.0;
            if (s1.Equals(s2, StringComparison.OrdinalIgnoreCase)) return 1.0;

            int len1 = s1.Length, len2 = s2.Length;
            int matchDistance = Math.Max(len1, len2) / 2 - 1;
            bool[] s1Matches = new bool[len1], s2Matches = new bool[len2];
            int matches = 0, transpositions = 0;

            for (int i = 0; i < len1; i++)
            {
                int start = Math.Max(0, i - matchDistance);
                int end = Math.Min(i + matchDistance + 1, len2);
                for (int j = start; j < end; j++)
                {
                    if (s2Matches[j] || char.ToUpperInvariant(s1[i]) != char.ToUpperInvariant(s2[j])) continue;
                    s1Matches[i] = true;
                    s2Matches[j] = true;
                    matches++;
                    break;
                }
            }
            if (matches == 0) return 0.0;

            int k = 0;
            for (int i = 0; i < len1; i++)
            {
                if (!s1Matches[i]) continue;
                while (!s2Matches[k]) k++;
                if (char.ToUpperInvariant(s1[i]) != char.ToUpperInvariant(s2[k])) transpositions++;
                k++;
            }

            double m = matches;
            double jaro = (m / len1 + m / len2 + (m - transpositions / 2.0) / m) / 3.0;
            int prefix = 0;
            for (int i = 0; i < Math.Min(4, Math.Min(len1, len2)); i++)
            {
                if (char.ToUpperInvariant(s1[i]) == char.ToUpperInvariant(s2[i])) prefix++; else break;
            }
            return jaro + prefix * 0.1 * (1.0 - jaro);
        }
    }
}
