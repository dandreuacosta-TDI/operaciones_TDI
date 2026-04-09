using GesinflotOpsHub.Application.Common.Interfaces;
using GesinflotOpsHub.Application.DTOs;
using GesinflotOpsHub.Domain.Entities;
using GesinflotOpsHub.Domain.Enums;
using GesinflotOpsHub.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace GesinflotOpsHub.Application.Services;

public interface IExpedienteService
{
    Task<IEnumerable<ExpedienteListDto>> GetAllAsync(CancellationToken ct = default);
    Task<IEnumerable<ExpedienteListDto>> GetByComercialAsync(string comercialId, CancellationToken ct = default);
    Task<ExpedienteDetailDto?> GetDetailAsync(Guid id, CancellationToken ct = default);
    Task<Guid> CrearAsync(CrearExpedienteDto dto, string usuarioCrea, CancellationToken ct = default);
    Task AvanzarEstadoAsync(ActualizarEstadoExpedienteDto dto, string usuario, CancellationToken ct = default);
    Task<string?> GenerarTokenPortalAsync(Guid id, CancellationToken ct = default);
    Task<ExpedienteDetailDto?> GetByTokenPortalAsync(string token, CancellationToken ct = default);
    Task SincronizarDesdeOdooAsync(CancellationToken ct = default);
}

public class ExpedienteService : IExpedienteService
{
    private readonly IExpedienteRepository _expedienteRepo;
    private readonly IClienteRepository _clienteRepo;
    private readonly IOdooService _odooService;
    private readonly IEmailService _emailService;
    private readonly ILogger<ExpedienteService> _logger;

    public ExpedienteService(
        IExpedienteRepository expedienteRepo,
        IClienteRepository clienteRepo,
        IOdooService odooService,
        IEmailService emailService,
        ILogger<ExpedienteService> logger)
    {
        _expedienteRepo = expedienteRepo;
        _clienteRepo = clienteRepo;
        _odooService = odooService;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<IEnumerable<ExpedienteListDto>> GetAllAsync(CancellationToken ct = default)
    {
        var expedientes = await _expedienteRepo.GetAllAsync(ct);
        return expedientes.Select(MapToListDto);
    }

    public async Task<IEnumerable<ExpedienteListDto>> GetByComercialAsync(string comercialId, CancellationToken ct = default)
    {
        var expedientes = await _expedienteRepo.GetByComercialAsync(comercialId, ct);
        return expedientes.Select(MapToListDto);
    }

    public async Task<ExpedienteDetailDto?> GetDetailAsync(Guid id, CancellationToken ct = default)
    {
        var e = await _expedienteRepo.GetByIdAsync(id, ct);
        return e is null ? null : MapToDetailDto(e);
    }

    public async Task<Guid> CrearAsync(CrearExpedienteDto dto, string usuarioCrea, CancellationToken ct = default)
    {
        var codigo = await GenerarCodigoExpedienteAsync(ct);

        var expediente = new ExpedienteInstalacion
        {
            CodigoExpediente = codigo,
            ClienteId = dto.ClienteId,
            TipoExpediente = dto.TipoExpediente,
            ClienteExistente = dto.ClienteExistente,
            OdooBudgetId = dto.OdooBudgetId,
            OdooSaleOrderId = dto.OdooSaleOrderId,
            NumeroPresupuesto = dto.NumeroPresupuesto,
            ComercialResponsable = dto.ComercialResponsable,
            ImportePresupuestado = dto.ImportePresupuestado,
            Observaciones = dto.Observaciones,
            Estado = EstadoExpediente.Borrador,
            CreadoPor = usuarioCrea
        };

        await _expedienteRepo.AddAsync(expediente, ct);
        await _expedienteRepo.SaveChangesAsync(ct);

        _logger.LogInformation("Expediente {Codigo} creado por {Usuario}", codigo, usuarioCrea);
        return expediente.Id;
    }

    public async Task AvanzarEstadoAsync(ActualizarEstadoExpedienteDto dto, string usuario, CancellationToken ct = default)
    {
        var expediente = await _expedienteRepo.GetByIdAsync(dto.Id, ct)
            ?? throw new KeyNotFoundException($"Expediente {dto.Id} no encontrado");

        var estadoAnterior = expediente.Estado;
        expediente.Estado = dto.NuevoEstado;
        expediente.FechaUltimaActualizacion = DateTime.UtcNow;
        expediente.ModificadoPor = usuario;

        // Sincronizar estado hacia Odoo si tiene vinculación
        if (!string.IsNullOrEmpty(expediente.OdooBudgetId))
        {
            await _odooService.ActualizarEstadoExpedienteAsync(
                expediente.OdooBudgetId,
                dto.NuevoEstado.ToString(),
                ct);
        }

        _expedienteRepo.Update(expediente);
        await _expedienteRepo.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Expediente {Codigo} estado {Anterior} → {Nuevo} por {Usuario}",
            expediente.CodigoExpediente, estadoAnterior, dto.NuevoEstado, usuario);
    }

