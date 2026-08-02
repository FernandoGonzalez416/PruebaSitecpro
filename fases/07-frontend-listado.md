# Fase 07 — Frontend: listado de solicitudes

## Objetivo

Implementar `/solicitudes`: tabla con filtros, búsqueda y paginación **todo server-side**, con los tres estados (cargando, vacío, error) y los `data-testid` de la sección 7.4.

## Alcance

- Vista `/solicitudes` con la tabla.
- Filtros: estado, prioridad, categoría, vencidas, búsqueda `q`.
- Paginación server-side (`page`, `pageSize`) y botones anterior/siguiente.
- `paginacion-info` con el formato EXACTO `Página X de Y — Z resultados`.
- Estados cargando / vacío / error.
- Botón "Nueva solicitud".

## Pasos

1. **Store de solicitudes (`src/stores/solicitudes.ts`)**:
   - Estado: `items`, `page`, `pageSize`, `total`, `totalPaginas`, `cargando`, `error`.
   - Acción `cargar(filtros)` que llama al API con los parámetros (estado, prioridad, categoriaId, vencidas, q, page, pageSize, sort).
   - Acciones de paginación: `irAPagina(n)`.
2. **Vista `/solicitudes`** — estructura:
   - Filtros con `filtro-estado`, `filtro-prioridad`, `filtro-categoria`, `filtro-vencidas`, `filtro-busqueda`, `btn-limpiar-filtros`.
   - Cambiar un filtro → volver a `page: 1` y recargar (server-side).
   - `filtro-busqueda` con debounce (o botón de aplicar) para no disparar por tecla.
   - Botón `btn-nueva-solicitud` → `/solicitudes/nueva`.
   - Tabla con `tabla-solicitudes`.
3. **Filas** — por cada solicitud:
   - `<tr>` con `data-testid="fila-solicitud"` y **`data-codigo="SOL-2026-00001"`** (código real de la fila).
   - Celdas: `celda-codigo`, `celda-estado`, `celda-prioridad`, `celda-sla`, y `badge-vencida` solo si la solicitud está vencida.
   - Click en la fila (o el código) → `/solicitudes/{id}`.
4. **Paginación**:
   - `paginacion-anterior`, `paginacion-siguiente` (botones; deshabilitados en primera/última página **está bien**, no son botones de acción del detalle).
   - `paginacion-info` con texto exacto: `Página X de Y — Z resultados` (con espacios, con em dash `—`).
5. **Estados**:
   - Cargando: `listado-cargando` visible (spinner/esqueleto). La tabla no debe quedarse en blanco sin señal.
   - Vacío: `listado-vacio` cuando `items.length === 0` y no está cargando.
   - Error: mostrar `toast-mensaje` con el error (o un estado de error explícito en la vista).
6. **Categorías** — cargar `GET /categorias` al montar para llenar `filtro-categoria`.

## Definición de terminado

- [ ] `tsc --noEmit` pasa.
- [ ] Filtros y paginación se resuelven en el servidor (verificar en la pestaña Red que se envían query params).
- [ ] `paginacion-info` contiene literalmente `Página X de Y — Z resultados`.
- [ ] `fila-solicitud` tiene `data-codigo` correcto por fila.
- [ ] Existen en el DOM: `btn-nueva-solicitud`, `filtro-estado`, `filtro-prioridad`, `filtro-categoria`, `filtro-vencidas`, `filtro-busqueda`, `btn-limpiar-filtros`, `tabla-solicitudes`, `celda-codigo`, `celda-estado`, `celda-prioridad`, `celda-sla`, `badge-vencida`, `paginacion-anterior`, `paginacion-siguiente`, `paginacion-info`, `listado-vacio`, `listado-cargando`.
- [ ] `listado-vacio` solo se muestra cuando no hay datos y no está cargando.
- [ ] Un Solicitante solo ve sus solicitudes (lo resuelve el backend; no filtrar en el cliente).

## Qué NO hacer en esta fase

- No filtrar/ordenar/paginar en el cliente (trae la página del servidor).
- No renombrar `data-testid`.
- No renderizar `badge-vencida` para solicitudes no vencidas.

## Commit sugerido

```
feat(frontend): agrega listado de solicitudes con filtros y paginacion

Filtros y paginacion server-side con debounce en la busqueda.
Formato literal en paginacion-info y data-testid segun seccion 7.4.
```
