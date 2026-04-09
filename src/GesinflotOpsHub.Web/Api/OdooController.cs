using GesinflotOpsHub.Application.Common.Interfaces;
using GesinflotOpsHub.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GesinflotOpsHub.Web.Api;

[ApiController]
[Route("api/odoo")]
[Authorize(Policy = "Operaciones")]
public class OdooController : ControllerBase
{
    private readonly IOdooService _odooService;
    private readonly IExpedienteService _expedienteService;

    public OdooController(IOdooService odooService, IExpedienteService expedienteService)
    {
        _odooService = odooService;
        _expedienteService = expedienteService;
    }

    /// <summary>GET /api/odoo/clientes — Lista clientes desde Odoo</summary>
    [HttpGet("clientes")]
    public async Task<IActionResult> GetClientes(CancellationToken ct)
    {
        var clientes = await _odooService.GetClientesAsync(ct);
        return Ok(clientes);
    }

    /// <summary>GET /api/odoo/presupuestos — Lista presupuestos desde Odoo</summary>
    [HttpGet("presupuestos")]
    public async Task<IActionResult> GetPresupuestos(CancellationToken ct)
    {
        var presupuestos = await _odooService.GetPresupuestosAsync(ct);
        return Ok(presupuestos);
    }

    /// <summary>POST /api/odoo/sync — Sincroniza clientes de Odoo a la base local</summary>
    [HttpPost("sync")]
    public async Task<IActionResult> SincronizarClientes(CancellationToken ct)
    {
        await _expedienteService.SincronizarDesdeOdooAsync(ct);
        return Ok(new { message = "Sincronización completada" });
    }
}
