namespace ProyectoFisca.Models
{
    public class AuditoriaResumenDto
    {
        public int TotalArchivosAnalizados { get; set; }
        public int TotalArchivosValidosTexto { get; set; }
        public int TotalArchivosOmitidosImagen { get; set; }
        public int TotalArchivosNoDisponibles { get; set; }
        public double PorcentajeArchivosValidos { get; set; }

        public int TotalRegistrosAnalizados { get; set; }
        public int TotalInconsistenciasDetectadas { get; set; }
        public double IndiceCalidadDatos { get; set; }

        // Gráfica 1: Estado de archivos
        public List<EtiquetaValorDto> EstadoArchivos { get; set; } = new();

        // Gráfica 2: Desglose de motivos de inconsistencia
        public List<EtiquetaValorDto> MotivosInconsistencias { get; set; } = new();

        // Gráfica 3: Entidades con mayor número de imprecisiones
        public List<EtiquetaValorDto> InconsistenciasPorEntidad { get; set; } = new();

        // Gráfica 4: Serie anual de inconsistencias
        public List<EtiquetaValorDto> InconsistenciasPorAnio { get; set; } = new();

        // Tabla detallada
        public List<ReporteInconsistencia> InconsistenciasDetalle { get; set; } = new();
    }

    public class EtiquetaValorDto
    {
        public string Etiqueta { get; set; } = string.Empty;
        public int Valor { get; set; }
        public string? Color { get; set; }
        public string? Descripcion { get; set; }
    }
}
