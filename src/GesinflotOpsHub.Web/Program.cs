using GesinflotOpsHub.Application;
using GesinflotOpsHub.Infrastructure;
using GesinflotOpsHub.Infrastructure.Persistence;
using GesinflotOpsHub.Web;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Formatting.Compact;

// ── Bootstrap Serilog early (logs estructurados a stdout para Railway) ────────
Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .Enrich.WithEnvironmentName()
    .Enrich.WithThreadId()
    .WriteTo.Console(new CompactJsonFormatter())
    .CreateBootstrapLogger();

try
{
    Log.Information("Iniciando GesinflotOpsHub v1.0");

    var builder = WebApplication.CreateBuilder(args);

    // ── Serilog completo desde configuración ──────────────────────────────
    builder.Host.UseSerilog((ctx, lc) => lc
        .ReadFrom.Configuration(ctx.Configuration)
        .Enrich.FromLogContext()
        .Enrich.WithEnvironmentName()
        .WriteTo.Console(new CompactJsonFormatter()));

    // ── Puerto dinámico Railway (variable PORT) ───────────────────────────
    var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
    builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

    // ── Layers ────────────────────────────────────────────────────────────
    builder.Services.AddApplication();
    builder.Services.AddInfrastructure(builder.Configuration);

    // ── Identity ──────────────────────────────────────────────────────────
    builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
    {
        options.Password.RequireDigit = true;
        options.Password.RequiredLength = 8;
        options.Password.RequireUppercase = false;
        options.Password.RequireNonAlphanumeric = false;
        options.SignIn.RequireConfirmedAccount = false;
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
        options.Lockout.MaxFailedAccessAttempts = 5;
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>();

    // ── Blazor Server ─────────────────────────────────────────────────────
    builder.Services.AddRazorPages();
    builder.Services.AddServerSideBlazor();
    builder.Services.AddScoped<AuthenticationStateProvider, RevalidatingIdentityAuthenticationStateProvider<ApplicationUser>>();

    // ── Controllers para la API del Dashboard ─────────────────────────────
    builder.Services.AddControllers();

    // ── Authorization Policies ────────────────────────────────────────────
    builder.Services.AddAuthorization(options =>
    {
        options.AddPolicy("Administrador", p => p.RequireRole("Administrador"));
        options.AddPolicy("Operaciones", p => p.RequireRole("Administrador", "Operaciones", "Direccion"));
        options.AddPolicy("Comercial", p => p.RequireRole("Administrador", "Operaciones", "Direccion", "Comercial"));
        options.AddPolicy("Instalador", p => p.RequireRole("Administrador", "Operaciones", "Instalador", "Partner"));
        options.AddPolicy("Dashboard", p => p.RequireRole("Administrador", "Operaciones", "Direccion"));
    });

    // ── HealthChecks ──────────────────────────────────────────────────────
    builder.Services.AddHealthChecks()
        .AddNpgSql(builder.Configuration.GetConnectionString("DefaultConnection") ?? "",
            name: "postgresql",
            tags: new[] { "ready", "db" });

    // ── Antiforgery ───────────────────────────────────────────────────────
    builder.Services.AddAntiforgery();

    var app = builder.Build();

    // ── Auto-migrate en startup (Railway pre-deploy hook) ─────────────────
    await app.MigrateAndSeedAsync();

    // ── Pipeline ──────────────────────────────────────────────────────────
    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Error");
        app.UseHsts();
    }

    app.UseSerilogRequestLogging();
    app.UseStaticFiles();
    app.UseRouting();
    app.UseAuthentication();
    app.UseAuthorization();

    // ── HealthChecks endpoints (Railway) ──────────────────────────────────
    app.MapHealthChecks("/health");
    app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
    {
        Predicate = hc => hc.Tags.Contains("ready")
    });

    // ── API controllers (Dashboard) ───────────────────────────────────────
    app.MapControllers();

    // ── Blazor ────────────────────────────────────────────────────────────
    app.MapBlazorHub();
    app.MapFallbackToPage("/_Host");

    app.Run();
}
catch (Exception ex) when (ex is not HostAbortedException)
{
    Log.Fatal(ex, "GesinflotOpsHub terminó inesperadamente");
    throw;
}
finally
{
    Log.CloseAndFlush();
}
