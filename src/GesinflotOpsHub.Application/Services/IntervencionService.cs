using GesinflotOpsHub.Application.DTOs;
using GesinflotOpsHub.Domain.Entities;
using GesinflotOpsHub.Domain.Enums;
using GesinflotOpsHub.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace GesinflotOpsHub.Application.Services;

public interface IIntervencionService
{
    Task<IEnumerable<IntervencionListDto>> GetAllAsync(CancellationToken ct = default);
    Task<IEnumerable<IntervencionListDto>> GetByInstaladorAsync(Guid instaladorId, CancellationToken ct = default);
    Task<IEnumerable<IntervencionListDto>> GetByClienteAsync(Guid clienteId, CancellationToken ct = default);
    Task<IntervencionDetailDto?> GetDetailAsync(Guid id, CancellationToken ct = default);
    Task<Guid> CrearAsync(Guid expedienteId, TipoIntervencion tipo, DateTime fechaPlanificada, Guid? instaladorId, CancellationToken ct = default);
    Task MarcarRealizadaAsync(Guid id, DateTime fechaEjecucion, string? resultado, bool esFacturable, decimal? importe, decimal? coste, CancellationToken ct = default);
    Task<KpiOperacionesDto> GetKpisAsync(int? año, int? mes, CancellationToken ct = default);
}

public class IntervencionService : IIntervencionService
{
    private readonly IIntervencionRepository _intervencionRepo;
    private readonly IExpedienteRepository _expedienteRepo;
    private readonly ILogger<IntervencionService> _logger;

    public IntervencionService(
        IIntervencionRepository intervencionRepo,
        IExpedienteRepository expedienteRepo,
        ILogger<IntervencionService> logger)
    {
        _intervencionRepo = intervencionRepo;
        _expedienteRepo = expedienteRepo;
        _logger = logger;
    }

    public async Task<IEnumerable<IntervencionListDto>> GetAllAsync(CancellationToken ct = default)
    {
        var list = await _intervencionRepo.GetAllAsync(ct);
        return list.Select(MapToListDto);
    }

    public async Task<IEnumerable<IntervencionListDto>> GetByInstaladorAsync(Guid instaladorId, CancellationToken ct = default)
    {
        var list = await _intervencionRepo.GetByInstaladorAsync(instaladorId, ct);
        return list.Select(MapToListDto);
    }

    public async Task<IEnumerable<IntervencionListDto>> GetByClienteAsync(Guid clienteId, CancellationToken ct = default)
    {
        var list = await _intervencionRepo.GetByClienteAsync(clienteId, ct);
        return list.Select(MapToListDto);
    }

    public async Task<IntervencionDetailDto?> GetDetailAsync(Guid id, CancellationToken ct = default)
    {
        var i = await _intervencionRepo.GetByIdAsync(id, ct);
        return i is null ? null : MapToDetailDto(i);
    }

    public async Task<Guid> CrearAsync(Guid expedienteId, TipoIntervencion tipo, DateTime fechaPlanificada, Guid? instaladorId, CancellationToken ct = default)
    {
        var expediente = await _expedienteRepo.GetByIdAsync(expedienteId, ct)
            ?? throw new KeyNotFoundException($"Expediente {expedienteId} no encontrado");

        var codigo = $"INT-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..6].ToUpper()}";

        var intervencion = new Intervencion
        {
            Codigo = codigo,
            ExpedienteId = expedienteId,
            ClienteId = expediente.ClienteId,
            InstaladorId = instaladorId,
            Tipo = tipo,
            Estado = EstadoIntervencion.Planificada,
            FechaPlanificada = fechaPlanificada
        };

        await _intervencionRepo.AddAsync(intervencion, ct);
        await _intervencionRepo.SaveChangesAsync(ct);

        _logger.LogInformation("Intervención {Codigo} creada para expediente {ExpId}", codigo, expedienteId);
        return intervencion.Id;
    }

