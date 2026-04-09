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

    // ── Mapear DATABASE_URL de Railway al formato EF Core ─────────────────
    var connStr = builder.Configuration.GetConnectionString("DefaultConnection");
    if (string.IsNullOrEmpty(connStr))
    {
        var dbUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
        if (!string.IsNullOrEmpty(dbUrl))
        {
            try
            {
                var uri = new Uri(dbUrl);
                var parts = uri.UserInfo.Split(':', 2);
                connStr = $"Host={uri.Host};Port={uri.Port};Database={uri.AbsolutePath.TrimStart('/')};Username={Uri.UnescapeDataString(parts[0])};Password={Uri.UnescapeDataString(parts[1])};SSL Mode=Require;Trust Server Certificate=true";
                builder.Configuration["ConnectionStrings:DefaultConnection"] = connStr;
            }
            catch { /* URL mal formada, continuar sin BD */ }
        }
    }

    // ── Data Protection (ephemeral keys: safe for single-instance Railway) ──
    builder.Services.AddDataProtection()
        .SetApplicationName("GesinflotOpsHub")
        .PersistKeysToFileSystem(new System.IO.DirectoryInfo(
            Environment.GetEnvironmentVariable("DP_KEYS_DIR") ?? "/tmp/dp-keys"));

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
    var hcBuilder = builder.Services.AddHealthChecks();
    var hcConnStr = builder.Configuration.GetConnectionString("DefaultConnection");
    if (!string.IsNullOrEmpty(hcConnStr))
    {
        hcBuilder.AddNpgSql(hcConnStr, name: "postgresql", tags: new[] { "ready", "db" });
    }

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
    // /health → liveness (siempre 200, no comprueba BD)
    app.MapHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
    {
        Predicate = _ => false
    });
    // /health/ready → readiness (comprueba BD)
    app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
    {
        Predicate = hc => hc.Tags.Contains("ready")
    });

    // ── API controllers (Dashboard) ───────────────────────────────────────
    app.MapControllers();

    // ── Razour Pages (Identity UI) + Blazor ──────────────────────────────
    app.MapRazorPages();
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
