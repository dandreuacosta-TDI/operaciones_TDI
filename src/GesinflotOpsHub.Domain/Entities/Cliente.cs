using GesinflotOpsHub.Domain.Common;

namespace GesinflotOpsHub.Domain.Entities;

public class Cliente : AuditableEntity
{
    // Vínculo con Odoo
    public string? OdooPartnerId { get; set; }

    public string RazonSocial { get; set; } = string.Empty;
    public string? NombreComercial { get; set; }
    public string? CIF { get; set; }

    // Contacto principal
    public string? Email { get; set; }
    public string? Telefono { get; set; }
    public string? ContactoPrincipal { get; set; }

    // Dirección
    public string? DireccionFiscal { get; set; }
    public string? Poblacion { get; set; }
    public string? Provincia { get; set; }
    public string? CodigoPostal { get; set; }

    // Control
    public bool Activo { get; set; } = true;
    public string? Segmento { get; set; }
    public string? ComercialAsignado { get; set; }
    public string? Notas { get; set; }

    // Navegación
    public ICollection<ExpedienteInstalacion> Expedientes { get; set; } = new List<ExpedienteInstalacion>();
    public ICollection<UnidadVehiculo> UnidadesFlota { get; set; } = new List<UnidadVehiculo>();
}
