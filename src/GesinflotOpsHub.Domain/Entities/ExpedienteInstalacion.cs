using GesinflotOpsHub.Domain.Common;
using GesinflotOpsHub.Domain.Enums;

namespace GesinflotOpsHub.Domain.Entities;

/// <summary>
/// Entidad central del sistema. Representa cada operación desde presupuesto hasta factura.
/// </summary>
public class ExpedienteInstalacion : AuditableEntity
{
    public string CodigoExpediente { get; set; } = string.Empty;

    // Vínculos con Odoo
    public string? OdooCustomerId { get; set; }
    public string? OdooBudgetId { get; set; }
    public string? OdooSaleOrderId { get; set; }
    public string? NumeroPresupuesto { get; set; }
    public string? NumeroPedidoVenta { get; set; }

    // Cliente
    public Guid ClienteId { get; set; }
    public Cliente Cliente { get; set; } = null!;
    public bool ClienteExistente { get; set; }

    // Clasificación
    public TipoExpediente TipoExpediente { get; set; }
    public EstadoExpediente Estado { get; set; } = EstadoExpediente.Borrador;

    // Comercial
    public string? ComercialResponsable { get; set; }
    public string? OdooComercialId { get; set; }

    // Control temporal
    public DateTime? FechaPresupuesto { get; set; }
    public DateTime? FechaAceptacion { get; set; }
    public DateTime? FechaInstalacionPrevista { get; set; }
    public DateTime? FechaInstalacionReal { get; set; }
    public DateTime? FechaFacturacion { get; set; }

    // Datos económicos
    public decimal? ImportePresupuestado { get; set; }
    public decimal? ImporteFacturado { get; set; }
    public decimal? CosteOperativo { get; set; }
    public bool EsFacturable { get; set; } = true;

    // Descripción
    public string? Observaciones { get; set; }
    public string? NotasInternas { get; set; }

    // Token para portal cliente (acceso sin login)
    public string? TokenPortalCliente { get; set; }
    public DateTime? TokenExpiracion { get; set; }

    // Navegación
    public ICollection<ChecklistTecnica> Checklists { get; set; } = new List<ChecklistTecnica>();
    public ICollection<PlanificacionInstalacion> Planificaciones { get; set; } = new List<PlanificacionInstalacion>();
    public ICollection<Intervencion> Intervenciones { get; set; } = new List<Intervencion>();
    public ICollection<AuditoriaExpediente> Auditorias { get; set; } = new List<AuditoriaExpediente>();
}
