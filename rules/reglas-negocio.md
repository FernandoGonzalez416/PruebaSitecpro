# Reglas de negocio — MesaSitec

## RN-01 — Aislamiento entre organizaciones

Todo acceso a datos se filtra por el `tenantId` que viene en el token de sesión. Si un usuario de la organización A pide un recurso de la organización B, la respuesta debe ser **404 Not Found**, no 403 Forbidden. Un 403 confirmaría que ese recurso existe; con un 404 el usuario no puede distinguir entre "no existe" y "existe pero no es tuyo".

**Checklist:**
- [ ] Toda query a `Solicitud`, `Categoria` o `Usuario` incluye `.Where(x => x.tenantId == tenantIdDelToken)`.
- [ ] Si el recurso no existe o el `tenantId` no coincide → 404 con `codigo: "RECURSO_NO_ENCONTRADO"`.
- [ ] El endpoint `/me` solo devuelve datos del usuario autenticado (implícitamente filtrado por su token).

---

## RN-02 — Máquina de estados

Una solicitud solo puede moverse por estas transiciones:

| Estado actual | Acciones permitidas |
|---|---|
| `Nueva` | `asignar` → `Asignada` · `cancelar` → `Cancelada` |
| `Asignada` | `iniciar` → `EnProceso` · `asignar` → `Asignada` (reasignar) · `cancelar` → `Cancelada` |
| `EnProceso` | `resolver` → `Resuelta` · `asignar` → `Asignada` · `cancelar` → `Cancelada` |
| `Resuelta` | `cerrar` → `Cerrada` · `reabrir` → `EnProceso` |
| `Cerrada` | (estado final, no admite acciones) |
| `Cancelada` | (estado final, no admite acciones) |

Cualquier acción fuera de esta tabla debe responder **409 Conflict** con `codigo: "TRANSICION_INVALIDA"`.

**Checklist:**
- [ ] Implementar a mano (sin librerías externas).
- [ ] Validar que la transición existe en la tabla antes de ejecutarla.
- [ ] Si no existe → 409 con `codigo: "TRANSICION_INVALIDA"`.
- [ ] Las pruebas unitarias deben cubrir cada transición válida y al menos 3 inválidas.

---

## RN-03 — Permisos por rol

| Acción | Admin | Agente | Solicitante |
|---|---|---|---|
| Listar todas las solicitudes de su organización | ✅ | ✅ | ❌ — solo las que él creó |
| Ver el detalle de una solicitud | ✅ | ✅ | ✅ solo las propias |
| Crear una solicitud | ✅ | ✅ | ✅ |
| Editar título, descripción, categoría o prioridad | ✅ | ✅ | ✅ solo las propias y solo si están en estado `Nueva` |
| `asignar` / `iniciar` / `resolver` / `reabrir` | ✅ | ✅ | ❌ |
| `cerrar` | ✅ | ✅ | ✅ solo las propias |
| `cancelar` | ✅ | ❌ | ❌ |

Si un usuario intenta algo que su rol no permite → **403 Forbidden** con `codigo: "OPERACION_NO_PERMITIDA"`.

**Checklist:**
- [ ] Combinar RN-02 + RN-03: la acción debe estar permitida por el estado **y** por el rol.
- [ ] Un Solicitante no puede ver solicitudes de otros usuarios.
- [ ] Un Solicitante solo edita las propias y solo en estado `Nueva`.
- [ ] Un Agente no puede `cancelar` (solo Admin).

---

## RN-04 — Cálculo del SLA

Cada categoría define un plazo base en horas (`slaHoras`). La prioridad de la solicitud lo ajusta con un factor:

```
factor = {
  Critica: 0.5,
  Alta:    0.75,
  Media:   1.0,
  Baja:    2.0
}

fechaLimiteSla = fechaCreacion + (categoria.slaHoras × factor[prioridad]) horas
```

Ejemplos:
- Categoría Incidente (8 h) con prioridad Crítica → límite a las 4 horas.
- Categoría Consulta (24 h) con prioridad Baja → límite a las 48 horas.

**Reglas adicionales:**
- El cálculo se hace **siempre en el servidor**. Si el cliente envía `fechaLimiteSla` en el cuerpo de la petición, se **ignora en silencio**.
- Si se cambia la prioridad o la categoría de una solicitud que aún no está resuelta, el SLA se **recalcula** — pero `fechaCreacion` no se toca nunca.
- Una solicitud se considera **vencida** si `fechaLimiteSla` ya pasó y su estado no es `Resuelta`, `Cerrada` ni `Cancelada`.

**Checklist:**
- [ ] Ignorar `fechaLimiteSla` si el cliente lo envía (no guardar, no leer).
- [ ] Al cambiar `prioridad` o `categoriaId` en un PUT, si la solicitud no está resuelta/cerrada/cancelada → recalcular `fechaLimiteSla`.
- [ ] El endpoint `GET /solicitudes` con `?vencidas=true` debe aplicar el filtro según la definición de "vencida".
- [ ] Pruebas unitarias para al menos 3 combinaciones de categoría × prioridad.

---

## RN-05 — Asignación válida

Al ejecutar la acción `asignar`, el `agenteId` recibido debe cumplir **todo** lo siguiente:
1. Existe
2. Está activo
3. Pertenece a la misma organización
4. Tiene rol `Agente` o `Admin`

Si falla cualquiera de las cuatro condiciones → **422** con `codigo: "AGENTE_INVALIDO"`.

---

## RN-06 — Cierre con justificación

- La acción `resolver` exige un motivo de al menos **20 caracteres**.
- La acción `cancelar` exige un motivo de al menos **10 caracteres**.

Si no se cumple → **422** con `codigo: "MOTIVO_REQUERIDO"`.

El motivo se guarda en `motivoResolucion` o `motivoCancelacion` según corresponda. Al resolver, además, se registra `fechaResolucion`.

---

## RN-07 — Código de la solicitud

Formato: `SOL-{año}-{correlativo de 5 dígitos con ceros a la izquierda}`.

El correlativo es **independiente por organización y por año**, y empieza en `00001`.

Ejemplo:
- La cuadragésima segunda solicitud de la Cooperativa Norte en 2026 es `SOL-2026-00042`.
- La primera del Bufete Sur en 2026 es `SOL-2026-00001`.

No es necesario que el correlativo sea infalible ante peticiones simultáneas — queda fuera del alcance de esta prueba.

**Checklist:**
- [ ] El código se genera en el servidor al crear la solicitud.
- [ ] El correlativo se reinicia cada año por organización.
- [ ] Formato exacto: `SOL-` + año (4 dígitos) + `-` + correlativo (5 dígitos, padding con ceros a la izquierda).
