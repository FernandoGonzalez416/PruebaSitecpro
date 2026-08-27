# Contrato de API — HelpDesk

**Base:** `http://localhost:5080/api/v1`

**Content-Type:** `application/json` (requests y respuestas exitosas)

**Fechas:** ISO-8601 UTC con sufijo `Z` (ej. `"2026-01-15T08:00:00Z"`)

**Propiedades:** camelCase exacto en todos los objetos.

**Paginación de listados:** server-side. No traer todos los registros para filtrar en el navegador.

---

## Formato de error (4xx y 5xx)

Todas las respuestas de error usan `Content-Type: application/problem+json`:

```json
{
  "type": "https://helpdesk.local/errores/transicion-invalida",
  "title": "Transición inválida",
  "status": 409,
  "detail": "No se puede aplicar 'resolver' sobre una solicitud en estado 'Nueva'.",
  "codigo": "TRANSICION_INVALIDA",
  "errores": {
    "titulo": ["El título debe tener al menos 5 caracteres."]
  }
}
```

- `codigo` es **obligatorio** en todos los errores. Es el campo que revisan las pruebas automáticas.
- `errores` aparece solo en errores de validación (400 y 422).
- `type` sigue el patrón `https://helpdesk.local/errores/{kebab-case}`.

### Códigos de error

| Situación | HTTP | `codigo` |
|---|---|---|
| Token ausente, inválido o expirado | 401 | `NO_AUTENTICADO` |
| Credenciales incorrectas en el login | 401 | `NO_AUTENTICADO` |
| El rol no permite la operación | 403 | `OPERACION_NO_PERMITIDA` |
| Recurso inexistente o de otra organización | 404 | `RECURSO_NO_ENCONTRADO` |
| Transición de estado no permitida | 409 | `TRANSICION_INVALIDA` |
| Agente inválido al asignar | 422 | `AGENTE_INVALIDO` |
| Falta el motivo o es muy corto | 422 | `MOTIVO_REQUERIDO` |
| Parámetro de consulta fuera de rango | 400 | `PARAMETRO_INVALIDO` |
| Error de validación de campos | 422 | `VALIDACION` |

---

## Endpoints

### 1 · `POST /auth/login`

Autenticación. Sin autenticación.

**Petición:**
```json
{ "email": "agente1@norte.test", "password": "Sitec.2026" }
```

**Respuesta 200:**
```json
{
  "accessToken": "eyJhbGci...",
  "expiraEn": 28800,
  "usuario": {
    "id": "...",
    "nombre": "...",
    "email": "...",
    "rol": "Agente",
    "tenantId": "...",
    "tenantNombre": "Cooperativa Norte"
  }
}
```

---

### 2 · `GET /me`

Devuelve el objeto `usuario` del token autenticado (misma forma que dentro de la respuesta de login).

**Respuesta 200:**
```json
{
  "id": "...",
  "nombre": "...",
  "email": "...",
  "rol": "Agente",
  "tenantId": "...",
  "tenantNombre": "Cooperativa Norte"
}
```

---

### 3 · `GET /categorias`

Categorías activas de la organización del token.

**Respuesta 200:**
```json
[
  { "id": "...", "nombre": "Incidente", "slaHoras": 8 }
]
```

---

### 4 · `GET /solicitudes`

Listado paginado y filtrado. Parámetros de consulta:

| Parámetro | Tipo | Notas |
|---|---|---|
| `estado` | enum | filtro exacto |
| `prioridad` | enum | filtro exacto |
| `categoriaId` | guid | filtro exacto |
| `agenteId` | guid | filtro exacto |
| `q` | string | búsqueda en `titulo`, `descripcion` y `codigo`, sin distinguir mayúsculas |
| `vencidas` | bool | según definición RN-04 |
| `page` | int | default 1 |
| `pageSize` | int | default 20, máximo 100 |
| `sort` | string | `fechaCreacion`, `-fechaCreacion`, `prioridad`, `-prioridad`, `codigo`. Default `-fechaCreacion` |

Si `pageSize > 100` o `page < 1` → **400** con `codigo: "PARAMETRO_INVALIDO"`.

Al ordenar por prioridad, el orden es semántico: `Critica > Alta > Media > Baja`, no alfabético.

**Respuesta 200:**
```json
{
  "items": [
    {
      "id": "...",
      "codigo": "SOL-2026-00001",
      "titulo": "No puedo acceder al portal",
      "estado": "Nueva",
      "prioridad": "Alta",
      "categoria": { "id": "...", "nombre": "Incidente" },
      "agente": null,
      "fechaCreacion": "2026-01-15T08:00:00Z",
      "fechaLimiteSla": "2026-01-15T14:00:00Z",
      "vencida": false
    }
  ],
  "page": 1,
  "pageSize": 20,
  "total": 47,
  "totalPaginas": 3
}
```

Cuando hay agente asignado, `agente` es `{ "id": "...", "nombre": "..." }`.

---

### 5 · `POST /solicitudes`

Crear una solicitud.

**Petición:**
```json
{
  "titulo": "No puedo acceder al portal",
  "descripcion": "Al ingresar mis credenciales el sistema me devuelve a la pantalla de login.",
  "categoriaId": "...",
  "prioridad": "Alta"
}
```

**Respuesta 201** con el objeto completo de la solicitud y cabecera `Location` apuntando al nuevo recurso.

---

### 6 · `GET /solicitudes/{id}`

Detalle completo de la solicitud, incluyendo `descripcion`, `solicitante`, `motivoResolucion`, `motivoCancelacion` y `fechaResolucion`.

**Respuesta 200.**

---

### 7 · `PUT /solicitudes/{id}`

Editar solicitud.

**Petición:**
```json
{
  "titulo": "...",
  "descripcion": "...",
  "categoriaId": "...",
  "prioridad": "Critica"
}
```

Aplica RN-03 (Solicitante solo edita propias y solo en estado `Nueva`) y RN-04 (cambiar prioridad o categoría recalcula el SLA).

**Respuesta 200.**

---

### 8 · `POST /solicitudes/{id}/transiciones`

Ejecutar una acción del flujo.

**Peticiones según acción:**

```json
// asignar
{ "accion": "asignar", "agenteId": "..." }

// resolver
{ "accion": "resolver", "motivo": "Se restableció la contraseña del usuario y se validó el acceso." }

// iniciar, cerrar, reabrir
{ "accion": "iniciar" }

// cancelar
{ "accion": "cancelar", "motivo": "Duplicada de SOL-2026-00012." }
```

Acciones válidas: `asignar`, `iniciar`, `resolver`, `cerrar`, `reabrir`, `cancelar`.

**Respuesta 200** con la solicitud actualizada.

---

### 9 · `GET /health`

Estado del servicio. **Sin autenticación.**

**Respuesta 200:**
```json
{ "estado": "ok" }
```
