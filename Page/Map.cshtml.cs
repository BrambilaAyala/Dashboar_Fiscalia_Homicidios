using System.Text.Json;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProyectoFisca.Sources.Interface;

namespace ProyectoFisca.Pages
{
    public class MapModel : PageModel
    {
        private readonly IHomicidiosDataAccess _dataAccess;

        public MapModel(IHomicidiosDataAccess dataAccess)
        {
            _dataAccess = dataAccess;
        }

        public static readonly Dictionary<string, long> PoblacionInegi = new(StringComparer.OrdinalIgnoreCase)
        {
            { "AGUASCALIENTES", 1425607 },
            { "BAJA CALIFORNIA", 3769020 },
            { "BAJA CALIFORNIA SUR", 798447 },
            { "CAMPECHE", 928363 },
            { "COAHUILA DE ZARAGOZA", 3146771 },
            { "COLIMA", 731391 },
            { "CHIAPAS", 5543828 },
            { "CHIHUAHUA", 3741869 },
            { "CIUDAD DE MÉXICO", 9209944 },
            { "DURANGO", 1832650 },
            { "GUANAJUATO", 6166934 },
            { "GUERRERO", 3540685 },
            { "HIDALGO", 3082841 },
            { "JALISCO", 8348151 },
            { "MÉXICO", 16992418 },
            { "MICHOACÁN DE OCAMPO", 4748846 },
            { "MORELOS", 1971520 },
            { "NAYARIT", 1235456 },
            { "NUEVO LEÓN", 5784442 },
            { "OAXACA", 4132148 },
            { "PUEBLA", 6583278 },
            { "QUERÉTARO", 2368467 },
            { "QUINTANA ROO", 1857985 },
            { "SAN LUIS POTOSÍ", 2822255 },
            { "SINALOA", 3026943 },
            { "SONORA", 2944840 },
            { "TABASCO", 2402598 },
            { "TAMAULIPAS", 3527735 },
            { "TLAXCALA", 1342977 },
            { "VERACRUZ DE IGNACIO DE LA LLAVE", 8062579 },
            { "YUCATÁN", 2320898 },
            { "ZACATECAS", 1622138 }
        };

        public List<EntidadGeoDto> ListaEntidades { get; set; } = new();
        public List<MunicipioRankingDto> TopMunicipios { get; set; } = new();
        public int GranTotalMuertos { get; set; }
        public long GranTotalPoblacion { get; set; }
        public string EntidadesJson { get; set; } = "[]";

        public class EntidadGeoDto
        {
            public string Entidad { get; set; } = string.Empty;
            public int Num_Muertos { get; set; }
            public int Hombres { get; set; }
            public int Mujeres { get; set; }
            public int No_Identificado { get; set; }
            public long Poblacion { get; set; }
            public double TasaPor100k { get; set; }
            public double PctNacional { get; set; }
        }

        public class MunicipioRankingDto
        {
            public string Municipio { get; set; } = string.Empty;
            public string Entidad { get; set; } = string.Empty;
            public int Num_Muertos { get; set; }
            public int Hombres { get; set; }
            public int Mujeres { get; set; }
            public int No_Identificado { get; set; }
        }

        public async Task OnGetAsync()
        {
            var totalesPorEntidad = await _dataAccess.ObtenerTotalesPorEntidadAsync();
            var todosHomicidios = await _dataAccess.ObtenerHomicidiosAsync();

            GranTotalMuertos = totalesPorEntidad.Sum(e => e.Num_Muertos);
            GranTotalPoblacion = PoblacionInegi.Values.Sum();

            ListaEntidades = totalesPorEntidad.Select(e =>
            {
                PoblacionInegi.TryGetValue(e.Entidad, out long pop);
                if (pop == 0) pop = 1500000; // Valor fallback representativo

                double tasa = pop > 0 ? Math.Round(((double)e.Num_Muertos / pop) * 100000.0, 2) : 0;
                double pctNac = GranTotalMuertos > 0 ? Math.Round(((double)e.Num_Muertos / GranTotalMuertos) * 100.0, 1) : 0;

                return new EntidadGeoDto
                {
                    Entidad = e.Entidad,
                    Num_Muertos = e.Num_Muertos,
                    Hombres = e.Hombres,
                    Mujeres = e.Mujeres,
                    No_Identificado = e.No_Identificado,
                    Poblacion = pop,
                    TasaPor100k = tasa,
                    PctNacional = pctNac
                };
            }).OrderByDescending(x => x.Num_Muertos).ToList();

            TopMunicipios = todosHomicidios
                .GroupBy(h => new { h.Entidad, h.Municipio })
                .Select(g => new MunicipioRankingDto
                {
                    Entidad = g.Key.Entidad,
                    Municipio = g.Key.Municipio,
                    Num_Muertos = g.Sum(x => x.Num_Muertos),
                    Hombres = g.Sum(x => x.Hombres),
                    Mujeres = g.Sum(x => x.Mujeres),
                    No_Identificado = g.Sum(x => x.No_Identificado)
                })
                .OrderByDescending(m => m.Num_Muertos).Take(15).ToList();

            EntidadesJson = JsonSerializer.Serialize(ListaEntidades);
        }
    }
}
