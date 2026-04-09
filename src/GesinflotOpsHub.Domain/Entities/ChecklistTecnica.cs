using GesinflotOpsHub.Domain.Common;
using GesinflotOpsHub.Domain.Enums;

namespace GesinflotOpsHub.Domain.Entities;

/// <summary>
/// Checklist técnica dinámica asociada a un expediente.
/// Para nuevas instalaciones contiene datos completos.
/// Para clientes existentes es diferencial (solo cambios).
/// </summary>
public class ChecklistTecnica : AuditableEntity
{
    public Guid ExpedienteId { get; set; }
    public ExpedienteInstalacion Expediente { get; set; } = null!;

    public bool EsDiferencial { get; set; } = false; // true = cliente existente

    // Unidades del checklist
    public ICollection<UnidadChecklist> Unidades { get; set; } = new List<UnidadChecklist>();

    // Control
    public bool Completada { get; set; } = false;
    public DateTime? FechaCompletada { get; set; }
    public string? ValidadoPor { get; set; }
    public string? Observaciones { get; set; }
}

public class UnidadChecklist : AuditableEntity
{
    public Guid ChecklistId { get; set; }
    public ChecklistTecnica Checklist { get; set; } = null!;

    // Referencia a unidad existente (clientes existentes)
    public Guid? UnidadVehiculoId { get; set; }
    public UnidadVehiculo? UnidadVehiculo { get; set; }

    // Acción sobre la unidad
    public TipoAccionUnidad Accion { get; set; } = TipoAccionUnidad.Alta;

    // -- DATOS BASE --
    public TipoUnidad TipoUnidad { get; set; }
    public string? Marca { get; set; }
    public string? Modelo { get; set; }
    public string? Matricula { get; set; }
    public string? Bastidor { get; set; }
    public string? LugarInstalacion { get; set; }

    // -- FMS / OEM --
    public bool TieneFMS { get; set; } = false;
    public MarcaFMS? MarcaFMS { get; set; }
    public string? ModeloFMS { get; set; }
    public string? VersionFirmwareFMS { get; set; }

    // -- SENSORES PORTON --
    public bool TieneSensoresPuerta { get; set; } = false;
    public int? NumeroPuertas { get; set; }
    public string? TipoSensorPuerta { get; set; }

    // -- MÁQUINA FRÍO --
    public bool TieneMaquinaFrio { get; set; } = false;
    public MarcaMaquinaFrio? MarcaMaquinaFrio { get; set; }
    public OEMMaquinaFrio? OEMMaquinaFrio { get; set; }
    public string? ModeloMaquinaFrio { get; set; }
    public string? NumeroSerieMaquinaFrio { get; set; }

    // -- TERMÓGRAFO --
    public bool TieneTermografo { get; set; } = false;
    public MarcaTermografo? MarcaTermografo { get; set; }
    public string? ModeloTermografo { get; set; }
    public string? NumeroSerieTermografo { get; set; }
    public int? NumeraSondas { get; set; }

    // -- EBS --
    public bool TieneEBS { get; set; } = false;
    public MarcaEBS? MarcaEBS { get; set; }
    public string? ModeloEBS { get; set; }

    // -- EQUIPO A INSTALAR --
    public string? EquipoGesinflot { get; set; }
    public string? NumeroSerieEquipo { get; set; }
    public string? VersionFirmware { get; set; }
    public string? SimCard { get; set; }
    public string? Operador { get; set; }

    // -- OBSERVACIONES --
    public string? ObservacionesTecnicas { get; set; }
    public bool Validada { get; set; } = false;
}
