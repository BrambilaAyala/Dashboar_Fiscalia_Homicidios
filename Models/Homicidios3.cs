using Microsoft.EntityFrameworkCore;

namespace ProyectoFisca.Models
{
    public class Homicidios
    {
        public int Id { get; set; }  // Clave primaria
        public int Municipio_Id { get; set; }
        public DateTime Fecha { get; set; }
        public int Num_Muertos { get; set; }
        public int Hombres { get; set; }
        public int Mujeres { get; set; }
        public int No_Identificado { get; set; }
        public string? Fuente { get; set; }

        public required Municipio  Municipio { get; set; } // Clave foránea

    }
    
}
