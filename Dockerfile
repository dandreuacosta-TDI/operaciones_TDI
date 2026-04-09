# ═══════════════════════════════════════════════════════════════════════════════
# GesinflotOpsHub — Multistage Dockerfile para Railway
# ═══════════════════════════════════════════════════════════════════════════════

# ── Stage 1: Build ─────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiar archivos de proyecto primero (cache de NuGet)
COPY ["src/GesinflotOpsHub.Domain/GesinflotOpsHub.Domain.csproj",          "src/GesinflotOpsHub.Domain/"]
COPY ["src/GesinflotOpsHub.Application/GesinflotOpsHub.Application.csproj","src/GesinflotOpsHub.Application/"]
COPY ["src/GesinflotOpsHub.Infrastructure/GesinflotOpsHub.Infrastructure.csproj","src/GesinflotOpsHub.Infrastructure/"]
COPY ["src/GesinflotOpsHub.Web/GesinflotOpsHub.Web.csproj",                "src/GesinflotOpsHub.Web/"]

# Restore paquetes
RUN dotnet restore "src/GesinflotOpsHub.Web/GesinflotOpsHub.Web.csproj"

# Copiar el resto del código
COPY . .

# Publicar en modo Release
RUN dotnet publish "src/GesinflotOpsHub.Web/GesinflotOpsHub.Web.csproj" \
    -c Release \
    -o /app/publish \
    --no-restore \
    /p:UseAppHost=false

# ── Stage 2: Runtime ────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Instalar curl para healthchecks (opcional, Railway usa HTTP probes)
RUN apt-get update && apt-get install -y --no-install-recommends curl && rm -rf /var/lib/apt/lists/*

# Usuario sin privilegios (seguridad)
RUN adduser --disabled-password --gecos "" appuser && chown -R appuser /app
USER appuser

# Copiar artefactos publicados
COPY --from=build --chown=appuser /app/publish .

# Railway inyecta PORT dinámicamente — la app lo consume desde Program.cs
ENV ASPNETCORE_URLS=http://0.0.0.0:${PORT:-8080}
ENV ASPNETCORE_ENVIRONMENT=Production
ENV DOTNET_RUNNING_IN_CONTAINER=true

# Healthcheck interno (Railway usa /health)
HEALTHCHECK --interval=30s --timeout=10s --start-period=60s --retries=3 \
    CMD curl -f http://localhost:${PORT:-8080}/health || exit 1

EXPOSE 8080

ENTRYPOINT ["dotnet", "GesinflotOpsHub.Web.dll"]
