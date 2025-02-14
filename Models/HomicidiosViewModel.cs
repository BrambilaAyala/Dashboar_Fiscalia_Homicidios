using Microsoft.EntityFrameworkCore;

namespace ProyectoFisca.Models
{
    public class HomicidiosViewModel
    {
        public string? Entidad { get; set; }
        public string? Municipio { get; set; }
        public int? Num_Muertos { get; set; }
    }
}

