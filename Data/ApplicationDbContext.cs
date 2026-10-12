using Microsoft.EntityFrameworkCore;
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
        public DbSet<Homicidios>? Homicidios { get; set; }
        public DbSet<ReporteInconsistencia>? ReportesInconsistencias { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Entidad>(entity =>
            {
                entity.ToTable("Entidad");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.Nombre).HasColumnName("nombre");
            });

            modelBuilder.Entity<Municipio>(entity =>
            {
                entity.ToTable("Municipio");
                entity.HasKey(m => m.Id);
                entity.Property(m => m.Id).HasColumnName("id");
                entity.Property(m => m.Nombre).HasColumnName("nombre");
                entity.Property(m => m.Entidad_Id).HasColumnName("entidad_id");

                entity.HasOne(m => m.Entidad).WithMany().HasForeignKey(m => m.Entidad_Id).OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Homicidios>(entity =>
            {
                entity.ToTable("Homicidios");
                entity.HasKey(h => h.Id);
                entity.Property(h => h.Id).HasColumnName("id");
                entity.Property(h => h.Municipio_Id).HasColumnName("municipio_id");
                entity.Property(h => h.Fecha).HasColumnName("fecha");
                entity.Property(h => h.Num_Muertos).HasColumnName("num_muertos");
                entity.Property(h => h.Hombres).HasColumnName("hombres");
                entity.Property(h => h.Mujeres).HasColumnName("mujeres");
                entity.Property(h => h.No_Identificado).HasColumnName("no_identificado");
                entity.Property(h => h.Fuente).HasColumnName("fuente");

                entity.HasOne(h => h.Municipio).WithMany().HasForeignKey(h => h.Municipio_Id).OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<ReporteInconsistencia>(entity =>
            {
                entity.ToTable("reportesinconsistencias");
                entity.HasKey(r => r.Id);
                entity.Property(r => r.Id).HasColumnName("id");
                entity.Property(r => r.Fecha).HasColumnName("fecha");
                entity.Property(r => r.EntidadDetectada).HasColumnName("entidad_detectada");
                entity.Property(r => r.MunicipioDetectado).HasColumnName("municipio_detectado");
                entity.Property(r => r.NumMuertos).HasColumnName("num_muertos");
                entity.Property(r => r.Fuente).HasColumnName("fuente");
                entity.Property(r => r.MotivoInconsistencia).HasColumnName("motivo_inconsistencia");
                entity.Property(r => r.FechaRegistro).HasColumnName("fecha_registro");
            });
        }
    }
}