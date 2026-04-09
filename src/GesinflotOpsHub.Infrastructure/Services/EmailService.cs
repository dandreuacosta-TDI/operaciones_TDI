using System.Net.Http.Json;
using System.Text.Json;
using GesinflotOpsHub.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace GesinflotOpsHub.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly HttpClient _http;
    private readonly IConfiguration _config;
    private readonly ILogger<EmailService> _logger;

    private string From => _config["Email:From"] ?? "info@gesinflot.com";
    private string AppBaseUrl => _config["App:BaseUrl"] ?? "https://localhost";

    public EmailService(IHttpClientFactory httpClientFactory, IConfiguration config, ILogger<EmailService> logger)
    {
        _http = httpClientFactory.CreateClient("Resend");
        _config = config;
        _logger = logger;
    }

    public async Task<bool> EnviarPresupuestoClienteAsync(string destinatario, string nombreCliente, string numeroPresupuesto, string tokenPortal, CancellationToken ct = default)
    {
        var url = $"{AppBaseUrl}/portal/{tokenPortal}";
        var asunto = $"Presupuesto {numeroPresupuesto} â€” Gesinflot";
        var cuerpo = $"""
            <html>
            <body style="font-family:Arial,sans-serif;max-width:600px;margin:auto">
              <h2 style="color:#1a4b8e">Gesinflot â€” Presupuesto de instalaciÃ³n</h2>
              <p>Estimado/a <strong>{HtmlEncode(nombreCliente)}</strong>,</p>
              <p>Le adjuntamos el presupuesto <strong>{HtmlEncode(numeroPresupuesto)}</strong> para la instalaciÃ³n de dispositivos de gestiÃ³n de flota.</p>
              <p>Para revisar y aceptar el presupuesto, acceda al siguiente enlace seguro:</p>
              <p style="text-align:center">
                <a href="{url}" style="background:#1a4b8e;color:white;padding:12px 24px;border-radius:4px;text-decoration:none;display:inline-block">
                  Ver y aceptar presupuesto
                </a>
              </p>
              <p style="color:#666;font-size:12px">Este enlace es vÃ¡lido durante 30 dÃ­as.</p>
              <hr/>
              <p style="color:#999;font-size:11px">Gesinflot Â· gestiÃ³n de flotas</p>
            </body>
            </html>
            """;

        return await EnviarAsync(destinatario, asunto, cuerpo, ct);
    }

    public async Task<bool> EnviarConfirmacionInstalacionAsync(string destinatario, string nombreCliente, DateTime fecha, string direccion, CancellationToken ct = default)
    {
        var asunto = $"ConfirmaciÃ³n de instalaciÃ³n â€” {fecha:dd/MM/yyyy}";
        var cuerpo = $"""
            <html>
            <body style="font-family:Arial,sans-serif;max-width:600px;margin:auto">
              <h2 style="color:#1a4b8e">ConfirmaciÃ³n de instalaciÃ³n</h2>
              <p>Estimado/a <strong>{HtmlEncode(nombreCliente)}</strong>,</p>
              <p>Le confirmamos la instalaciÃ³n programada para:</p>
              <ul>
                <li><strong>Fecha:</strong> {fecha:dddd, d 'de' MMMM 'de' yyyy}</li>
                <li><strong>Hora:</strong> {fecha:HH:mm}</li>
                <li><strong>DirecciÃ³n:</strong> {HtmlEncode(direccion)}</li>
              </ul>
              <p>Por favor, confirme la recepciÃ³n respondiendo a este correo o contactando con su comercial asignado.</p>
              <hr/>
              <p style="color:#999;font-size:11px">Gesinflot Â· gestiÃ³n de flotas</p>
            </body>
            </html>
            """;

        return await EnviarAsync(destinatario, asunto, cuerpo, ct);
    }

    public async Task<bool> EnviarNotificacionInternaAsync(string asunto, string cuerpo, CancellationToken ct = default)
    {
        var destinatario = _config["Email:NotificacionesInternas"] ?? From;
        return await EnviarAsync(destinatario, $"[Gesinflot Ops] {asunto}", $"<pre>{HtmlEncode(cuerpo)}</pre>", ct);
    }

    private async Task<bool> EnviarAsync(string destinatario, string asunto, string cuerpoHtml, CancellationToken ct)
    {
        try
        {
            var payload = new
            {
                from = From,
                to = new[] { destinatario },
                subject = asunto,
                html = cuerpoHtml
            };

            var response = await _http.PostAsJsonAsync("https://api.resend.com/emails", payload, ct);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Email enviado a {Destinatario}: {Asunto}", destinatario, asunto);
                return true;
            }

            var error = await response.Content.ReadAsStringAsync(ct);
            _logger.LogError("Resend API error {Status}: {Error}", (int)response.StatusCode, error);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error enviando email a {Destinatario}", destinatario);
            return false;
        }
    }

    private static string HtmlEncode(string text)
        => System.Net.WebUtility.HtmlEncode(text);
}
