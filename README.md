# GesinflotOpsHub — Operations Hub

> **Sustituto del GSheet · Fuente de verdad operativa · Conectado a Odoo · Alimentando el dashboard**

Sistema interno de gestión operativa para Gesinflot: conecta Odoo ERP, la operativa técnica de flota y el dashboard de gestión, gestionando el flujo completo desde presupuesto hasta facturación.

---

## Arquitectura del sistema

```
┌─────────────────────────────────────────────────────────────────────┐
│                        GESINFLOT OPS HUB                            │
│                                                                     │
│  ┌──────────────┐   ┌──────────────┐   ┌────────────────────────┐  │
│  │   Odoo ERP   │◄──┤  IOdooService│   │  Dashboard Externo     │  │
│  │  (maestro)   │──►│  JSON-RPC    │   │  /api/intervenciones   │  │
│  └──────────────┘   └──────────────┘   │  /api/kpis/operaciones │  │
│                                        └────────────────────────┘  │
│  ┌─────────────────────────────────────────────────────────────┐    │
│  │                    Blazor Server (Web)                       │    │
│  │  Dashboard │ Expedientes │ Intervenciones │ Planificación    │    │
│  │  Checklist │ Usuarios    │ Odoo Sync      │ Portal Cliente   │    │
│  └─────────────────────────────────────────────────────────────┘    │
│  ┌─────────────────────────────────────────────────────────────┐    │
│  │                    Application Layer                         │    │
│  │  ExpedienteService │ IntervencionService │ Validators        │    │
│  └─────────────────────────────────────────────────────────────┘    │
│  ┌─────────────────────────────────────────────────────────────┐    │
│  │                    Domain Layer                              │    │
│  │  ExpedienteInstalacion │ Intervencion │ ChecklistTecnica     │    │
│  │  PlanificacionInstalacion │ Cliente │ Instalador             │    │
│  └─────────────────────────────────────────────────────────────┘    │
│  ┌─────────────────────────────────────────────────────────────┐    │
│  │                   Infrastructure Layer                       │    │
│  │  AppDbContext (EF Core + PostgreSQL) │ Repositories          │    │
│  │  OdooService │ EmailService │ DashboardService               │    │
│  └─────────────────────────────────────────────────────────────┘    │
└─────────────────────────────────────────────────────────────────────┘
```

---

## Flujo operativo completo

```
[Odoo] Presupuesto enviado
         │
         ▼
[Portal Cliente] Acepta presupuesto ──► Email confirmación
         │
         ▼
[Ops] Crear checklist técnica dinámica
  ├── Nueva instalación: checklist completa
  └── Cliente existente: solo cambios (diferencial)
         │
         ▼
[Ops] Planificación
  ├── Asignar instalador (interno / partner)
  ├── Elegir fecha
  └── Confirmar con cliente ──► Email confirmación
         │
         ▼  [REGLA: sin checklist + instalador + confirmación → NO se puede avanzar]
         ▼
[Instalador] Ejecutar intervención
  └── Resultado técnico + facturable + importe + coste
         │
         ▼
[Ops] Marcar como realizado
  └── Sync automático → Odoo (estado + facturable + importe)
         │
         ▼
[Facturación] Control en Odoo
         │
         ▼
[Dashboard] Consume /api/intervenciones y /api/kpis/operaciones
```

---

## Entidades principales

```
ExpedienteInstalacion ──────────────────────────────────────
  Id, CodigoExpediente (EXP-2025-00001)
  ClienteId ──► Cliente (synced desde Odoo res.partner)
  OdooBudgetId, OdooSaleOrderId
  TipoExpediente: NuevaInstalacion | Ampliacion | Renovacion | ...
  EstadoExpediente: Borrador → PresupuestoEnviado → ... → Facturado
  TokenPortalCliente (acceso sin login, 30 días)

ChecklistTecnica ─── (1..N) ──► UnidadChecklist
  EsDiferencial (false=nuevo, true=cliente existente)
  Unidades: tipo, marca, FMS, sensor puerta, máq.frío, termógrafo, EBS

PlanificacionInstalacion
  InstaladorId, FechaPlanificada, ConfirmadoConCliente
  Dirección, Contacto, Teléfono

Intervencion ─── CORE del Dashboard ───────────────────────
  Codigo (INT-20250409-A3F9B1)
  Tipo: Instalacion | Ampliacion | ... | Auditoria
  Estado: Planificada → Confirmada → Realizada
  EsFacturable, Importe, Coste, Margen (calculado)
  SincronizadoOdoo
```

