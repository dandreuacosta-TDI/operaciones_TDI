using GesinflotOpsHub.Application.Common.Interfaces;
using GesinflotOpsHub.Domain.Interfaces;
using GesinflotOpsHub.Infrastructure.Persistence;
using GesinflotOpsHub.Infrastructure.Repositories;
using GesinflotOpsHub.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GesinflotOpsHub.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // ── Database ──────────────────────────────────────────────────────
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                npgsql => npgsql.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName)));

        // ── Repositories ──────────────────────────────────────────────────
        services.AddScoped<IExpedienteRepository, ExpedienteRepository>();
        services.AddScoped<IClienteRepository, ClienteRepository>();
        services.AddScoped<IIntervencionRepository, IntervencionRepository>();
        services.AddScoped<IInstaladorRepository, InstaladorRepository>();

        // ── External Services ─────────────────────────────────────────────
        services.AddHttpClient<IOdooService, OdooService>(client =>
        {
            var baseUrl = configuration["Odoo:BaseUrl"];
            if (!string.IsNullOrEmpty(baseUrl))
                client.BaseAddress = new Uri(baseUrl);
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IDashboardService, DashboardService>();

        return services;
    }
}
