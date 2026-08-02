# Fase 04 — Dominio: estados, SLA y permisos + pruebas

## Objetivo

Implementar en `Dominio/` las tres reglas de negocio más importantes —máquina de estados (RN-02), cálculo de SLA (RN-04) y permisos por rol (RN-03)— **sin levantar la aplicación** y cubrirlas con al menos 8 pruebas unitarias xUnit.

## Alcance

- Máquina de estados a mano (prohibido usar librería de estados).
- Calculador de SLA con factores de prioridad.
- Reglas de permisos rol × estado.
- 8+ pruebas xUnit (ver [rules/testing.md](../rules/testing.md)).

## Pasos

1. **Máquina de estados (RN-02)** — en `Dominio/`:
   - Modelar las transiciones de la tabla (por ejemplo, un diccionario `estado → acciones → estadoDestino` o un método `Solicitud.AplicarAccion(accion, ...)`).
   - Estados finales: `Cerrada` y `Cancelada` no admiten acciones.
   - Cualquier acción no permitida → excepción de dominio que el API mapea a 409 `TRANSICION_INVALIDA`.
2. **SLA (RN-04)** — en `Dominio/`:
   - `fechaLimiteSla = fechaCreacion + (categoria.slaHoras * factor[prioridad]) horas`.
   - Factores: `Critica 0.5`, `Alta 0.75`, `Media 1.0`, `Baja 2.0`.
   - Método para saber si una solicitud está vencida (límite pasado **y** estado no final).
   - Recalcular al cambiar prioridad/categoría **sin tocar `fechaCreacion`**.
3. **Permisos (RN-03)** — en `Dominio/`:
   - Tabla/regla: `rol × acción → permitido`.
   - Un Solicitante solo ve/edita las propias (el estado `Nueva` condiciona la edición).
   - `cancelar` solo Admin; Agente no puede cancelar.
   - Violación → excepción de dominio que el API mapea a 403 `OPERACION_NO_PERMITIDA`.
4. **Pruebas xUnit** en `tests/` (estructura según [rules/testing.md](../rules/testing.md)):
   - **RN-02** — mínimo 4: `Nueva→Asignada`, `Resuelta→Cerrada`, `Nueva→Resuelta` inválida, `Cerrada→*` inválida.
   - **RN-04** — mínimo 3: Incidente+Crítica = +4 h, Consulta+Baja = +48 h, recalcular al cambiar prioridad sin tocar `fechaCreacion`.
   - **RN-03** — mínimo 2: Solicitante no ve solicitud ajena, Agente no puede cancelar.
   - Las pruebas no deben necesitar HTTP ni base de datos real.

## Definición de terminado

- [ ] `dotnet test` pasa en verde con **al menos 8 pruebas**.
- [ ] La máquina de estados está implementada a mano.
- [ ] Ninguna prueba levanta la API ni usa SQLite.
- [ ] Las excepciones de dominio definidas permiten mapear a los códigos del contrato.
- [ ] `dotnet build` compila.

## Qué NO hacer en esta fase

- No usar librerías de máquina de estados (Stateless, etc.).
- No tocar controladores ni endpoints.
- No usar `DateTime.Now` — siempre UTC en el dominio.

## Notas para el registro de decisiones

- Cómo modelaste la máquina de estados (switch/diccionario/en la entidad) — es una de las preguntas de entrevista.
- Dónde vive la tabla de permisos.

## Commit sugerido

```
feat(transiciones): implementa máquina de estados, SLA y permisos

Máquina de estados según RN-02, cálculo de fechaLimiteSla según
RN-04 y permisos por rol según RN-03, todos en Dominio/.
```
