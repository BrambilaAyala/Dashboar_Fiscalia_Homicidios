using System.Text.Json;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProyectoFisca.Models;
using ProyectoFisca.Sources.Interface;

namespace ProyectoFisca.Pages
{
    public class GraphsModel : PageModel
    {
        private readonly IHomicidiosDataAccess _dataAccess;

        public GraphsModel(IHomicidiosDataAccess dataAccess)
        {
            _dataAccess = dataAccess;
        }

        public List<HomicidioTrimestreDto> Trimestres { get; set; } = new();
        public List<TotalesEntidadDto> TotalesPorEntidad { get; set; } = new();

        public int GranTotalMuertos { get; set; }
        public int GranTotalHombres { get; set; }
        public int GranTotalMujeres { get; set; }
        public int GranTotalNoIdentificado { get; set; }
        public double PorcentajeHombres { get; set; }
        public double PorcentajeMujeres { get; set; }
        public double PorcentajeNoIdentificado { get; set; }

        public string TrimestresJson { get; set; } = "[]";
        public string EntidadesJson { get; set; } = "[]";

        public async Task OnGetAsync()
        {
            Trimestres = await _dataAccess.ObtenerHomicidiosPorTrimestreAsync();
            TotalesPorEntidad = await _dataAccess.ObtenerTotalesPorEntidadAsync();

            GranTotalMuertos = Trimestres.Sum(t => t.Num_Muertos);
            GranTotalHombres = Trimestres.Sum(t => t.Hombres);
            GranTotalMujeres = Trimestres.Sum(t => t.Mujeres);
            GranTotalNoIdentificado = Trimestres.Sum(t => t.No_Identificado);

            int totalGenero = GranTotalHombres + GranTotalMujeres + GranTotalNoIdentificado;
            if (totalGenero > 0)
            {
                PorcentajeHombres = Math.Round((double)GranTotalHombres / totalGenero * 100, 1);
                PorcentajeMujeres = Math.Round((double)GranTotalMujeres / totalGenero * 100, 1);
                PorcentajeNoIdentificado = Math.Round((double)GranTotalNoIdentificado / totalGenero * 100, 1);
            }

            TrimestresJson = JsonSerializer.Serialize(Trimestres);
            EntidadesJson = JsonSerializer.Serialize(TotalesPorEntidad.Take(10));
        }
    }
}
