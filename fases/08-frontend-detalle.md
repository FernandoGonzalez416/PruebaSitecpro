# Fase 08 — Frontend: detalle + acciones + modal

## Objetivo

Implementar `/solicitudes/:id` con toda la información de la solicitud y los botones de acción disponibles según **estado + rol**, más el modal para ejecutar la acción (asignar agente o escribir motivo). Los botones no permitidos **no deben existir en el DOM**.

## Alcance

- Vista de detalle con todos los `detalle-*` de la sección 7.4.
- Botones `btn-editar` y `btn-accion-*` renderizados con `v-if` cruzando RN-02 y RN-03.
- Modal genérico (`modal-accion`) para asignar agente / escribir motivo.
- Llamadas al endpoint de transiciones.

## Pasos

1. **Vista `/solicitudes/:id`** — cargar detalle con `GET /solicitudes/{id}`.
   - `detalle-codigo`, `detalle-titulo`, `detalle-descripcion`, `detalle-estado`, `detalle-prioridad`, `detalle-categoria`, `detalle-agente`, `detalle-fecha-creacion`, `detalle-fecha-limite`, `detalle-vencida`, `detalle-motivo`.
   - `detalle-vencida` solo si aplica; `detalle-motivo` con `motivoResolucion`/`motivoCancelacion` según corresponda.
   - Formatear fechas legibles (mantener el valor exacto en el `data-testid` se puede mostrar formateado; ver nota).
   - Estados: cargando / error (con `toast-mensaje`) / contenido.
2. **Botones de acción — regla CRÍTICA (sección 7.5)**:
   - Cruzar RN-02 (acciones del estado actual) con RN-03 (acciones del rol).
   - **`v-if`**: si la acción no corresponde al estado o al rol, el botón NO se renderiza. Nunca `disabled`, `v-show`, `display:none`.
   - Regla combinada de [rules/frontend.md](../rules/frontend.md):
     | Acción | Admin/Agente | Solicitante |
     |---|---|---|
     | asignar/iniciar/resolver/reabrir | ✅ si estado lo permite | ❌ |
     | cerrar | ✅ si estado lo permite | ✅ solo propias y si estado lo permite |
     | cancelar | ✅ solo Admin | ❌ |
   - `btn-editar`: visible si el usuario puede editar (Admin/Agente de la org; Solicitante solo propias y estado `Nueva`) → `/solicitudes/:id/editar`.
   - Ejemplo: Solicitante con solicitud en `Nueva` ve solo `btn-editar`, ningún `btn-accion-*`.
3. **Modal genérico (`modal-accion`)**:
   - Para `asignar`: `modal-select-agente` (select con agentes de la org — cargar lista de usuarios agentes; si no hay endpoint, decidir: se puede obtener de una fuente razonable o documentar la omisión).
   - Para `resolver`/`cancelar`: `modal-motivo` (textarea; validar longitud mínima 20/10 según acción).
   - `modal-error` para errores de la API (ej. `TRANSICION_INVALIDA`, `AGENTE_INVALIDO`, `MOTIVO_REQUERIDO`).
   - `modal-confirmar` (ejecutar) y `modal-cancelar` (cerrar).
4. **Ejecución** — llamar a `POST /solicitudes/{id}/transiciones` con `{ accion, agenteId? | motivo? }`.
   - Al éxito: recargar el detalle, cerrar el modal, mostrar `toast-mensaje` de éxito.
   - Al error: mostrar `modal-error` con el mensaje (sin cerrar el modal).
5. **Refresco del detalle** después de cada transición (los botones dependen del nuevo estado).

## Definición de terminado

- [ ] `tsc --noEmit` pasa.
- [ ] Todos los `detalle-*` existen literalmente.
- [ ] Un Solicitante viendo una solicitud `Nueva` propia NO tiene ningún `btn-accion-*` en el DOM (verificar con DevTools).
- [ ] Un Admin viendo una `Nueva` tiene `btn-accion-asignar` y `btn-accion-cancelar`; NO tiene `iniciar/resolver/cerrar/reabrir`.
- [ ] Agente NO ve `btn-accion-cancelar` (RN-03).
- [ ] `asignar` abre modal con `modal-select-agente`; `resolver`/`cancelar` con `modal-motivo`.
- [ ] Un motivo de 15 caracteres en `resolver` → `modal-error` (validación de longitud en cliente + servidor).
- [ ] Modal muestra error de la API en `modal-error` y no se cierra.
- [ ] Botones se actualizan tras una transición exitosa.

## Qué NO hacer en esta fase

- No renderizar botones con `disabled` o `v-show` — siempre `v-if`.
- No inventar `data-testid`.
- No dejar `any` en los tipos de la respuesta de transición.

## Notas para el registro de decisiones

- Fuente de la lista de agentes para `modal-select-agente` (el contrato no expone "listar usuarios de la org"; decidir y declarar).
- Formato de fecha mostrada.

## Commit sugerido

```
feat(frontend): agrega detalle de solicitud con acciones por estado y rol

Botones btn-accion-* renderizados con v-if segun RN-02 y RN-03,
con modal de accion para asignar agente o ingresar motivo.
```
