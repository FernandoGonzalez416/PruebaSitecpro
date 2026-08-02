# Decisiones técnicas — MesaSitec

Resumen ejecutivo del desarrollo. El registro completo y ampliado vive en `rules/registro-decisiones.md`.

## 1 · Las 3 decisiones más importantes

**1. Reglas de negocio como funciones puras en `Dominio/` (RN-02, RN-03, RN-04).**
La máquina de estados es un diccionario estático que replica la tabla de transiciones (RN-02), los permisos un `switch` puro rol × acción × estado (RN-03) y el SLA una función con `ahora` explícito que recalcula sobre `fechaCreacion` (RN-04).
*Alternativa descartada:* Stateless, atributos `[RequierePermiso]` o lógica en los controllers.
*Por qué:* la lógica se prueba sin levantar la API (56 tests), la tabla se lee igual que el enunciado y los controllers quedan como orquestadores finos.

**2. Contrato de errores con `codigo` obligatorio y `application/problem+json` (fase 03).**
Un `IExceptionHandler` central mapea excepciones de dominio a su código/status, `InvalidModelStateResponseFactory` convierte el ModelState en 422 `VALIDACION`, y `MapInboundClaims = false` permite leer `sub` sin remapeos. El `Content-Type` problem+json se fija en el punto de escritura, no con `Response.ContentType` (lo sobrescribía `WriteAsJsonAsync`).
*Alternativa descartada:* middleware clásico con `try/catch` y respuestas de error ad-hoc por controller.
*Por qué:* las pruebas automáticas verifican `codigo` en toda respuesta 4xx/5xx; un solo lugar evita códigos equivocados.

**3. Persistencia determinista sobre SQLite (fases 02-03).**
Fechas en UTC con `ValueConverter` ISO-8601 "O" (SQLite no preserva `DateTimeKind`), ruta de la DB anclada a `ContentRootPath` (independiente del directorio de trabajo), semilla con GUIDs fijos y hash BCrypt con salt fijo (dos corridas = mismos datos).
*Alternativa descartada:* ticks como `long`, rutas absolutas en `appsettings`, o resolver relaciones semilla por email.
*Por qué:* el contrato exige ISO-8601 con `Z`, el DoD exige seed determinista y el levantamiento no debe depender del CWD.

## 2 · Qué se hizo con IA y qué a mano

- **IA (con revisión humana):** generación de código siguiendo especificaciones (dominio, endpoints, frontend), redacción de DTOs tipados, detección y corrección de bugs de integración (p. ej. el `Content-Type` pisado), y propuestas de commits/PRs por fase.
- **A mano (decisión humana):** el diseño de la arquitectura en capas, la interpretación de las 7 reglas de negocio, el contrato literal de la API, la elección de GUIDs fijos y la curaduría de este documento. La validación del comportamiento se hizo contra la API real (46/46 checks HTTP) y con revisión de los `data-testid` literales.

## 3 · Qué se haría distinto con una semana más

- Migrar la base a PostgreSQL para producción (SQLite quedó por portabilidad de la prueba).
- Implementar `GET /agentes` real en el backend (hoy es una constante del frontend) y prueba E2E con Playwright.
- Suite de integración automatizada sobre los 9 endpoints (hoy la verificación HTTP fue manual/semi-manual).
- `refresh token` con rotación en vez de un solo JWT de 8 horas.

## 4 · Dónde se atascó el desarrollo y cómo se resolvió

- **Fase 03 — claims del JWT ilegibles:** el pipeline por defecto de JwtBearer remapeaba `sub` → `ClaimTypes.NameIdentifier` y `/me` devolvía `null`. Se resolvió con `MapInboundClaims = false`.
- **Fase 03 — errores sin `Content-Type` problem+json:** `WriteAsJsonAsync` sobrescribía el content-type seteado antes; se resolvió pasándolo como argumento en el punto de escritura.
- **Fase 10 — PUT /solicitudes/{id} devolvía 500 al cambiar categoría (RN-04):** EF Core fixup nulleaba la navegación `Categoria` y `ConstruirDetalle` lanzaba `NullReferenceException`. Se resolvió re-consultando la solicitud tras `SaveChanges`, lo que además destapó un bug latente de identity de EF al reasignar el mismo agente (se corrigió seteando solo `AgenteId`).
- **Cuelgues de sesión de desarrollo:** sesiones largas se interrumpían a mitad de tarea; se mitigó con pasos pequeños y verificables (test → build → prueba HTTP real) y registrando cada decisión.
