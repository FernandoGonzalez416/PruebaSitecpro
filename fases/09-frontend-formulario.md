# Fase 09 — Frontend: formulario crear/editar

## Objetivo

Implementar un único componente de formulario reutilizable para crear (`/solicitudes/nueva`) y editar (`/solicitudes/:id/editar`), con validación en el cliente y los `data-testid` de la sección 7.4.

## Alcance

- Componente `SolicitudForm.vue` compartido entre crear y editar.
- Vistas `/solicitudes/nueva` y `/solicitudes/:id/editar`.
- Validación en cliente (título 5–120, descripción 10–4000, categoría y prioridad requeridas).
- `data-testid`: `form-titulo`, `form-descripcion`, `form-categoria`, `form-prioridad`, `form-submit`, `form-cancelar`, `error-titulo`, `error-descripcion`, `error-categoria`.

## Pasos

1. **Cargar categorías** — `GET /categorias` al montar para llenar `form-categoria` (solo activas de la org).
2. **Componente `SolicitudForm`** — props:
   - `solicitud?: SolicitudDetalle | null` — si llega, modo edición (prellenar y cargar al `GET /solicitudes/{id}`); si no, modo creación.
3. **Validación en cliente**:
   - `titulo`: 5–120 caracteres → `error-titulo` si falla.
   - `descripcion`: 10–4000 caracteres → `error-descripcion` si falla.
   - `categoria`: requerida → `error-categoria` si falta.
   - Mostrar el error al intentar enviar o al perder el foco (decidir y mantener consistente).
   - El `data-testid` del error es visible solo cuando hay error.
4. **Envío**:
   - Crear → `POST /solicitudes` → éxito: navegar a `/solicitudes/{id}` del recurso creado (usar el `id` de la respuesta).
   - Editar → `PUT /solicitudes/{id}` → éxito: navegar al detalle.
   - Error de API (422 `VALIDACION`, 403, 404) → mostrar mensaje (reutilizar `toast-mensaje` o errores por campo si la API los devuelve en `errores`).
   - `form-submit` deshabilitado (pero visible) mientras se envía.
5. **`form-cancelar`** — vuelve atrás: si se llega desde detalle, a `/solicitudes/{id}`; si es creación, a `/solicitudes`.
6. **Restricción de edición (RN-03)** — el guard/ruta de edición debe impedir entrar si el usuario no puede editar:
   - Solicitante solo propias y en estado `Nueva`; si no aplica, redirigir al detalle (o mostrar error).
   - El backend ya valida; el frontend solo evita la UX incorrecta.

## Definición de terminado

- [ ] `tsc --noEmit` pasa.
- [ ] Crear una solicitud desde `/solicitudes/nueva` funciona y navega al detalle con su código.
- [ ] Editar desde `/solicitudes/:id/editar` funciona y recalcula el SLA (se ve `detalle-fecha-limite` distinto si cambia prioridad/categoría).
- [ ] Errores de validación en cliente muestran `error-titulo`, `error-descripcion`, `error-categoria` con los mensajes.
- [ ] `form-titulo`, `form-descripcion`, `form-categoria`, `form-prioridad`, `form-submit`, `form-cancelar` existen literalmente.
- [ ] Un Solicitante que intenta editar una solicitud que no es suya, o no está en `Nueva`, es redirigido/recibe error.

## Qué NO hacer en esta fase

- No crear dos componentes separados para crear y editar.
- No enviar `fechaLimiteSla` al servidor (se ignora, pero no lo mandes).
- No ocultar errores por campo con `display:none` si el test verifica presencia — los `error-*` deben **no renderizarse** cuando no hay error.

## Notas para el registro de decisiones

- Formato de validación elegido (a mano vs. librería como Vuelidate/zod) — si usas librería, decláralo.
- Manejo de navegación tras crear (por qué al detalle).

## Commit sugerido

```
feat(frontend): agrega formulario compartido de crear y editar

Validacion en cliente con data-testid por campo. Navega al
detalle tras crear o editar. Reutiliza el mismo componente.
```
