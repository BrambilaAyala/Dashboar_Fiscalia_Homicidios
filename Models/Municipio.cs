using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProyectoFisca.Models
{
    [Table("Municipio")]
    public class Municipio
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("nombre")]
        public string? Nombre { get; set; }

        [Column("entidad_id")]
        public int Entidad_Id { get; set; }

        [ForeignKey("Entidad_Id")]
        public Entidad? Entidad { get; set; }
    }
}
