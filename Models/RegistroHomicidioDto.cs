namespace ProyectoFisca.Models
{
    public class RegistroHomicidioDto
    {
        public DateTime Fecha { get; set; }
        public string Entidad { get; set; } = string.Empty;
        public string Municipio { get; set; } = string.Empty;
        public int NumMuertos { get; set; }
        public int Hombres { get; set; }
        public int Mujeres { get; set; }
        public int NoIdentificado { get; set; }
        public string Fuente { get; set; } = string.Empty;
        public bool RequiereRevision { get; set; }
        public string? DetalleInferencia { get; set; }
        public string? EntidadRaw { get; set; }
        public string? MunicipioRaw { get; set; }
        public bool TuvoCorreccion { get; set; }
        public string? MotivoCorreccion { get; set; }
    }

    public class EtlResultDto
    {
        public bool Exitoso { get; set; }
        public string Mensaje { get; set; } = string.Empty;
        public int ArchivosDescargados { get; set; }
        public int ArchivosProcesados { get; set; }
        public int TotalRegistrosExtraidos { get; set; }
        public int TotalRegistrosGuardados { get; set; }
        public int TotalInconsistencias { get; set; }
        public List<string> Detalles { get; set; } = new();
    }

    public class RangoFechasRequest
    {
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
    }
}
