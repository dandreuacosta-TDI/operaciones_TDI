using GesinflotOpsHub.Domain.Enums;

namespace GesinflotOpsHub.Application.DTOs;

// ─── Expediente ───────────────────────────────────────────────────────────────

public record ExpedienteListDto(
    Guid Id,
    string CodigoExpediente,
    string RazonSocialCliente,
    TipoExpediente TipoExpediente,
    EstadoExpediente Estado,
    string? NumeroPresupuesto,
    string? ComercialResponsable,
    DateTime FechaCreacion,
    DateTime? FechaInstalacionPrevista,
    bool EsFacturable,
    decimal? ImportePresupuestado
);

public record ExpedienteDetailDto(
    Guid Id,
    string CodigoExpediente,
    Guid ClienteId,
    string RazonSocialCliente,
    string? OdooCustomerId,
    string? OdooBudgetId,
    string? OdooSaleOrderId,
    string? NumeroPresupuesto,
    string? NumeroPedidoVenta,
    TipoExpediente TipoExpediente,
    EstadoExpediente Estado,
    bool ClienteExistente,
    string? ComercialResponsable,
    DateTime? FechaPresupuesto,
    DateTime? FechaAceptacion,
    DateTime? FechaInstalacionPrevista,
    DateTime? FechaInstalacionReal,
    DateTime? FechaFacturacion,
    decimal? ImportePresupuestado,
    decimal? ImporteFacturado,
    decimal? CosteOperativo,
    bool EsFacturable,
    string? Observaciones,
    DateTime FechaCreacion,
    DateTime FechaUltimaActualizacion
);

public record CrearExpedienteDto(
    Guid ClienteId,
    TipoExpediente TipoExpediente,
    bool ClienteExistente,
    string? OdooBudgetId,
    string? OdooSaleOrderId,
    string? NumeroPresupuesto,
    string? ComercialResponsable,
    decimal? ImportePresupuestado,
    string? Observaciones
);

public record ActualizarEstadoExpedienteDto(
    Guid Id,
    EstadoExpediente NuevoEstado,
    string? Observacion
);

// ─── Intervención ─────────────────────────────────────────────────────────────

public record IntervencionListDto(
    Guid Id,
    string Codigo,
    string RazonSocialCliente,
    string? NombreInstalador,
    TipoIntervencion Tipo,
    EstadoIntervencion Estado,
    DateTime FechaPlanificada,
    DateTime? FechaEjecucion,
    string? Provincia,
    bool EsFacturable,
    decimal? Importe,
    decimal? Margen
);

public record IntervencionDetailDto(
    Guid Id,
    string Codigo,
    Guid ExpedienteId,
    string CodigoExpediente,
    Guid ClienteId,
    string RazonSocialCliente,
    Guid? InstaladorId,
    string? NombreInstalador,
    TipoIntervencion Tipo,
    EstadoIntervencion Estado,
    OrigenIntervencion Origen,
    DateTime FechaPlanificada,
    DateTime? FechaEjecucion,
    int? DuracionRealMinutos,
    string? DireccionEjecucion,
    string? Provincia,
    bool EsFacturable,
    decimal? Importe,
    decimal? Coste,
    decimal? Margen,
    string? Descripcion,
    string? ResultadoTecnico,
    bool SincronizadoOdoo
);

// ─── KPIs para Dashboard ─────────────────────────────────────────────────────

public record KpiOperacionesDto(
    int TotalExpedientes,
    int ExpedientesActivos,
    int IntervencionesEstesMes,
    int IntervencionesRealizadas,
    decimal FacturacionMes,
    decimal FacturacionAcumulada,
    decimal MargenMedio,
    int InstalacionesPendientes,
    int ChecklistsPendientes,
    Dictionary<string, int> IntervencionesiPorTipo,
    Dictionary<string, int> IntervencionsesPorProvincia,
    Dictionary<string, decimal> FacturacionPorComercial
);

// ─── Cliente ─────────────────────────────────────────────────────────────────

public record ClienteListDto(
    Guid Id,
    string RazonSocial,
    string? NombreComercial,
    string? CIF,
    string? Email,
    string? Provincia,
    bool Activo,
    int NumExpedientes
);

// ─── Planificación ───────────────────────────────────────────────────────────

public record PlanificacionDto(
    Guid Id,
    Guid ExpedienteId,
    string CodigoExpediente,
    string RazonSocialCliente,
    Guid InstaladorId,
    string NombreInstalador,
    DateTime FechaPlanificada,
    TimeSpan? HoraPlanificada,
    bool ConfirmadoConCliente,
    string? DireccionInstalacion,
    string? Provincia,
    string? ContactoCliente,
    string? TelefonoContacto
);
