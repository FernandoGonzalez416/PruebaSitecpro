# Fase 05 — Endpoints de solicitudes y categorías

## Objetivo

Completar los 9 endpoints del contrato (falta el listado, CRUD de solicitudes y transiciones) respetando RN-01 (aislamiento), RN-05 (asignación), RN-06 (motivos) y RN-07 (código). **RN-01 se implementa aquí y queda bien desde el principio.**

## Alcance

- `GET /categorias` (solo activas, solo de la org del token).
- `GET /solicitudes` — paginado y filtrado server-side (estado, prioridad, categoriaId, agenteId, q, vencidas, page, pageSize, sort).
- `POST /solicitudes` — crear con RN-04 y RN-07.
- `GET /solicitudes/{id}` — detalle completo.
- `PUT /solicitudes/{id}` — editar con RN-03 + RN-04.
- `POST /solicitudes/{id}/transiciones` — acciones del flujo con RN-02/03/05/06.
- Todos con `application/problem+json` y `codigo` en errores.

## Pasos

1. **Helper de tenant (RN-01)** — en todos los handlers que toquen datos:
   - Extraer `tenantId` del token.
   - **Toda query** filtra `Where(x => x.tenantId == tenantId)`.
   - Recurso inexistente **o** de otra org → 404 `RECURSO_NO_ENCONTRADO` (nunca 403 para esto).
2. **`GET /categorias`** — categorías `activo == true` de la org.
3. **`GET /solicitudes`**:
   - Filtros exactos: `estado`, `prioridad`, `categoriaId`, `agenteId`.
   - `q` busca en `titulo`, `descripcion`, `codigo` sin distinguir mayúsculas (`Contains`).
   - `vencidas=true` aplica la definición de RN-04 (límite pasado y estado no final).
   - `page` default 1, `pageSize` default 20 máx 100 → fuera de rango = 400 `PARAMETRO_INVALIDO`.
   - `sort`: `fechaCreacion`, `-fechaCreacion` (default), `prioridad`, `-prioridad` (orden semántico, no alfabético), `codigo`.
   - Para el orden semántico de prioridad se puede ordenar por una columna calculada (por ejemplo, un `Select`/case con índice de prioridad).
   - Respuesta: `items`, `page`, `pageSize`, `total`, `totalPaginas`.
   - Formato de item: `categoria {id,nombre}`, `agente {id,nombre}` o `null`, `vencida`.
   - **Solicitante**: solo ve las que él creó (RN-03).
4. **`POST /solicitudes`**:
   - Validar `titulo` (5–120), `descripcion` (10–4000) → 422 `VALIDACION` con `errores`.
   - Categoría debe existir y ser de la org (si no → 404/422 según decidas y registres).
   - Ignorar `fechaLimiteSla` si el cliente lo envía (RN-04).
   - Calcular `fechaLimiteSla` en servidor (RN-04).
   - Generar `codigo` con RN-07: `SOL-{año}-{correlativo5}` por org y año (contar solicitudes de la org en el año actual + 1).
   - `solicitanteId` = usuario del token. `agenteId` = null. Estado inicial `Nueva`.
   - Responder 201 con `Location` y el objeto completo.
5. **`GET /solicitudes/{id}`** — objeto completo (incluye `descripcion`, `solicitante`, motivos, `fechaResolucion`). RN-01 → 404 si no es de la org; RN-03 → Solicitante solo propias.
6. **`PUT /solicitudes/{id}`**:
   - RN-03: Solicitante solo propias y solo en estado `Nueva`; Admin/Agente pueden editar (dentro de su org).
   - RN-04: si cambia `prioridad` o `categoriaId` y la solicitud no está resuelta/cerrada/cancelada → recalcular SLA sin tocar `fechaCreacion`.
7. **`POST /solicitudes/{id}/transiciones`**:
   - Acciones: `asignar`, `iniciar`, `resolver`, `cerrar`, `reabrir`, `cancelar`.
   - Validar RN-02 (estado) y RN-03 (rol) → 409 `TRANSICION_INVALIDA` / 403 `OPERACION_NO_PERMITIDA`.
   - `asignar` → RN-05: agente existe, activo, misma org, rol Agente/Admin → 422 `AGENTE_INVALIDO`. Reasignar en `Asignada`/`EnProceso` cambia `agenteId`.
   - `resolver` → motivo ≥ 20 → 422 `MOTIVO_REQUERIDO`; guarda `motivoResolucion` + `fechaResolucion`.
   - `cancelar` → motivo ≥ 10 → 422 `MOTIVO_REQUERIDO`; guarda `motivoCancelacion`.
   - `iniciar`, `cerrar`, `reabrir` sin payload.
   - Responder 200 con la solicitud actualizada.

## Definición de terminado

- [ ] Los 9 endpoints del contrato responden correctamente.
- [ ] Login con `user1@sur.test` e intentar abrir una solicitud de Norte → **404** (checklist sección 11).
- [ ] `pageSize > 100` o `page < 1` → 400 `PARAMETRO_INVALIDO`.
- [ ] Filtrar/ordenar/paginar es server-side (el cliente no filtra).
- [ ] `fechaLimiteSla` enviado por el cliente se ignora.
- [ ] Todas las respuestas de error son `problem+json` con `codigo`.
- [ ] `dotnet test` sigue en verde.
- [ ] Probado contra Swagger (base `http://localhost:5080/api/v1`).

## Qué NO hacer en esta fase

- No inventar endpoints ni campos fuera del contrato.
- No devolver 403 para recursos de otra organización (siempre 404).
- No filtrar en memoria luego de traer todo: filtrar en la query de EF Core.

## Notas para el registro de decisiones

- Qué pasa si el cliente envía una `categoriaId` inexistente o de otra org (404 vs 422) — el enunciado no lo dice, hay que decidir y justificar.
- Cómo garantizaste el orden semántico de prioridad.
- Cómo implementaste RN-07 (conteo de filas del año vs otra técnica) y su límite ante concurrencia (el enunciado lo deja fuera de alcance).

## Commits sugeridos

```
feat(categorias): agrega GET /categorias filtrado por tenant

feat(solicitudes): agrega listado paginado y filtrado server-side

Filtros por estado, prioridad, categoria, agente, busqueda q y
vencidas. Orden semantico por prioridad. Aislamiento RN-01.

feat(solicitudes): agrega creacion y detalle con RN-04 y RN-07

feat(solicitudes): agrega edicion con recalculo de SLA (RN-04)

feat(solicitudes): agrega transiciones con RN-02, RN-05 y RN-06
```
