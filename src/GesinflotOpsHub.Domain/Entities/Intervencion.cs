using GesinflotOpsHub.Domain.Common;
using GesinflotOpsHub.Domain.Enums;

namespace GesinflotOpsHub.Domain.Entities;

/// <summary>
/// Intervención ejecutada. Es la entidad core para el dashboard de gestión.
/// </summary>
public class Intervencion : AuditableEntity
{
    public string Codigo { get; set; } = string.Empty;

    public Guid ExpedienteId { get; set; }
    public ExpedienteInstalacion Expediente { get; set; } = null!;

    public Guid ClienteId { get; set; }
    public Cliente Cliente { get; set; } = null!;

    public Guid? InstaladorId { get; set; }
    public Instalador? Instalador { get; set; }

    // Clasificación
    public TipoIntervencion Tipo { get; set; }
    public EstadoIntervencion Estado { get; set; } = EstadoIntervencion.Planificada;
    public OrigenIntervencion Origen { get; set; } = OrigenIntervencion.Nuevo;

    // Temporalidad
    public DateTime FechaPlanificada { get; set; }
    public DateTime? FechaEjecucion { get; set; }
    public int? DuracionRealMinutos { get; set; }

    // Localización
    public string? DireccionEjecucion { get; set; }
    public string? Poblacion { get; set; }
    public string? Provincia { get; set; }
    public double? Latitud { get; set; }
    public double? Longitud { get; set; }

    // Economía
    public bool EsFacturable { get; set; } = true;
    public decimal? Importe { get; set; }
    public decimal? Coste { get; set; }
    public decimal? Margen => (Importe.HasValue && Coste.HasValue) ? Importe.Value - Coste.Value : null;
    public decimal? MargenPorcentaje =>
        (Importe.HasValue && Importe.Value > 0 && Coste.HasValue)
            ? Math.Round(((Importe.Value - Coste.Value) / Importe.Value) * 100, 2)
            : null;

    // Vínculo Odoo
    public string? OdooIntervencionId { get; set; }
    public bool SincronizadoOdoo { get; set; } = false;
    public DateTime? FechaSincronizacionOdoo { get; set; }

    // Resultado
    public string? Descripcion { get; set; }
    public string? ResultadoTecnico { get; set; }
    public string? Incidencias { get; set; }
    public bool RequiereSeguimiento { get; set; } = false;

    // Unidades intervenidas
    public ICollection<UnidadChecklist> UnidadesIntervenidas { get; set; } = new List<UnidadChecklist>();
}
