# Fase 02 — Modelo de datos, EF Core y semilla

## Objetivo

Definir las 4 entidades (ni una más), el `DbContext`, aplicar migraciones automáticamente al arrancar y sembrar la base con los datos del enunciado usando `SEED_FECHA_BASE`.

## Alcance

- Entidades: `Tenant`, `Usuario`, `Categoria`, `Solicitud` (ver [rules/datos-semilla.md](../rules/datos-semilla.md)).
- `DbContext` en Infraestructura con las configuraciones de EF Core (índices, longitud, tipos, relaciones).
- Migración inicial + aplicación automática en `Program.cs`.
- Seeder que solo actúa si la base está vacía.

## Pasos

1. **Entidades en `Dominio/`** — con NRT habilitado y reglas del modelo:
   - `Tenant`: `id`, `nombre`, `activo`
   - `Usuario`: `id`, `tenantId`, `email` (único global), `passwordHash`, `nombre`, `rol` (enum `Admin|Agente|Solicitante`), `activo`
   - `Categoria`: `id`, `tenantId`, `nombre`, `slaHoras`, `activo`
   - `Solicitud`: todos los campos del enunciado, `agenteId` nullable, enums `Prioridad` y `EstadoSolicitud`
2. **Paquete EF Core** — en Infraestructura: `Microsoft.EntityFrameworkCore.Sqlite` (+ `Design` si se usan migraciones por dotnet ef).
3. **`DbContext`** — configurar:
   - Índice único en `Usuario.email`.
   - Índice por `tenantId` en cada tabla que lo tenga.
   - Longitudes máximas de `titulo` (120) y `descripcion` (4000).
   - `DateTime` en UTC (configurar `DateTimeKind.Utc` o convertirlas al leer/escribir).
4. **Migración inicial** — generar con `dotnet ef migrations add Inicial` (o agregar al `Program.cs` el `MigrateAsync()` si prefieres aplicar sin herramienta CLI — se puede usar `context.Database.Migrate()`).
5. **Semilla** — en Infraestructura (o un seeder dedicado):
   - Solo si la base está vacía (sin tenants).
   - Fechas = desplazamientos fijos desde `SEED_FECHA_BASE` (default `2026-01-15T08:00:00Z`). **Nunca `DateTime.UtcNow`.**
   - 2 tenants (Cooperativa Norte, Bufete Sur), 7 usuarios con contraseña `Sitec.2026` hasheada con BCrypt.
   - 4 categorías en ambas organizaciones.
   - 25 solicitudes en Norte, 8 en Sur; en Norte al menos 5 vencidas y 3 resueltas, repartidas entre estados y prioridades.
   - `fechaLimiteSla` de cada solicitud calculado según RN-04.
   - Códigos `SOL-{año}-{correlativo}` por org (RN-07).

## Definición de terminado

- [ ] Al arrancar, la API crea la base y aplica migraciones sola.
- [ ] La base sembrada contiene 2 tenants, 7 usuarios, 8 categorías, 33 solicitudes.
- [ ] Las fechas de la semilla son estables: ejecutar dos veces (borrando la base) produce los mismos datos.
- [ ] Ninguna fecha se genera desde `DateTime.UtcNow`.
- [ ] Las contraseñas están hasheadas (nunca texto plano).
- [ ] `dotnet build` compila.

## Qué NO hacer en esta fase

- No agregar entidades extra (el enunciado solo permite con justificación).
- No sembrar con `Guid.NewGuid()` que cambie entre corridas si quieres datos 100% idénticos — se puede usar GUIDs fijos para poder referenciarlos, o resolver relaciones por nombre/email.
- No cambiar SQLite por otra base.

## Notas para el registro de decisiones

- Si decides GUIDs fijos en la semilla vs. resolver por email/nombre, regístralo (ambas son válidas).
- Cómo resuelves el correlativo RN-07 en la semilla.

## Commit sugerido

```
feat(seed): agrega modelo de datos, migración y datos semilla

Entidades Tenant, Usuario, Categoria y Solicitud con EF Core.
La base se migra y siembra automáticamente al arrancar usando
SEED_FECHA_BASE como referencia fija (RN-07, RN-04).
```
