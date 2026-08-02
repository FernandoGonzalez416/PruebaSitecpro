# Frontend — MesaSitec

## Atributos data-testid (obligatorios y literales)

Se ejecutarán pruebas automáticas de interfaz contra estos selectores. Un `data-testid` faltante o mal escrito equivale a la funcionalidad no entregada, aunque la pantalla funcione perfectamente.

### Globales
```
app-nav
nav-usuario-nombre
nav-usuario-rol
btn-logout
toast-mensaje
```

### Login
```
login-email
login-password
login-submit
login-error
```

### Listado de solicitudes
```
btn-nueva-solicitud
filtro-estado
filtro-prioridad
filtro-categoria
filtro-vencidas
filtro-busqueda
btn-limpiar-filtros
tabla-solicitudes
fila-solicitud          (uno por fila, con atributo data-codigo="SOL-2026-00001")
celda-codigo
celda-estado
celda-prioridad
celda-sla
badge-vencida
paginacion-anterior
paginacion-siguiente
paginacion-info
listado-vacio
listado-cargando
```

**paginacion-info** debe contener exactamente el formato:
```
Página X de Y — Z resultados
```
(con espacios, con em dash `—`, sin variaciones)

### Formulario (crear y editar — mismo componente)
```
form-titulo
form-descripcion
form-categoria
form-prioridad
form-submit
form-cancelar
error-titulo
error-descripcion
error-categoria
```

### Detalle
```
detalle-codigo
detalle-titulo
detalle-descripcion
detalle-estado
detalle-prioridad
detalle-categoria
detalle-agente
detalle-fecha-creacion
detalle-fecha-limite
detalle-vencida
detalle-motivo
btn-editar
btn-accion-asignar
btn-accion-iniciar
btn-accion-resolver
btn-accion-cerrar
btn-accion-reabrir
btn-accion-cancelar
modal-accion
modal-select-agente
modal-motivo
modal-error
modal-confirmar
modal-cancelar
```

---

## Regla de visibilidad de botones de acción

Los botones `btn-accion-*` que **no** correspondan al estado actual de la solicitud **o** al rol del usuario **no deben existir en el DOM**.

No basta con:
- `disabled` (el elemento sigue en el DOM)
- `display: none` / `v-show` / `visibility: hidden`
- `opacity: 0` / `pointer-events: none`

Se debe usar `v-if` (Vue) para que el elemento no se renderice.

### Lógica combinada

Para determinar qué botones renderizar, hay que cruzar RN-02 (transiciones válidas por estado) con RN-03 (acciones permitidas por rol):

| Acción | Admin | Agente | Solicitante |
|---|---|---|---|
| `asignar` | ✅ si estado lo permite | ✅ si estado lo permite | ❌ |
| `iniciar` | ✅ si estado lo permite | ✅ si estado lo permite | ❌ |
| `resolver` | ✅ si estado lo permite | ✅ si estado lo permite | ❌ |
| `reabrir` | ✅ si estado lo permite | ✅ si estado lo permite | ❌ |
| `cerrar` | ✅ si estado lo permite | ✅ si estado lo permite | ✅ solo propias y si estado lo permite |
| `cancelar` | ✅ si estado lo permite | ❌ | ❌ |

**Ejemplo:** Un usuario con rol `Solicitante` viendo una solicitud en estado `Nueva`:
- Ve `btn-editar` (puede editar las propias en estado Nueva)
- NO ve ningún `btn-accion-*` (no puede asignar, iniciar, resolver, cerrar reabrir ni cancelar)

### Estados de las vistas

Cada vista debe manejar tres estados:

1. **Cargando** — mostrar indicación visual (spinner, esqueleto). Usar `data-testid="listado-cargando"` para la tabla.
2. **Vacío** — mostrar mensaje de que no hay datos. Usar `data-testid="listado-vacio"`.
3. **Error** — mostrar mensaje de error (con `toast-mensaje` para errores globales o `login-error` para el login).

Una tabla que se queda en blanco mientras carga, sin ninguna señal, cuenta como incompleta.
