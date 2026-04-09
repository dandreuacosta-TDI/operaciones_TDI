using GesinflotOpsHub.Domain.Common;
using GesinflotOpsHub.Domain.Enums;

namespace GesinflotOpsHub.Domain.Entities;

public class Instalador : AuditableEntity
{
    public string Nombre { get; set; } = string.Empty;
    public string? Apellidos { get; set; }
    public string NombreCompleto => $"{Nombre} {Apellidos}".Trim();

    public TipoInstalador Tipo { get; set; }

    // Empresa (para partners)
    public string? Empresa { get; set; }
    public string? CIF { get; set; }

    // Contacto
    public string? Email { get; set; }
    public string? Telefono { get; set; }
    public string? TelefonoMovil { get; set; }

    // Zona de trabajo
    public string? ProvinciasCobertura { get; set; }
    public string? Zona { get; set; }

    // Control
    public bool Activo { get; set; } = true;
    public string? Notas { get; set; }

    // Vínculo con usuario del sistema (si existe)
    public string? ApplicationUserId { get; set; }

    public ICollection<PlanificacionInstalacion> Planificaciones { get; set; } = new List<PlanificacionInstalacion>();
    public ICollection<Intervencion> Intervenciones { get; set; } = new List<Intervencion>();
}
