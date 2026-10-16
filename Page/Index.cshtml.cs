using Microsoft.AspNetCore.Mvc.RazorPages;
using ProyectoFisca.Models;
using ProyectoFisca.Sources.Interface;

namespace ProyectoFisca.Pages
{
    public class IndexModel : PageModel
    {
        private readonly IHomicidiosDataAccess _dataAccess;

        public IndexModel(IHomicidiosDataAccess dataAccess)
        {
            _dataAccess = dataAccess;
        }

        public int TotalMuertos { get; set; }
        public int TotalHombres { get; set; }
        public int TotalMujeres { get; set; }
        public int TotalNoIdentificado { get; set; }
        public double PorcentajeHombres { get; set; }
        public double PorcentajeMujeres { get; set; }
        public double PorcentajeNoIdentificado { get; set; }
        public int TotalEntidades { get; set; }
        public int TotalMunicipios { get; set; }
        public List<HomicidioDetalleDto> UltimosHomicidios { get; set; } = new();
        public List<TotalesEntidadDto> TopEntidades { get; set; } = new();

        public async Task OnGetAsync()
        {
            var homicidios = await _dataAccess.ObtenerHomicidiosAsync();
            var totalesEntidades = await _dataAccess.ObtenerTotalesPorEntidadAsync();

            TotalMuertos = homicidios.Sum(h => h.Num_Muertos);
            TotalHombres = homicidios.Sum(h => h.Hombres);
            TotalMujeres = homicidios.Sum(h => h.Mujeres);
            TotalNoIdentificado = homicidios.Sum(h => h.No_Identificado);

            int totalGenero = TotalHombres + TotalMujeres + TotalNoIdentificado;
            if (totalGenero > 0)
            {
                PorcentajeHombres = Math.Round((double)TotalHombres / totalGenero * 100, 1);
                PorcentajeMujeres = Math.Round((double)TotalMujeres / totalGenero * 100, 1);
                PorcentajeNoIdentificado = Math.Round((double)TotalNoIdentificado / totalGenero * 100, 1);
            }

            TotalEntidades = homicidios.Select(h => h.Entidad).Where(e => !string.IsNullOrEmpty(e)).Distinct().Count();
            TotalMunicipios = homicidios.Select(h => h.Municipio).Where(m => !string.IsNullOrEmpty(m)).Distinct().Count();

            UltimosHomicidios = homicidios.Take(15).ToList();
            TopEntidades = totalesEntidades.Take(5).ToList();
        }
    }
}
