using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoFisca;
using ProyectoFisca.Models;

namespace ProyectoFisca.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
        {
        }

        public DbSet<Entidad>? Entidad { get; set; }
        public DbSet<Municipio>? Municipio { get; set; }
        public DbSet<Homicidios3>? Homicidios3 { get; set; }  
    }
}