using HelpDesk.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;

namespace HelpDesk.Infraestructura.Data;

public class HelpDeskDbContext : DbContext
{
    public HelpDeskDbContext(DbContextOptions<HelpDeskDbContext> options)
        : base(options)
    {
    }

    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Categoria> Categorias => Set<Categoria>();
    public DbSet<Solicitud> Solicitudes => Set<Solicitud>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasIndex(u => u.Email).IsUnique();
            entity.HasIndex(u => u.TenantId);

            entity.Property(u => u.Rol).HasConversion<string>();

            entity.HasOne<Tenant>()
                .WithMany()
                .HasForeignKey(u => u.TenantId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Categoria>(entity =>
        {
            entity.HasIndex(c => c.TenantId);

            entity.HasOne<Tenant>()
                .WithMany()
                .HasForeignKey(c => c.TenantId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Solicitud>(entity =>
        {
            entity.HasIndex(s => s.TenantId);

            entity.Property(s => s.Titulo).HasMaxLength(120);
            entity.Property(s => s.Descripcion).HasMaxLength(4000);

            entity.Property(s => s.Estado).HasConversion<string>();
            entity.Property(s => s.Prioridad).HasConversion<string>();

            entity.Property(s => s.FechaCreacion).HasConversion(UtcDateTimeConverter.Instance);
            entity.Property(s => s.FechaLimiteSla).HasConversion(UtcDateTimeConverter.Instance);
            entity.Property(s => s.FechaResolucion).HasConversion(UtcNullableDateTimeConverter.Instance);

            entity.HasOne<Tenant>()
                .WithMany()
                .HasForeignKey(s => s.TenantId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(s => s.Categoria)
                .WithMany()
                .HasForeignKey(s => s.CategoriaId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(s => s.Solicitante)
                .WithMany()
                .HasForeignKey(s => s.SolicitanteId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(s => s.Agente)
                .WithMany()
                .HasForeignKey(s => s.AgenteId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
