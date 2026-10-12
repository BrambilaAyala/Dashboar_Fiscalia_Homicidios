using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProyectoFisca.Models
{
    [Table("reportesinconsistencias")]
    public class ReporteInconsistencia
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("fecha")]
        public DateTime Fecha { get; set; }

        [Column("entidad_detectada")]
        public string? EntidadDetectada { get; set; }

        [Column("municipio_detectado")]
        public string? MunicipioDetectado { get; set; }

        [Column("num_muertos")]
        public int NumMuertos { get; set; }

        [Column("fuente")]
        public string? Fuente { get; set; }

        [Column("motivo_inconsistencia")]
        public string? MotivoInconsistencia { get; set; }

        [Column("fecha_registro")]
        public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;
    }
}