    public async Task MarcarRealizadaAsync(Guid id, DateTime fechaEjecucion, string? resultado, bool esFacturable, decimal? importe, decimal? coste, CancellationToken ct = default)
    {
        var intervencion = await _intervencionRepo.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException($"Intervención {id} no encontrada");

        intervencion.Estado = EstadoIntervencion.Realizada;
        intervencion.FechaEjecucion = fechaEjecucion;
        intervencion.ResultadoTecnico = resultado;
        intervencion.EsFacturable = esFacturable;
        intervencion.Importe = importe;
        intervencion.Coste = coste;
        intervencion.SincronizadoOdoo = false; // pendiente de sync

        _intervencionRepo.Update(intervencion);
        await _intervencionRepo.SaveChangesAsync(ct);

        _logger.LogInformation("Intervención {Codigo} marcada como realizada", intervencion.Codigo);
    }

    public async Task<KpiOperacionesDto> GetKpisAsync(int? año, int? mes, CancellationToken ct = default)
    {
        var todasIntervenciones = await _intervencionRepo.GetAllAsync(ct);
        var todosExpedientes = await _expedienteRepo.GetAllAsync(ct);

        var ahora = DateTime.UtcNow;
        var añoActual = año ?? ahora.Year;
        var mesActual = mes ?? ahora.Month;

        var delMes = todasIntervenciones
            .Where(i => i.FechaPlanificada.Year == añoActual && i.FechaPlanificada.Month == mesActual)
            .ToList();

        var realizadas = todasIntervenciones
            .Where(i => i.Estado == EstadoIntervencion.Realizada)
            .ToList();

        var facturacionMes = delMes
            .Where(i => i.EsFacturable && i.Importe.HasValue)
            .Sum(i => i.Importe!.Value);

        var facturacionTotal = realizadas
            .Where(i => i.EsFacturable && i.Importe.HasValue)
            .Sum(i => i.Importe!.Value);

        var margenMedio = realizadas.Any(i => i.Margen.HasValue)
            ? realizadas.Where(i => i.Margen.HasValue).Average(i => i.MargenPorcentaje ?? 0)
            : 0;

        return new KpiOperacionesDto(
            TotalExpedientes: todosExpedientes.Count(),
            ExpedientesActivos: todosExpedientes.Count(e =>
                e.Estado != EstadoExpediente.Cancelado && e.Estado != EstadoExpediente.Facturado),
            IntervencionesEstesMes: delMes.Count,
            IntervencionesRealizadas: realizadas.Count,
            FacturacionMes: facturacionMes,
            FacturacionAcumulada: facturacionTotal,
            MargenMedio: margenMedio,
            InstalacionesPendientes: todosExpedientes.Count(e =>
                e.Estado == EstadoExpediente.PlanificacionConfirmada),
            ChecklistsPendientes: todosExpedientes.Count(e =>
                e.Estado == EstadoExpediente.ChecklistPendiente),
            IntervencionesiPorTipo: todasIntervenciones
                .GroupBy(i => i.Tipo.ToString())
                .ToDictionary(g => g.Key, g => g.Count()),
            IntervencionsesPorProvincia: todasIntervenciones
                .Where(i => i.Provincia != null)
                .GroupBy(i => i.Provincia!)
                .ToDictionary(g => g.Key, g => g.Count()),
            FacturacionPorComercial: todosExpedientes
                .Where(e => e.ComercialResponsable != null && e.ImporteFacturado.HasValue)
                .GroupBy(e => e.ComercialResponsable!)
                .ToDictionary(g => g.Key, g => g.Sum(e => e.ImporteFacturado!.Value))
        );
    }

    private static IntervencionListDto MapToListDto(Intervencion i) => new(
        i.Id, i.Codigo,
        i.Cliente?.RazonSocial ?? "—",
        i.Instalador?.NombreCompleto,
        i.Tipo, i.Estado,
        i.FechaPlanificada, i.FechaEjecucion,
        i.Provincia, i.EsFacturable,
        i.Importe, i.Margen
    );

    private static IntervencionDetailDto MapToDetailDto(Intervencion i) => new(
        i.Id, i.Codigo,
        i.ExpedienteId, i.Expediente?.CodigoExpediente ?? "—",
        i.ClienteId, i.Cliente?.RazonSocial ?? "—",
        i.InstaladorId, i.Instalador?.NombreCompleto,
        i.Tipo, i.Estado, i.Origen,
        i.FechaPlanificada, i.FechaEjecucion, i.DuracionRealMinutos,
        i.DireccionEjecucion, i.Provincia,
        i.EsFacturable, i.Importe, i.Coste, i.Margen,
        i.Descripcion, i.ResultadoTecnico,
        i.SincronizadoOdoo
    );
}
