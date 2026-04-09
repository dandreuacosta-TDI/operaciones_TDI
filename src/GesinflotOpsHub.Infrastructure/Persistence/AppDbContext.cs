using GesinflotOpsHub.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace GesinflotOpsHub.Infrastructure.Persistence;

public class AppDbContext : IdentityDbContext<ApplicationUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<ExpedienteInstalacion> Expedientes => Set<ExpedienteInstalacion>();
    public DbSet<ChecklistTecnica> Checklists => Set<ChecklistTecnica>();
    public DbSet<UnidadChecklist> UnidadesChecklist => Set<UnidadChecklist>();
    public DbSet<UnidadVehiculo> UnidadesVehiculo => Set<UnidadVehiculo>();
    public DbSet<PlanificacionInstalacion> Planificaciones => Set<PlanificacionInstalacion>();
    public DbSet<Intervencion> Intervenciones => Set<Intervencion>();
    public DbSet<Instalador> Instaladores => Set<Instalador>();
    public DbSet<AuditoriaExpediente> AuditoriasExpediente => Set<AuditoriaExpediente>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // ── Identity table names ───────────────────────────────────────────
        builder.Entity<ApplicationUser>().ToTable("usuarios");
        builder.Entity<IdentityRole>().ToTable("roles");
        builder.Entity<IdentityUserRole<string>>().ToTable("usuario_roles");
        builder.Entity<IdentityUserClaim<string>>().ToTable("usuario_claims");
        builder.Entity<IdentityUserLogin<string>>().ToTable("usuario_logins");
        builder.Entity<IdentityRoleClaim<string>>().ToTable("rol_claims");
        builder.Entity<IdentityUserToken<string>>().ToTable("usuario_tokens");

        // ── Cliente ───────────────────────────────────────────────────────
        builder.Entity<Cliente>(e =>
        {
            e.ToTable("clientes");
            e.HasKey(x => x.Id);
            e.Property(x => x.RazonSocial).HasMaxLength(200).IsRequired();
            e.Property(x => x.CIF).HasMaxLength(20);
            e.Property(x => x.Email).HasMaxLength(200);
            e.HasIndex(x => x.OdooPartnerId);
        });

        // ── ExpedienteInstalacion ─────────────────────────────────────────
        builder.Entity<ExpedienteInstalacion>(e =>
        {
            e.ToTable("expedientes");
            e.HasKey(x => x.Id);
            e.Property(x => x.CodigoExpediente).HasMaxLength(50).IsRequired();
            e.HasIndex(x => x.CodigoExpediente).IsUnique();
            e.HasIndex(x => x.TokenPortalCliente);
            e.Property(x => x.ImportePresupuestado).HasPrecision(12, 2);
            e.Property(x => x.ImporteFacturado).HasPrecision(12, 2);
            e.Property(x => x.CosteOperativo).HasPrecision(12, 2);

            e.HasOne(x => x.Cliente)
             .WithMany(c => c.Expedientes)
             .HasForeignKey(x => x.ClienteId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // ── ChecklistTecnica ──────────────────────────────────────────────
        builder.Entity<ChecklistTecnica>(e =>
        {
            e.ToTable("checklists");
            e.HasKey(x => x.Id);
            e.HasOne(x => x.Expediente)
             .WithMany(exp => exp.Checklists)
             .HasForeignKey(x => x.ExpedienteId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<UnidadChecklist>(e =>
        {
            e.ToTable("unidades_checklist");
            e.HasKey(x => x.Id);
            e.HasOne(x => x.Checklist)
             .WithMany(c => c.Unidades)
             .HasForeignKey(x => x.ChecklistId)
             .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.UnidadVehiculo)
             .WithMany(u => u.HistorialChecklist)
             .HasForeignKey(x => x.UnidadVehiculoId)
             .OnDelete(DeleteBehavior.SetNull);
        });

        // ── UnidadVehiculo ────────────────────────────────────────────────
        builder.Entity<UnidadVehiculo>(e =>
        {
            e.ToTable("unidades_vehiculo");
            e.HasKey(x => x.Id);
            e.Property(x => x.Matricula).HasMaxLength(20);
            e.HasOne(x => x.Cliente)
             .WithMany(c => c.UnidadesFlota)
             .HasForeignKey(x => x.ClienteId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // ── Planificación ─────────────────────────────────────────────────
        builder.Entity<PlanificacionInstalacion>(e =>
        {
            e.ToTable("planificaciones");
            e.HasKey(x => x.Id);
            e.HasOne(x => x.Expediente)
             .WithMany(exp => exp.Planificaciones)
             .HasForeignKey(x => x.ExpedienteId)
             .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Instalador)
             .WithMany(i => i.Planificaciones)
             .HasForeignKey(x => x.InstaladorId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // ── Intervención ──────────────────────────────────────────────────
        builder.Entity<Intervencion>(e =>
        {
            e.ToTable("intervenciones");
            e.HasKey(x => x.Id);
            e.Property(x => x.Codigo).HasMaxLength(50).IsRequired();
            e.HasIndex(x => x.Codigo).IsUnique();
            e.Property(x => x.Importe).HasPrecision(12, 2);
            e.Property(x => x.Coste).HasPrecision(12, 2);
            e.Ignore(x => x.Margen);
            e.Ignore(x => x.MargenPorcentaje);

            e.HasOne(x => x.Expediente)
             .WithMany(exp => exp.Intervenciones)
             .HasForeignKey(x => x.ExpedienteId)
             .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Cliente)
             .WithMany()
             .HasForeignKey(x => x.ClienteId)
             .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Instalador)
             .WithMany(i => i.Intervenciones)
             .HasForeignKey(x => x.InstaladorId)
             .OnDelete(DeleteBehavior.SetNull);
        });

        // ── Instalador ────────────────────────────────────────────────────
        builder.Entity<Instalador>(e =>
        {
            e.ToTable("instaladores");
            e.HasKey(x => x.Id);
            e.Property(x => x.Nombre).HasMaxLength(100).IsRequired();
            e.Ignore(x => x.NombreCompleto);
        });

        // ── Auditoría ─────────────────────────────────────────────────────
        builder.Entity<AuditoriaExpediente>(e =>
        {
            e.ToTable("auditoria_expedientes");
            e.HasKey(x => x.Id);
            e.HasOne(x => x.Expediente)
             .WithMany(exp => exp.Auditorias)
             .HasForeignKey(x => x.ExpedienteId)
             .OnDelete(DeleteBehavior.Cascade);
        });
    }
}

public class ApplicationUser : IdentityUser
{
    public string? NombreCompleto { get; set; }
    public string? Rol { get; set; }
    public DateTime? FechaUltimoAcceso { get; set; }
    public bool Activo { get; set; } = true;
    public Guid? InstaladorId { get; set; }
    public string? ComercialOdooId { get; set; }
}
