using GesinflotOpsHub.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace GesinflotOpsHub.Infrastructure.Services;

/// <summary>
/// Integración con Odoo mediante JSON-RPC.
/// La API key se inyecta desde variable de entorno Odoo__ApiKey — nunca hardcodeada.
/// </summary>
public class OdooService : IOdooService
{
    private readonly HttpClient _http;
    private readonly IConfiguration _config;
    private readonly ILogger<OdooService> _logger;

    private string BaseUrl => _config["Odoo:BaseUrl"] ?? throw new InvalidOperationException("Odoo:BaseUrl no configurado");
    private string Database => _config["Odoo:Database"] ?? throw new InvalidOperationException("Odoo:Database no configurado");
    private string Username => _config["Odoo:Username"] ?? throw new InvalidOperationException("Odoo:Username no configurado");
    private string ApiKey => _config["Odoo:ApiKey"] ?? throw new InvalidOperationException("Odoo:ApiKey no configurado. Configúralo via variable de entorno Odoo__ApiKey");

    public OdooService(HttpClient http, IConfiguration config, ILogger<OdooService> logger)
    {
        _http = http;
        _config = config;
        _logger = logger;
    }

    public async Task<IEnumerable<OdooClienteDto>> GetClientesAsync(CancellationToken ct = default)
    {
        var result = await CallJsonRpcAsync<List<JsonElement>>("res.partner", "search_read",
            new object[] { new object[] { new object[] { "is_company", "=", true } } },
            new { fields = new[] { "id", "name", "email", "phone", "vat", "street", "city", "state_id" } },
            ct);

        return result?.Select(r => new OdooClienteDto(
            Id: r.GetProperty("id").ToString(),
            RazonSocial: r.TryGetProperty("name", out var n) ? n.GetString() ?? "" : "",
            Email: r.TryGetProperty("email", out var e) && e.ValueKind != JsonValueKind.False ? e.GetString() : null,
            Telefono: r.TryGetProperty("phone", out var p) && p.ValueKind != JsonValueKind.False ? p.GetString() : null,
            CIF: r.TryGetProperty("vat", out var v) && v.ValueKind != JsonValueKind.False ? v.GetString() : null,
            Direccion: r.TryGetProperty("street", out var s) && s.ValueKind != JsonValueKind.False ? s.GetString() : null,
            Ciudad: r.TryGetProperty("city", out var c) && c.ValueKind != JsonValueKind.False ? c.GetString() : null,
            Provincia: null
        )) ?? Enumerable.Empty<OdooClienteDto>();
    }

    public async Task<OdooClienteDto?> GetClienteAsync(string odooId, CancellationToken ct = default)
    {
        var clientes = await GetClientesAsync(ct);
        return clientes.FirstOrDefault(c => c.Id == odooId);
    }

    public async Task<IEnumerable<OdooPresupuestoDto>> GetPresupuestosAsync(CancellationToken ct = default)
    {
        var result = await CallJsonRpcAsync<List<JsonElement>>("sale.order", "search_read",
            new object[] { new object[] { new object[] { "state", "in", new[] { "draft", "sent", "sale" } } } },
            new { fields = new[] { "id", "name", "state", "partner_id", "user_id", "date_order", "amount_total", "currency_id", "order_line" } },
            ct);

        return result?.Select(r => new OdooPresupuestoDto(
            Id: r.GetProperty("id").ToString(),
            Nombre: r.TryGetProperty("name", out var n) ? n.GetString() ?? "" : "",
            Estado: r.TryGetProperty("state", out var st) ? st.GetString() ?? "" : "",
            ClienteId: r.TryGetProperty("partner_id", out var pid) && pid.ValueKind == JsonValueKind.Array
                ? pid[0].ToString() : "",
            ComercialId: r.TryGetProperty("user_id", out var uid) && uid.ValueKind == JsonValueKind.Array
                ? uid[0].ToString() : null,
            ComercialNombre: r.TryGetProperty("user_id", out var uname) && uname.ValueKind == JsonValueKind.Array
                ? uname[1].GetString() : null,
            FechaPresupuesto: r.TryGetProperty("date_order", out var dt) ? DateTime.Parse(dt.GetString() ?? DateTime.UtcNow.ToString()) : DateTime.UtcNow,
            ImporteTotal: r.TryGetProperty("amount_total", out var amt) ? amt.GetDecimal() : 0,
            Moneda: null,
            Lineas: new List<OdooLineaPresupuestoDto>()
        )) ?? Enumerable.Empty<OdooPresupuestoDto>();
    }

    public async Task<OdooPresupuestoDto?> GetPresupuestoAsync(string odooId, CancellationToken ct = default)
    {
        var todos = await GetPresupuestosAsync(ct);
        return todos.FirstOrDefault(p => p.Id == odooId);
    }

    public async Task<IEnumerable<OdooPresupuestoDto>> GetPresupuestosClienteAsync(string odooClienteId, CancellationToken ct = default)
    {
        var todos = await GetPresupuestosAsync(ct);
        return todos.Where(p => p.ClienteId == odooClienteId);
    }