---

## Stack tecnológico

| Capa | Tecnología |
|------|-----------|
| Backend | ASP.NET Core 8 |
| Frontend | Blazor Server |
| Base de datos | PostgreSQL 16 |
| ORM | Entity Framework Core 8 |
| Logging | Serilog (JSON estructurado → stdout) |
| Auth | ASP.NET Core Identity + Roles |
| Arquitectura | Clean Architecture (Domain / Application / Infrastructure / Web) |
| Deploy | Railway (Dockerfile multistage) |

---

## Roles y permisos

| Rol | Permisos |
|-----|---------|
| Administrador | Todo: usuarios, configuración, todos los expedientes |
| Dirección | Lectura total, KPIs, dashboard |
| Comercial | Solo sus clientes y expedientes (por OdooComercialId) |
| Operaciones | Edición operativa completa |
| Soporte | Lectura + notas |
| Instalador | Solo sus intervenciones asignadas |
| Partner | Solo su scope de intervenciones |
| SoloLectura | Lectura sin edición |

---

## API para Dashboard externo

```
GET  /api/intervenciones                     → Lista todas las intervenciones
GET  /api/intervenciones/{id}                → Detalle
GET  /api/intervenciones/cliente/{clienteId} → Por cliente
GET  /api/kpis/operaciones?año=2025&mes=4    → KPIs operativos
GET  /api/odoo/clientes                      → Clientes desde Odoo
GET  /api/odoo/presupuestos                  → Presupuestos desde Odoo
POST /api/odoo/sync                          → Sincroniza clientes
```

---

## Healthchecks Railway

```
GET /health        → Liveness (siempre disponible)
GET /health/ready  → Readiness (incluye check PostgreSQL)
```

---

## Estructura del proyecto

```
GesinflotOpsHub.sln
├── src/
│   ├── GesinflotOpsHub.Domain/
│   │   ├── Common/         BaseEntity, AuditableEntity
│   │   ├── Entities/       ExpedienteInstalacion, Intervencion, Checklist...
│   │   ├── Enums/          TipoExpediente, EstadoExpediente, TipoIntervencion...
│   │   └── Interfaces/     IExpedienteRepository, IIntervencionRepository...
│   │
│   ├── GesinflotOpsHub.Application/
│   │   ├── Common/Interfaces/  IOdooService, IEmailService, IDashboardService
│   │   ├── DTOs/               ExpedienteListDto, IntervencionDetailDto, KpiDto...
│   │   ├── Services/           ExpedienteService, IntervencionService
│   │   └── DependencyInjection.cs
│   │
│   ├── GesinflotOpsHub.Infrastructure/
│   │   ├── Persistence/    AppDbContext (EF Core + Identity + PostgreSQL)
│   │   ├── Repositories/   Implementaciones de IRepository
│   │   ├── Services/       OdooService (JSON-RPC), EmailService, DashboardService
│   │   └── DependencyInjection.cs
│   │
│   └── GesinflotOpsHub.Web/
│       ├── Api/            DashboardControllers, OdooController
│       ├── Components/     KpiCard, KanbanColumna, EstadoBadge
│       ├── Pages/
│       │   ├── Expedientes/    Lista + Detalle
│       │   ├── Intervenciones/ Lista
│       │   ├── Planificacion/  Vista Kanban
│       │   ├── Usuarios/       CRUD usuarios
│       │   └── Portal/         Portal cliente (acceso por token)
│       ├── Shared/         MainLayout, NavMenu, LoginDisplay
│       ├── Program.cs      Bootstrap (Puerto dinámico, Serilog, migración auto)
│       ├── DatabaseInitializer.cs  Seed roles + admin
│       └── wwwroot/css/    app.css, portal.css
│
├── Dockerfile              Multistage .NET 8 para Railway
├── docker-compose.yml      Desarrollo local con PostgreSQL + MailHog
├── railway.json            Configuración Railway
├── .env.example            Plantilla de variables (sin secretos)
└── .gitignore
```

---

## Inicio rápido

### Desarrollo local

