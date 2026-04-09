using GesinflotOpsHub.Domain.Common;
using GesinflotOpsHub.Domain.Enums;

namespace GesinflotOpsHub.Domain.Entities;

public class UnidadVehiculo : AuditableEntity
{
    public Guid ClienteId { get; set; }
    public Cliente Cliente { get; set; } = null!;

    public TipoUnidad TipoUnidad { get; set; }
    public string? Marca { get; set; }
    public string? Modelo { get; set; }
    public string Matricula { get; set; } = string.Empty;
    public string? Bastidor { get; set; }

    // Equipo instalado actualmente
    public string? NumeroSerieEquipoActual { get; set; }
    public string? SimCardActual { get; set; }
    public DateTime? FechaUltimaIntervencion { get; set; }

    public bool Activo { get; set; } = true;
    public string? Notas { get; set; }

    public ICollection<UnidadChecklist> HistorialChecklist { get; set; } = new List<UnidadChecklist>();
}
