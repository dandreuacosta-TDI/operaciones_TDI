using GesinflotOpsHub.Application.DTOs;
using GesinflotOpsHub.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GesinflotOpsHub.Web.Api;

/// <summary>
/// API REST consumida por el dashboard de gestión externo.
/// Autenticación via Bearer token (API key).
/// </summary>
[ApiController]
[Route("api/intervenciones")]
[Authorize(Policy = "Dashboard")]
public class IntervencionesController : ControllerBase
{
    private readonly IIntervencionService _intervencionService;

    public IntervencionesController(IIntervencionService intervencionService)
        => _intervencionService = intervencionService;

    /// <summary>GET /api/intervenciones — Lista todas las intervenciones</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<IntervencionListDto>), 200)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await _intervencionService.GetAllAsync(ct);
        return Ok(result);
    }

    /// <summary>GET /api/intervenciones/{id}</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(IntervencionDetailDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await _intervencionService.GetDetailAsync(id, ct);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>GET /api/intervenciones/cliente/{clienteId}</summary>
    [HttpGet("cliente/{clienteId:guid}")]
    [ProducesResponseType(typeof(IEnumerable<IntervencionListDto>), 200)]
    public async Task<IActionResult> GetByCliente(Guid clienteId, CancellationToken ct)
    {
        var result = await _intervencionService.GetByClienteAsync(clienteId, ct);
        return Ok(result);
    }
}

[ApiController]
[Route("api/kpis")]
[Authorize(Policy = "Dashboard")]
public class KpisController : ControllerBase
{
    private readonly IIntervencionService _intervencionService;

    public KpisController(IIntervencionService intervencionService)
        => _intervencionService = intervencionService;

    /// <summary>GET /api/kpis/operaciones?año=2025&amp;mes=4</summary>
    [HttpGet("operaciones")]
    [ProducesResponseType(typeof(KpiOperacionesDto), 200)]
    public async Task<IActionResult> GetOperaciones(
        [FromQuery] int? año,
        [FromQuery] int? mes,
        CancellationToken ct)
    {
        var kpis = await _intervencionService.GetKpisAsync(año, mes, ct);
        return Ok(kpis);
    }
}