```bash
# 1. Clonar y arrancar infraestructura
docker-compose up -d postgres mailhog

# 2. Configurar secretos (user-secrets, NO en appsettings)
cd src/GesinflotOpsHub.Web
dotnet user-secrets set "Odoo:ApiKey" "TU_API_KEY"
dotnet user-secrets set "Email:Password" "TU_PASSWORD"

# 3. Aplicar migraciones
dotnet ef database update

# 4. Ejecutar
dotnet run
# → http://localhost:8080
# → MailHog UI: http://localhost:8025
```

### Generar primera migración

```bash
cd src/GesinflotOpsHub.Infrastructure
dotnet ef migrations add InitialCreate \
  --startup-project ../GesinflotOpsHub.Web \
  --output-dir Persistence/Migrations
```

### Despliegue Railway

```bash
# 1. Conectar repo a Railway
# 2. Añadir servicio PostgreSQL (Railway Plugin)
# 3. Configurar variables en Railway →  Variables:

ASPNETCORE_ENVIRONMENT=Production
ConnectionStrings__DefaultConnection=${{Postgres.DATABASE_URL}}
Odoo__BaseUrl=https://tu-instancia.odoo.com
Odoo__Database=nombre-bd
Odoo__Username=usuario@empresa.com
Odoo__ApiKey=<api-key-real>
Email__Host=smtp.tu-proveedor.com
Email__Port=587
Email__User=noreply@gesinflot.com
Email__Password=<password-real>
Email__From=Gesinflot Ops <noreply@gesinflot.com>
App__BaseUrl=https://tu-app.railway.app
App__AdminEmail=admin@gesinflot.com
App__AdminPassword=<contraseña-segura>

# 4. Push → Railway detecta Dockerfile y despliega automáticamente
git push origin main
```

---

## Riesgos técnicos y mitigaciones

| Riesgo | Impacto | Mitigación |
|--------|---------|-----------|
| API Odoo caída | Alto | Resiliencia con retry + fallback + logs |
| Migración falla en startup | Crítico | Captura de excepción en `MigrateAsync`, logs claros |
| Token portal cliente robado | Alto | Expiración 30 días, HTTPS obligatorio, invalidación explícita |
| Fuga de API key Odoo | Crítico | Solo via env var Railway, nunca en código/repo |
| Conexión Blazor Server caída | Medio | Circuit breaker, reconexión automática Blazor |
| Carga alta / memory | Medio | Stateless + Railway autoscaling + Blazor connection limits |
| Deuda de sincronización Odoo | Medio | Cola de pendientes `SincronizadoOdoo=false` + job periódico |

---

## Mejoras futuras (Fase 6)

- [ ] Worker Service para sincronización Odoo periódica (background job)
- [ ] SignalR para notificaciones en tiempo real al equipo operativo
- [ ] Firma digital de presupuestos en portal cliente
- [ ] Exportación a PDF (expediente, checklist, informe intervención)
- [ ] App móvil PWA para instaladores (offline-capable)
- [ ] Power BI Embedded en dashboard
- [ ] Webhook desde Odoo al aprobar pedido
- [ ] Multi-tenant (si escala a otras empresas)
- [ ] OpenTelemetry + distributed tracing

---

## Caso real completo — Nueva instalación

```
1. Comercial crea presupuesto en Odoo (sale.order, estado=sent)
2. [Odoo Sync] → App importa presupuesto, crea ExpedienteInstalacion (Borrador)
3. Ops → Estado: PresupuestoEnviado → Portal cliente genera token
4. Email automático al cliente con enlace portal
5. Cliente en portal: revisa presupuesto, pulsa "Acepto"
6. → Estado: PresupuestoAceptado | Odoo actualizado | FechaAceptacion guardada
7. Ops → Completa ChecklistTecnica (5 camiones + FMS Renault + Carrier)
8. → Estado: ChecklistCompletada
9. Ops → Crea PlanificacionInstalacion (instalador Juan, 15/05/2025, Zaragoza)
10. Confirmación con cliente → Email enviado | ConfirmadoConCliente=true
11. → Estado: PlanificacionConfirmada
12. Instalador ejecuta → Ops: MarcarRealizada (facturable=true, importe=3.200€, coste=800€)
13. → Estado: Ejecutado | Sync Odoo: x_intervencion_realizada=true | Margen=2.400€ (75%)
14. Facturación en Odoo → Ops: Estado=Facturado
15. Dashboard: /api/kpis/operaciones → facturación acumulada actualizada
```

---

*Gesinflot Operations Hub v1.0 — Arquitectura diseñada para Railway + .NET 8 + PostgreSQL*
