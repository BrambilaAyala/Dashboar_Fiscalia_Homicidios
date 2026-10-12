using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProyectoFisca.Models
{
    [Table("Homicidios")]
    public class Homicidios
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("municipio_id")]
        public int Municipio_Id { get; set; }

        [Column("fecha")]
        public DateTime Fecha { get; set; }

        [Column("num_muertos")]
        public int Num_Muertos { get; set; }

        [Column("hombres")]
        public int Hombres { get; set; }

        [Column("mujeres")]
        public int Mujeres { get; set; }

        [Column("no_identificado")]
        public int No_Identificado { get; set; }

        [Column("fuente")]
        public string? Fuente { get; set; }

        [ForeignKey("Municipio_Id")]
        public Municipio? Municipio { get; set; }
    }
}
