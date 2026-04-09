using GesinflotOpsHub.Domain.Entities;
using GesinflotOpsHub.Domain.Interfaces;
using GesinflotOpsHub.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GesinflotOpsHub.Infrastructure.Repositories;

public class ExpedienteRepository : IExpedienteRepository
{
    private readonly AppDbContext _db;
    public ExpedienteRepository(AppDbContext db) => _db = db;

    public async Task<ExpedienteInstalacion?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _db.Expedientes
            .Include(e => e.Cliente)
            .Include(e => e.Checklists).ThenInclude(c => c.Unidades)
            .Include(e => e.Planificaciones).ThenInclude(p => p.Instalador)
            .Include(e => e.Intervenciones)
            .FirstOrDefaultAsync(e => e.Id == id, ct);

    public async Task<ExpedienteInstalacion?> GetByCodigoAsync(string codigo, CancellationToken ct = default)
        => await _db.Expedientes
            .Include(e => e.Cliente)
            .FirstOrDefaultAsync(e => e.CodigoExpediente == codigo, ct);

    public async Task<ExpedienteInstalacion?> GetByTokenPortalAsync(string token, CancellationToken ct = default)
        => await _db.Expedientes
            .Include(e => e.Cliente)
            .Include(e => e.Checklists).ThenInclude(c => c.Unidades)
            .FirstOrDefaultAsync(e => e.TokenPortalCliente == token, ct);

    public async Task<IEnumerable<ExpedienteInstalacion>> GetAllAsync(CancellationToken ct = default)
        => await _db.Expedientes
            .Include(e => e.Cliente)
            .OrderByDescending(e => e.FechaCreacion)
            .ToListAsync(ct);

    public async Task<IEnumerable<ExpedienteInstalacion>> GetByClienteAsync(Guid clienteId, CancellationToken ct = default)
        => await _db.Expedientes
            .Include(e => e.Cliente)
            .Where(e => e.ClienteId == clienteId)
            .OrderByDescending(e => e.FechaCreacion)
            .ToListAsync(ct);

    public async Task<IEnumerable<ExpedienteInstalacion>> GetByComercialAsync(string comercialId, CancellationToken ct = default)
        => await _db.Expedientes
            .Include(e => e.Cliente)
            .Where(e => e.ComercialResponsable == comercialId || e.OdooComercialId == comercialId)
            .OrderByDescending(e => e.FechaCreacion)
            .ToListAsync(ct);

    public async Task AddAsync(ExpedienteInstalacion expediente, CancellationToken ct = default)
        => await _db.Expedientes.AddAsync(expediente, ct);

    public void Update(ExpedienteInstalacion expediente)
        => _db.Expedientes.Update(expediente);

    public void Delete(ExpedienteInstalacion expediente)
        => _db.Expedientes.Remove(expediente);

    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
        => await _db.SaveChangesAsync(ct);
}

public class ClienteRepository : IClienteRepository
{
    private readonly AppDbContext _db;
    public ClienteRepository(AppDbContext db) => _db = db;

    public async Task<Cliente?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _db.Clientes.FindAsync(new object[] { id }, ct);

    public async Task<Cliente?> GetByOdooIdAsync(string odooId, CancellationToken ct = default)
        => await _db.Clientes.FirstOrDefaultAsync(c => c.OdooPartnerId == odooId, ct);

    public async Task<IEnumerable<Cliente>> GetAllAsync(CancellationToken ct = default)
        => await _db.Clientes.Where(c => c.Activo).OrderBy(c => c.RazonSocial).ToListAsync(ct);

    public async Task AddAsync(Cliente cliente, CancellationToken ct = default)
        => await _db.Clientes.AddAsync(cliente, ct);

    public void Update(Cliente cliente) => _db.Clientes.Update(cliente);

    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
        => await _db.SaveChangesAsync(ct);
}

public class IntervencionRepository : IIntervencionRepository
{
    private readonly AppDbContext _db;
    public IntervencionRepository(AppDbContext db) => _db = db;

    public async Task<Intervencion?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _db.Intervenciones
            .Include(i => i.Cliente)
            .Include(i => i.Instalador)
            .Include(i => i.Expediente)
            .FirstOrDefaultAsync(i => i.Id == id, ct);

    public async Task<IEnumerable<Intervencion>> GetAllAsync(CancellationToken ct = default)
        => await _db.Intervenciones
            .Include(i => i.Cliente)
            .Include(i => i.Instalador)
            .Include(i => i.Expediente)
            .OrderByDescending(i => i.FechaPlanificada)
            .ToListAsync(ct);

    public async Task<IEnumerable<Intervencion>> GetByInstaladorAsync(Guid instaladorId, CancellationToken ct = default)
        => await _db.Intervenciones
            .Include(i => i.Cliente)
            .Include(i => i.Expediente)
            .Where(i => i.InstaladorId == instaladorId)
            .OrderByDescending(i => i.FechaPlanificada)
            .ToListAsync(ct);

    public async Task<IEnumerable<Intervencion>> GetByClienteAsync(Guid clienteId, CancellationToken ct = default)
        => await _db.Intervenciones
            .Include(i => i.Instalador)
            .Include(i => i.Expediente)
            .Where(i => i.ClienteId == clienteId)
            .OrderByDescending(i => i.FechaPlanificada)
            .ToListAsync(ct);

    public async Task<IEnumerable<Intervencion>> GetPendienteSincronizacionOdooAsync(CancellationToken ct = default)
        => await _db.Intervenciones
            .Where(i => !i.SincronizadoOdoo && i.Estado == Domain.Enums.EstadoIntervencion.Realizada)
            .ToListAsync(ct);

    public async Task AddAsync(Intervencion intervencion, CancellationToken ct = default)
        => await _db.Intervenciones.AddAsync(intervencion, ct);

    public void Update(Intervencion intervencion) => _db.Intervenciones.Update(intervencion);

    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
        => await _db.SaveChangesAsync(ct);
}

public class InstaladorRepository : IInstaladorRepository
{
    private readonly AppDbContext _db;
    public InstaladorRepository(AppDbContext db) => _db = db;

    public async Task<Instalador?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _db.Instaladores.FindAsync(new object[] { id }, ct);

    public async Task<IEnumerable<Instalador>> GetActivosAsync(CancellationToken ct = default)
        => await _db.Instaladores.Where(i => i.Activo).OrderBy(i => i.Nombre).ToListAsync(ct);

    public async Task AddAsync(Instalador instalador, CancellationToken ct = default)
        => await _db.Instaladores.AddAsync(instalador, ct);

    public void Update(Instalador instalador) => _db.Instaladores.Update(instalador);

    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
        => await _db.SaveChangesAsync(ct);
}
