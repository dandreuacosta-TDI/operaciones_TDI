using GesinflotOpsHub.Application.Common.Interfaces;
using GesinflotOpsHub.Application.Services;
using Microsoft.Extensions.Logging;

namespace GesinflotOpsHub.Infrastructure.Services;

public class DashboardService : IDashboardService
{
    private readonly IIntervencionService _intervencionService;
    private readonly ILogger<DashboardService> _logger;

    public DashboardService(IIntervencionService intervencionService, ILogger<DashboardService> logger)
    {
        _intervencionService = intervencionService;
        _logger = logger;
    }

    public async Task<bool> RefrescarKpisAsync(CancellationToken ct = default)
    {
        // Puede conectarse a un dashboard externo (Power BI, Metabase, etc.)
        // Por ahora registra que los KPIs están disponibles en /api/kpis/operaciones
        _logger.LogInformation("KPIs refrescados — disponibles en /api/kpis/operaciones");
        return await Task.FromResult(true);
    }

    public async Task<bool> EnviarDatosIntervencionAsync(Guid intervencionId, CancellationToken ct = default)
    {
        var intervencion = await _intervencionService.GetDetailAsync(intervencionId, ct);
        if (intervencion is null) return false;
        _logger.LogInformation("Datos de intervención {Codigo} disponibles en API", intervencion.Codigo);
        return true;
    }
}
