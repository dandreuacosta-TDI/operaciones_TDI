using FluentValidation;
using GesinflotOpsHub.Application.DTOs;
using GesinflotOpsHub.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace GesinflotOpsHub.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IExpedienteService, ExpedienteService>();
        services.AddScoped<IIntervencionService, IntervencionService>();

        services.AddValidatorsFromAssemblyContaining<CrearExpedienteValidator>();

        return services;
    }
}

// ─── Validators ───────────────────────────────────────────────────────────────

public class CrearExpedienteValidator : AbstractValidator<CrearExpedienteDto>
{
    public CrearExpedienteValidator()
    {
        RuleFor(x => x.ClienteId).NotEmpty().WithMessage("ClienteId es obligatorio");
        RuleFor(x => x.TipoExpediente).IsInEnum().WithMessage("Tipo de expediente inválido");
    }
}
