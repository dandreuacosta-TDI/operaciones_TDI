namespace GesinflotOpsHub.Domain.Enums;

public enum TipoExpediente
{
    NuevaInstalacion = 1,
    Ampliacion = 2,
    Renovacion = 3,
    Sustitucion = 4,
    Reinstalacion = 5,
    Auditoria = 6
}

public enum EstadoExpediente
{
    Borrador = 1,
    PresupuestoEnviado = 2,
    PresupuestoAceptado = 3,
    ChecklistPendiente = 4,
    ChecklistCompletada = 5,
    PlanificacionPendiente = 6,
    PlanificacionConfirmada = 7,
    EnEjecucion = 8,
    Ejecutado = 9,
    PendienteFacturacion = 10,
    Facturado = 11,
    Cancelado = 12,
    EnEspera = 13
}

public enum TipoIntervencion
{
    Instalacion = 1,
    Ampliacion = 2,
    Renovacion = 3,
    Sustitucion = 4,
    Mantenimiento = 5,
    Incidencia = 6,
    Auditoria = 7
}

public enum EstadoIntervencion
{
    Planificada = 1,
    Confirmada = 2,
    EnCurso = 3,
    Realizada = 4,
    Cancelada = 5,
    Reprogramada = 6
}

public enum TipoInstalador
{
    Interno = 1,
    Partner = 2
}

public enum TipoAccionUnidad
{
    Alta = 1,
    Baja = 2,
    Modificacion = 3,
    Sustitucion = 4,
    SinCambios = 5
}

public enum TipoUnidad
{
    CamionTractora = 1,
    CamionRigido = 2,
    Semirremolque = 3,
    FurgonRefrigerado = 4,
    Otro = 5
}

public enum MarcaFMS
{
    Renault = 1,
    Volvo = 2,
    MAN = 3,
    Scania = 4,
    DAF = 5,
    MercedesBenz = 6,
    DRT = 7,
    Otro = 8
}

public enum MarcaMaquinaFrio
{
    Carrier = 1,
    ThermoKing = 2,
    Otro = 3
}

public enum OEMMaquinaFrio
{
    Carrier = 1,
    ThermoKing = 2,
    SchmitzCargobull = 3
}

public enum MarcaTermografo
{
    BBox = 1,
    Transcan = 2,
    Datacold = 3,
    Apache = 4,
    AGS = 5,
    Gesinflot = 6,
    Otro = 7
}

public enum MarcaEBS
{
    Wabco = 1,
    Knorr = 2,
    Haldex = 3,
    Otro = 4
}

public enum RolUsuario
{
    Administrador = 1,
    Direccion = 2,
    Comercial = 3,
    Operaciones = 4,
    Soporte = 5,
    Instalador = 6,
    Partner = 7,
    SoloLectura = 8
}

public enum OrigenIntervencion
{
    Nuevo = 1,
    ClienteExistente = 2,
    Reclamacion = 3,
    Garantia = 4
}
