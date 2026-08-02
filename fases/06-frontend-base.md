# Fase 06 — Frontend base: cliente HTTP, store, router, login

## Objetivo

Dejar la base del frontend: proyecto Vue 3 + TS strict levantado en `:5173`, cliente HTTP centralizado que inyecta el token y redirige a `/login` en 401, store de auth, guard de rutas y la vista de login con los `data-testid` de la sección 7.4.

## Alcance

- Cliente HTTP único en `src/api/`.
- Tipos de DTOs en `src/types/` (camelCase exacto del contrato).
- Store de auth (Pinia).
- Router con guard para rutas privadas.
- Vista `/login`.
- Navegación global (`app-nav`, `nav-usuario-nombre`, `nav-usuario-rol`, `btn-logout`, `toast-mensaje`).

## Pasos

1. **Tipos (`src/types/`)** — tipar todos los DTOs del contrato:
   - `LoginRequest`, `LoginResponse`, `Usuario`, `Categoria`, `SolicitudItem`, `SolicitudDetalle`, `Paginacion`, `TransicionRequest`, etc.
   - camelCase exacto (ej. `accessToken`, `tenantId`, `fechaLimiteSla`, `motivoResolucion`).
   - Enums TS para `rol`, `estado`, `prioridad` (con los strings exactos de la API).
2. **Cliente HTTP (`src/api/http.ts`)** — un único módulo:
   - `fetch` (o axios) con base `http://localhost:5080/api/v1`.
   - Inyecta `Authorization: Bearer <token>` desde el store en cada petición.
   - Intercepta **cualquier** 401 → limpia la sesión y redirige a `/login`.
   - Funciones por endpoint en `src/api/` (login, me, categorias, solicitudes, transiciones).
3. **Store auth (`src/stores/auth.ts`)**:
   - Estado: `token`, `usuario`.
   - Acciones: `login()`, `logout()`, `cargarMe()` (para restaurar sesión al recargar).
   - Persistir token (localStorage) para no perder sesión al refrescar.
4. **Router**:
   - Rutas: `/login` (pública), `/solicitudes`, `/solicitudes/nueva`, `/solicitudes/:id`, `/solicitudes/:id/editar` (privadas).
   - Guard: sin token → redirect a `/login`. Si ya hay token y vas a `/login` → redirect a `/solicitudes`.
5. **Vista `/login`**:
   - Campos email y password con `login-email`, `login-password`, botón `login-submit`.
   - Manejo de credenciales inválidas: mostrar `login-error` con el mensaje del error (marcar error de la API).
   - Al éxito: guardar sesión y navegar a `/solicitudes`.
6. **Layout/navegación**:
   - `app-nav` con `nav-usuario-nombre`, `nav-usuario-rol`, `btn-logout`.
   - `toast-mensaje` para errores globales (un componente simple de toast reutilizable).
7. **Estados de la vista**: login maneja cargando (deshabilitar botón) y error (`login-error`).

## Definición de terminado

- [ ] `tsc --noEmit` pasa sin errores, sin `any` explícito.
- [ ] Login funciona contra la API con credenciales semilla.
- [ ] Al recargar la página la sesión se mantiene (`cargarMe` o token persistido).
- [ ] Un 401 de cualquier endpoint redirige a `/login`.
- [ ] Todos los `data-testid` de esta fase existen literalmente: `app-nav`, `nav-usuario-nombre`, `nav-usuario-rol`, `btn-logout`, `toast-mensaje`, `login-email`, `login-password`, `login-submit`, `login-error`.
- [ ] Rutas privadas sin token redirigen a `/login`.

## Qué NO hacer en esta fase

- No implementar las pantallas de solicitudes (siguientes fases) — puedes dejar rutas placeholder.
- No usar `any` (ni siquiera en respuestas de fetch: tipar siempre).
- No duplicar el cliente HTTP en cada vista.

## Notas para el registro de decisiones

- Persistencia del token en localStorage vs. en memoria (trade-off con la redirección en 401).
- Por qué se decidió el almacenamiento elegido.

## Commit sugerido

```
feat(api-client): agrega cliente HTTP centralizado y tipos de DTOs

Unico modulo que inyecta el token JWT y redirige a /login ante
un 401. DTOs tipados en camelCase exacto del contrato.

feat(auth): agrega store de auth, guard de rutas y vista de login
```