    public async Task<IEnumerable<OdooSaleOrderDto>> GetSaleOrdersAsync(CancellationToken ct = default)
    {
        var result = await CallJsonRpcAsync<List<JsonElement>>("sale.order", "search_read",
            new object[] { new object[] { new object[] { "state", "=", "sale" } } },
            new { fields = new[] { "id", "name", "state", "partner_id", "amount_total", "date_order" } },
            ct);

        return result?.Select(r => new OdooSaleOrderDto(
            Id: r.GetProperty("id").ToString(),
            Nombre: r.TryGetProperty("name", out var n) ? n.GetString() ?? "" : "",
            Estado: r.TryGetProperty("state", out var st) ? st.GetString() ?? "" : "",
            ClienteId: r.TryGetProperty("partner_id", out var pid) && pid.ValueKind == JsonValueKind.Array ? pid[0].ToString() : "",
            PresupuestoOrigen: null,
            ImporteTotal: r.TryGetProperty("amount_total", out var amt) ? amt.GetDecimal() : 0,
            FechaPedido: r.TryGetProperty("date_order", out var dt) ? DateTime.Parse(dt.GetString() ?? DateTime.UtcNow.ToString()) : DateTime.UtcNow
        )) ?? Enumerable.Empty<OdooSaleOrderDto>();
    }

    public async Task<OdooSaleOrderDto?> GetSaleOrderAsync(string odooId, CancellationToken ct = default)
    {
        var todos = await GetSaleOrdersAsync(ct);
        return todos.FirstOrDefault(s => s.Id == odooId);
    }

    public async Task<bool> ActualizarEstadoExpedienteAsync(string odooBudgetId, string estado, CancellationToken ct = default)
    {
        try
        {
            await WriteJsonRpcAsync("sale.order", "write",
                new object[] { new[] { int.Parse(odooBudgetId) }, new { x_estado_gesinflot = estado } },
                ct);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "No se pudo actualizar estado en Odoo para {Id}", odooBudgetId);
            return false;
        }
    }

    public async Task<bool> RegistrarAceptacionAsync(string odooBudgetId, DateTime fechaAceptacion, CancellationToken ct = default)
    {
        try
        {
            await WriteJsonRpcAsync("sale.order", "write",
                new object[] { new[] { int.Parse(odooBudgetId) }, new { x_fecha_aceptacion = fechaAceptacion.ToString("yyyy-MM-dd") } },
                ct);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "No se pudo registrar aceptación en Odoo para {Id}", odooBudgetId);
            return false;
        }
    }

    public async Task<bool> ActualizarFechaInstalacionAsync(string odooBudgetId, DateTime fecha, CancellationToken ct = default)
    {
        try
        {
            await WriteJsonRpcAsync("sale.order", "write",
                new object[] { new[] { int.Parse(odooBudgetId) }, new { x_fecha_instalacion = fecha.ToString("yyyy-MM-dd") } },
                ct);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "No se pudo actualizar fecha instalación en Odoo para {Id}", odooBudgetId);
            return false;
        }
    }

    public async Task<bool> RegistrarIntervencionRealizadaAsync(string odooSaleOrderId, bool esFacturable, decimal? importe, CancellationToken ct = default)
    {
        try
        {
            await WriteJsonRpcAsync("sale.order", "write",
                new object[]
                {
                    new[] { int.Parse(odooSaleOrderId) },
                    new
                    {
                        x_intervencion_realizada = true,
                        x_es_facturable = esFacturable,
                        x_importe_intervencion = importe ?? 0
                    }
                },
                ct);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "No se pudo registrar intervención en Odoo para {Id}", odooSaleOrderId);
            return false;
        }
    }

    // ─── Private JSON-RPC helpers ────────────────────────────────────────────

    private async Task<T?> CallJsonRpcAsync<T>(string model, string method, object[] args, object kwargs, CancellationToken ct)
    {
        var uid = await AuthenticateAsync(ct);
        if (uid <= 0) return default;

        var payload = new
        {
            jsonrpc = "2.0",
            method = "call",
            id = 1,
            @params = new
            {
                service = "object",
                method = "execute_kw",
                args = new object[] { Database, uid, ApiKey, model, method, args, kwargs }
            }
        };

        var response = await _http.PostAsJsonAsync($"{BaseUrl}/jsonrpc", payload, ct);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadFromJsonAsync<JsonElement>(cancellationToken: ct);
        if (json.TryGetProperty("result", out var result))
            return JsonSerializer.Deserialize<T>(result.GetRawText());

        _logger.LogWarning("Odoo JSON-RPC error en {Model}.{Method}", model, method);
        return default;
    }

    private async Task WriteJsonRpcAsync(string model, string method, object[] args, CancellationToken ct)
    {
        var uid = await AuthenticateAsync(ct);
        if (uid <= 0) throw new InvalidOperationException("Autenticación Odoo fallida");

        var payload = new
        {
            jsonrpc = "2.0",
            method = "call",
            id = 1,
            @params = new
            {
                service = "object",
                method = "execute_kw",
                args = new object[] { Database, uid, ApiKey, model, method, args }
            }
        };

        var response = await _http.PostAsJsonAsync($"{BaseUrl}/jsonrpc", payload, ct);
        response.EnsureSuccessStatusCode();
    }

    private async Task<int> AuthenticateAsync(CancellationToken ct)
    {
        var payload = new
        {
            jsonrpc = "2.0",
            method = "call",
            id = 1,
            @params = new
            {
                service = "common",
                method = "authenticate",
                args = new object[] { Database, Username, ApiKey, new { } }
            }
        };

        var response = await _http.PostAsJsonAsync($"{BaseUrl}/jsonrpc", payload, ct);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadFromJsonAsync<JsonElement>(cancellationToken: ct);
        if (json.TryGetProperty("result", out var uid) && uid.ValueKind == JsonValueKind.Number)
            return uid.GetInt32();

        _logger.LogError("Autenticación con Odoo fallida. Verifica Odoo__Username y Odoo__ApiKey");
        return -1;
    }
}
