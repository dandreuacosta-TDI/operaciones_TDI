namespace GesinflotOpsHub.Domain.Common;

public abstract class BaseEntity
{
    public Guid Id { get; protected set; } = Guid.NewGuid();
}

public abstract class AuditableEntity : BaseEntity
{
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    public DateTime FechaUltimaActualizacion { get; set; } = DateTime.UtcNow;
    public string? CreadoPor { get; set; }
    public string? ModificadoPor { get; set; }
}
