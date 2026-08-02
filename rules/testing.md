# Testing — MesaSitec

## Stack

- **Framework:** xUnit
- **Proyecto:** `backend/tests/`

## Cobertura mínima

Al menos **8 pruebas unitarias**, distribuidas así:

### Máquina de estados (RN-02) — mínimo 3 pruebas

1. Transición válida: `Nueva` → `Asignada` vía acción `asignar`.
2. Transición válida: `Resuelta` → `Cerrada` vía acción `cerrar`.
3. Transición inválida: `Nueva` → `Resuelta` (sin pasar por los estados intermedios) devuelve error.
4. Transición inválida: `Cerrada` → cualquier estado devuelve error.

### Cálculo de SLA (RN-04) — mínimo 3 pruebas

1. Categoría Incidente (8 h) + prioridad Crítica (factor 0.5) = `fechaCreacion + 4h`.
2. Categoría Consulta (24 h) + prioridad Baja (factor 2.0) = `fechaCreacion + 48h`.
3. Recalcular SLA tras cambiar prioridad de Baja a Crítica sin modificar `fechaCreacion`.
4. (Opcional) `fechaLimiteSla` enviado por el cliente se ignora.

### Permisos (RN-03) — mínimo 2 pruebas

1. Un Solicitante intenta ver una solicitud que no le pertenece → error.
2. Un Agente intenta `cancelar` → error (solo Admin puede cancelar).
3. (Opcional) Un Admin puede ejecutar cualquier acción permitida por el estado.

## Reglas

- Todo cambio a lógica de dominio (máquina de estados, SLA, permisos) debe venir acompañado de su prueba unitaria.
- Las pruebas deben ejecutarse sin levantar la aplicación completa (sin HTTP, sin base de datos real).
- `dotnet test` debe pasar en verde antes de cada commit.

## Estructura esperada

```
backend/tests/
└── Dominio/
    ├── MaquinaEstadosTests.cs
    ├── SlaTests.cs
    └── PermisosTests.cs
```