    public async Task<string?> GenerarTokenPortalAsync(Guid id, CancellationToken ct = default)
    {
        var expediente = await _expedienteRepo.GetByIdAsync(id, ct);
        if (expediente is null) return null;

        expediente.TokenPortalCliente = Convert.ToBase64String(Guid.NewGuid().ToByteArray())
            .Replace("+", "-").Replace("/", "_").TrimEnd('=');
        expediente.TokenExpiracion = DateTime.UtcNow.AddDays(30);
        expediente.FechaUltimaActualizacion = DateTime.UtcNow;

        _expedienteRepo.Update(expediente);
        await _expedienteRepo.SaveChangesAsync(ct);

        return expediente.TokenPortalCliente;
    }

    public async Task<ExpedienteDetailDto?> GetByTokenPortalAsync(string token, CancellationToken ct = default)
    {
        var e = await _expedienteRepo.GetByTokenPortalAsync(token, ct);
        if (e is null || e.TokenExpiracion < DateTime.UtcNow) return null;
        return MapToDetailDto(e);
    }

    public async Task SincronizarDesdeOdooAsync(CancellationToken ct = default)
    {
        var clientes = await _odooService.GetClientesAsync(ct);
        foreach (var oc in clientes)
        {
            var existente = await _clienteRepo.GetByOdooIdAsync(oc.Id, ct);
            if (existente is null)
            {
                var nuevo = new Cliente
                {
                    OdooPartnerId = oc.Id,
                    RazonSocial = oc.RazonSocial,
                    Email = oc.Email,
                    Telefono = oc.Telefono,
                    CIF = oc.CIF
                };
                await _clienteRepo.AddAsync(nuevo, ct);
            }
            else
            {
                existente.RazonSocial = oc.RazonSocial;
                existente.Email = oc.Email;
                _clienteRepo.Update(existente);
            }
        }
        await _clienteRepo.SaveChangesAsync(ct);
        _logger.LogInformation("Sincronización Odoo completada: {Count} clientes", clientes.Count());
    }

    // ─── Private helpers ─────────────────────────────────────────────────────

    private static string GenerarCodigoSecuencial(int numero, int año)
        => $"EXP-{año}-{numero:D5}";

    private async Task<string> GenerarCodigoExpedienteAsync(CancellationToken ct)
    {
        var año = DateTime.UtcNow.Year;
        var todos = await _expedienteRepo.GetAllAsync(ct);
        var count = todos.Count(e => e.FechaCreacion.Year == año);
        return GenerarCodigoSecuencial(count + 1, año);
    }

    private static ExpedienteListDto MapToListDto(ExpedienteInstalacion e) => new(
        e.Id, e.CodigoExpediente,
        e.Cliente?.RazonSocial ?? "—",
        e.TipoExpediente, e.Estado,
        e.NumeroPresupuesto, e.ComercialResponsable,
        e.FechaCreacion, e.FechaInstalacionPrevista,
        e.EsFacturable, e.ImportePresupuestado
    );

    private static ExpedienteDetailDto MapToDetailDto(ExpedienteInstalacion e) => new(
        e.Id, e.CodigoExpediente, e.ClienteId,
        e.Cliente?.RazonSocial ?? "—",
        e.OdooCustomerId, e.OdooBudgetId, e.OdooSaleOrderId,
        e.NumeroPresupuesto, e.NumeroPedidoVenta,
        e.TipoExpediente, e.Estado, e.ClienteExistente,
        e.ComercialResponsable,
        e.FechaPresupuesto, e.FechaAceptacion,
        e.FechaInstalacionPrevista, e.FechaInstalacionReal,
        e.FechaFacturacion,
        e.ImportePresupuestado, e.ImporteFacturado, e.CosteOperativo,
        e.EsFacturable, e.Observaciones,
        e.FechaCreacion, e.FechaUltimaActualizacion
    );
}
