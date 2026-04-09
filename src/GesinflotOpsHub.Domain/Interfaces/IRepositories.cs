using GesinflotOpsHub.Domain.Entities;

namespace GesinflotOpsHub.Domain.Interfaces;

public interface IExpedienteRepository
{
    Task<ExpedienteInstalacion?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<ExpedienteInstalacion?> GetByCodigoAsync(string codigo, CancellationToken ct = default);
    Task<ExpedienteInstalacion?> GetByTokenPortalAsync(string token, CancellationToken ct = default);
    Task<IEnumerable<ExpedienteInstalacion>> GetAllAsync(CancellationToken ct = default);
    Task<IEnumerable<ExpedienteInstalacion>> GetByClienteAsync(Guid clienteId, CancellationToken ct = default);
    Task<IEnumerable<ExpedienteInstalacion>> GetByComercialAsync(string comercialId, CancellationToken ct = default);
    Task AddAsync(ExpedienteInstalacion expediente, CancellationToken ct = default);
    void Update(ExpedienteInstalacion expediente);
    void Delete(ExpedienteInstalacion expediente);
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}

public interface IClienteRepository
{
    Task<Cliente?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Cliente?> GetByOdooIdAsync(string odooId, CancellationToken ct = default);
    Task<IEnumerable<Cliente>> GetAllAsync(CancellationToken ct = default);
    Task AddAsync(Cliente cliente, CancellationToken ct = default);
    void Update(Cliente cliente);
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}

public interface IIntervencionRepository
{
    Task<Intervencion?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IEnumerable<Intervencion>> GetAllAsync(CancellationToken ct = default);
    Task<IEnumerable<Intervencion>> GetByInstaladorAsync(Guid instaladorId, CancellationToken ct = default);
    Task<IEnumerable<Intervencion>> GetByClienteAsync(Guid clienteId, CancellationToken ct = default);
    Task<IEnumerable<Intervencion>> GetPendienteSincronizacionOdooAsync(CancellationToken ct = default);
    Task AddAsync(Intervencion intervencion, CancellationToken ct = default);
    void Update(Intervencion intervencion);
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}

public interface IInstaladorRepository
{
    Task<Instalador?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IEnumerable<Instalador>> GetActivosAsync(CancellationToken ct = default);
    Task AddAsync(Instalador instalador, CancellationToken ct = default);
    void Update(Instalador instalador);
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
