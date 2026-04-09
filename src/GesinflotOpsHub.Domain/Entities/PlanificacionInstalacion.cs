using GesinflotOpsHub.Domain.Common;
using GesinflotOpsHub.Domain.Enums;

namespace GesinflotOpsHub.Domain.Entities;

public class PlanificacionInstalacion : AuditableEntity
{
    public Guid ExpedienteId { get; set; }
    public ExpedienteInstalacion Expediente { get; set; } = null!;

    // Instalador asignado
    public Guid InstaladorId { get; set; }
    public Instalador Instalador { get; set; } = null!;

    // Fecha y hora
    public DateTime FechaPlanificada { get; set; }
    public TimeSpan? HoraPlanificada { get; set; }
    public int? DuracionEstimadaHoras { get; set; }

    // Confirmación con cliente
    public bool ConfirmadoConCliente { get; set; } = false;
    public DateTime? FechaConfirmacion { get; set; }
    public string? ConfirmadoPor { get; set; }

    // Lugar de instalación
    public string? DireccionInstalacion { get; set; }
    public string? Poblacion { get; set; }
    public string? Provincia { get; set; }
    public string? CodigoPostal { get; set; }

    // Contacto en cliente
    public string? ContactoCliente { get; set; }
    public string? TelefonoContacto { get; set; }
    public string? EmailContacto { get; set; }

    // Estado
    public bool Activa { get; set; } = true;
    public string? MotivoReprogramacion { get; set; }
    public string? Notas { get; set; }
}
