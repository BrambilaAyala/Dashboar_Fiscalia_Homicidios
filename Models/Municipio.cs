using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProyectoFisca.Models
{
    public class Municipio
    {
        public int Id { get; set; } // Clave primaria
        public string? Nombre { get; set; }
        public int? Entidad_Id { get; set; }
        public required Entidad Entidad { get; set; }
        
        }
}
