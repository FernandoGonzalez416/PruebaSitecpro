# Plan de fases — HelpDesk

Plan de trabajo de la prueba técnica, dividido en fases. Cada fase es un archivo `.md` independiente que se resuelve de una sola vez: tiene su objetivo, sus pasos, su criterio de terminado y su commit sugerido.

## Reglas generales

- Cada fase termina con un **commit** (Conventional Commits, ver [rules/conventional-commits.md](../rules/conventional-commits.md)). El mínimo requerido del enunciado son 8 commits; este plan tiene 10+.
- Antes de cerrar cualquier fase del backend: `dotnet test` en verde.
- Antes de cerrar cualquier fase del frontend: `tsc --noEmit` sin errores.
- Toda decisión no trivial que surja se registra en [rules/registro-decisiones.md](../rules/registro-decisiones.md) en el momento.
- No se mezcla backend y frontend en un mismo commit.
- Documentación fuente: [rules/reglas-negocio.md](../rules/reglas-negocio.md), [rules/contrato-api.md](../rules/contrato-api.md), [rules/frontend.md](../rules/frontend.md), [rules/datos-semilla.md](../rules/datos-semilla.md), [rules/testing.md](../rules/testing.md).

## Las fases

| # | Fase | Qué entrega | Salida verificable |
|---|---|---|---|
| 01 | [Scaffolding del proyecto](01-scaffolding.md) | Repo limpio, estructura de carpetas, proyectos .NET y Vue arrancan | `dotnet build` + Vite dev server ok |
| 02 | [Modelo de datos, EF Core y semilla](02-modelo-datos.md) | Entidades, DbContext, migración automática, seed | `/health` responde; datos visibles en Swagger |
| 03 | [Autenticación JWT, /me y /health](03-autenticacion.md) | Login, JWT, Swagger Bearer, CORS, middleware de errores | Login en Swagger devuelve token; `/me` funciona |
| 04 | [Dominio: estados, SLA y permisos + pruebas](04-dominio-pruebas.md) | RN-02/03/04 en Dominio/, 8+ pruebas xUnit | `dotnet test` verde |
| 05 | [Endpoints de solicitudes y categorías](05-endpoints-solicitudes.md) | Los 9 endpoints, RN-01/05/06/07, filtros server-side | Contrato completo probado contra Swagger |
| 06 | [Frontend base: cliente HTTP, store, router, login](06-frontend-base.md) | Scaffold Vue3+TS strict, api client, auth store, guard, login | Login funcional en `:5173` |
| 07 | [Frontend: listado de solicitudes](07-frontend-listado.md) | Tabla, filtros, paginación, data-testid | Listado con datos semilla |
| 08 | [Frontend: detalle + acciones + modal](08-frontend-detalle.md) | Detalle, botones por estado/rol, modal | Detalle funcional con transiciones |
| 09 | [Frontend: formulario crear/editar](09-frontend-formulario.md) | Formulario compartido con validación | Crear y editar funcionan |
| 10 | [Entrega: README, DECISIONES y limpieza](10-entrega.md) | README, DECISIONES.md, checklist final | Cumple sección 8 y 11 |

## Orden de trabajo recomendado

Este orden sigue la sección 9 del enunciado y minimiza el riesgo de quedarse sin tiempo:

1. Fases 01–05 (backend completo) — son la columna vertebral y lo que más se evalúa.
2. Fase 06 (login) — desbloquea todo el frontend.
3. Fases 07–09 (pantallas restantes) — si el tiempo aprieta, una pantalla completa vale más que tres a medias.
4. Fase 10 (entrega) — reservar tiempo real; no dejar para el final.

## Guarda al principio de cada sesión

```powershell
dotnet test          # backend verde
tsc --noEmit         # frontend verde (en frontend/)
```

> Nota: la fase 04 (dominio + pruebas) se puede adelantar después de la 02 si se quiere probar la lógica pura antes de tocar autenticación. Es la única fase "movible" sin romper dependencias de orden.
