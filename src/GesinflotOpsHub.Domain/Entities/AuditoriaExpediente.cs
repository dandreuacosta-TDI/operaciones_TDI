using GesinflotOpsHub.Domain.Common;

namespace GesinflotOpsHub.Domain.Entities;

/// <summary>
/// Registro de auditoría para trazabilidad completa de cada expediente.
/// </summary>
public class AuditoriaExpediente : BaseEntity
{
    public Guid ExpedienteId { get; set; }
    public ExpedienteInstalacion Expediente { get; set; } = null!;

    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string Usuario { get; set; } = string.Empty;
    public string Accion { get; set; } = string.Empty;
    public string? EstadoAnterior { get; set; }
    public string? EstadoNuevo { get; set; }
    public string? Detalle { get; set; }
    public string? IP { get; set; }
}
