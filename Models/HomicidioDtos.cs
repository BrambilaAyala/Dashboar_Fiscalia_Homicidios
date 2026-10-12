namespace ProyectoFisca.Models
{
    public class HomicidioDetalleDto
    {
        public int Id { get; set; }
        public string Municipio { get; set; } = string.Empty;
        public string Entidad { get; set; } = string.Empty;
        public int Num_Muertos { get; set; }
        public int Hombres { get; set; }
        public int Mujeres { get; set; }
        public int No_Identificado { get; set; }
        public string Fuente { get; set; } = string.Empty;
        public string Fecha { get; set; } = string.Empty;
    }

    public class HomicidioTrimestreDto
    {
        public string Trimestre { get; set; } = string.Empty;
        public int Num_Muertos { get; set; }
        public int Hombres { get; set; }
        public int Mujeres { get; set; }
        public int No_Identificado { get; set; }
        public string Fecha { get; set; } = string.Empty;
    }

    public class TotalesEntidadDto
    {
        public string Entidad { get; set; } = string.Empty;
        public int Num_Muertos { get; set; }
        public int Hombres { get; set; }
        public int Mujeres { get; set; }
        public int No_Identificado { get; set; }
    }
}
