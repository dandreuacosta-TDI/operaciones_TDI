using GesinflotOpsHub.Domain.Enums;

namespace GesinflotOpsHub.Application.Common.Interfaces;

// ─── Odoo DTOs ───────────────────────────────────────────────────────────────

public record OdooClienteDto(
    string Id,
    string RazonSocial,
    string? Email,
    string? Telefono,
    string? CIF,
    string? Direccion,
    string? Ciudad,
    string? Provincia
);

public record OdooPresupuestoDto(
    string Id,
    string Nombre,
    string Estado,
    string ClienteId,
    string? ComercialId,
    string? ComercialNombre,
    DateTime FechaPresupuesto,
    decimal ImporteTotal,
    string? Moneda,
    List<OdooLineaPresupuestoDto> Lineas
);

public record OdooLineaPresupuestoDto(
    string Id,
    string Descripcion,
    decimal Cantidad,
    decimal PrecioUnitario,
    decimal Subtotal
);

public record OdooSaleOrderDto(
    string Id,
    string Nombre,
    string Estado,
    string ClienteId,
    string? PresupuestoOrigen,
    decimal ImporteTotal,
    DateTime FechaPedido
);

// ─── Interfaces de servicios externos ────────────────────────────────────────

public interface IOdooService
{
    // Clientes
    Task<IEnumerable<OdooClienteDto>> GetClientesAsync(CancellationToken ct = default);
    Task<OdooClienteDto?> GetClienteAsync(string odooId, CancellationToken ct = default);

    // Presupuestos
    Task<IEnumerable<OdooPresupuestoDto>> GetPresupuestosAsync(CancellationToken ct = default);
    Task<OdooPresupuestoDto?> GetPresupuestoAsync(string odooId, CancellationToken ct = default);
    Task<IEnumerable<OdooPresupuestoDto>> GetPresupuestosClienteAsync(string odooClienteId, CancellationToken ct = default);

    // Sale Orders
    Task<IEnumerable<OdooSaleOrderDto>> GetSaleOrdersAsync(CancellationToken ct = default);
    Task<OdooSaleOrderDto?> GetSaleOrderAsync(string odooId, CancellationToken ct = default);

    // Escritura hacia Odoo
    Task<bool> ActualizarEstadoExpedienteAsync(string odooBudgetId, string estado, CancellationToken ct = default);
    Task<bool> RegistrarAceptacionAsync(string odooBudgetId, DateTime fechaAceptacion, CancellationToken ct = default);
    Task<bool> ActualizarFechaInstalacionAsync(string odooBudgetId, DateTime fecha, CancellationToken ct = default);
    Task<bool> RegistrarIntervencionRealizadaAsync(string odooSaleOrderId, bool esFacturable, decimal? importe, CancellationToken ct = default);
}

public interface IEmailService
{
    Task<bool> EnviarPresupuestoClienteAsync(string destinatario, string nombreCliente, string numeroPresupuesto, string tokenPortal, CancellationToken ct = default);
    Task<bool> EnviarConfirmacionInstalacionAsync(string destinatario, string nombreCliente, DateTime fecha, string direccion, CancellationToken ct = default);
    Task<bool> EnviarNotificacionInternaAsync(string asunto, string cuerpo, CancellationToken ct = default);
}

public interface IDashboardService
{
    Task<bool> RefrescarKpisAsync(CancellationToken ct = default);
    Task<bool> EnviarDatosIntervencionAsync(Guid intervencionId, CancellationToken ct = default);
}
