using GesinflotOpsHub.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace GesinflotOpsHub.Web;

public static class DatabaseInitializer
{
    public static async Task MigrateAndSeedAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<AppDbContext>>();

        try
        {
            logger.LogInformation("Ejecutando migraciones EF Core...");
            await db.Database.MigrateAsync();
            logger.LogInformation("Migraciones completadas.");

            await SeedRolesAsync(roleManager);
            await SeedAdminUserAsync(userManager, app.Configuration);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error durante la inicialización de base de datos");
            throw;
        }
    }

    private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
    {
        var roles = new[]
        {
            "Administrador", "Direccion", "Comercial",
            "Operaciones", "Soporte", "Instalador", "Partner", "SoloLectura"
        };

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }
    }

    private static async Task SeedAdminUserAsync(UserManager<ApplicationUser> userManager, IConfiguration config)
    {
        var adminEmail = config["App:AdminEmail"] ?? "admin@gesinflot.com";
        var adminPass = config["App:AdminPassword"] ?? "Admin@Gesinflot2024!";

        var existing = await userManager.FindByEmailAsync(adminEmail);
        if (existing is not null) return;

        var admin = new ApplicationUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            NombreCompleto = "Administrador Sistema",
            Rol = "Administrador",
            Activo = true,
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(admin, adminPass);
        if (result.Succeeded)
            await userManager.AddToRoleAsync(admin, "Administrador");
    }
}
