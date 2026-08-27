# AGENTS.md — HelpDesk

## Resumen del proyecto

HelpDesk es una mesa de servicio SaaS multi-tenant construida con .NET 8 (Web API, EF Core, SQLite) y Vue 3 + TypeScript strict + Pinia + Vite. El objetivo es permitir que varias organizaciones compartan una misma instancia de aplicación sin que los datos de una se filtren a otra, gestionando solicitudes de soporte con un flujo de estados, cálculo automático de SLA y control de acceso por rol.

## Arquitectura obligatoria

### Backend — capas
```
backend/src/
├── Api/            → Controllers, Middleware, Program.cs
├── Aplicacion/     → Casos de uso, DTOs, servicios de aplicación
├── Dominio/        → Entidades, Value Objects, reglas de negocio (máquina de estados, SLA, permisos)
└── Infraestructura/→ EF Core (DbContext, migraciones, repositorios), JWT, BCrypt
```
Prohibido meter lógica de negocio dentro de los controllers. La máquina de estados, el cálculo del SLA y las validaciones de permisos deben vivir en Dominio/, donde se puedan probar sin levantar la app completa.

### Frontend
```
frontend/src/
├── api/        → Cliente HTTP centralizado (único módulo, inyecta token, redirige a /login en 401)
├── components/ → Componentes reutilizables
├── views/      → Páginas (login, listado, detalle, formulario)
├── stores/     → Pinia stores
├── types/      → DTOs tipados (camelCase exacto)
└── router/     → Vue Router con guard para rutas privadas
```

### Estructura del repositorio
```
/
├── README.md
├── DECISIONES.md
├── .env.example
├── backend/
│   ├── src/{Api,Aplicacion,Dominio,Infraestructura}
│   └── tests/
└── frontend/
    └── src/{api,components,views,stores,types,router}
```

## Reglas de negocio

Ver archivo separado: [rules/reglas-negocio.md](rules/reglas-negocio.md)

Resumen de las 7 reglas:
- **RN-01** — Aislamiento multi-tenant: todo acceso filtrado por `tenantId` del token. Recurso de otra org → 404 (nunca 403).
- **RN-02** — Máquina de estados: solo las transiciones de la tabla. Cualquier otra → 409 con `TRANSICION_INVALIDA`.
- **RN-03** — Permisos por rol cruzados con estado de la solicitud.
- **RN-04** — SLA calculado siempre en servidor; ignorar `fechaLimiteSla` del cliente; recalcular si cambia categoría/prioridad.
- **RN-05** — Asignación válida: agenteId existe, activo, mismo tenant, rol Agente/Admin → 422 `AGENTE_INVALIDO`.
- **RN-06** — Resolver exige motivo ≥ 20 caracteres; cancelar exige motivo ≥ 10 caracteres → 422 `MOTIVO_REQUERIDO`.
- **RN-07** — Código `SOL-{año}-{correlativo5}` independiente por org y año.

## Contrato de API

Ver archivo separado: [rules/contrato-api.md](rules/contrato-api.md)

El contrato es literal y se prueba automáticamente. camelCase exacto, fechas ISO-8601 con sufijo Z, errores en `application/problem+json` con campo `codigo` obligatorio. Nueve rutas exactas, códigos de error exactos.

## Convenciones de código

- **TypeScript**: modo `strict`. Prohibido `any` explícito sin justificar en DECISIONES.md.
- **C#**: Nullable Reference Types habilitado. Buenas prácticas de nullabilidad.
- **Cliente HTTP**: un único módulo centralizado en `frontend/src/api/` que inyecte el token JWT en cada petición y redirija a `/login` ante un 401.
- **DTOs**: todos los DTOs de la API deben estar tipados en TypeScript con camelCase exacto.

## Testing

Ver archivo separado: [rules/testing.md](rules/testing.md)

Mínimo **8 pruebas unitarias con xUnit** cubriendo:
- Máquina de estados (RN-02)
- Cálculo de SLA (RN-04)
- Reglas de permisos (RN-03)

`dotnet test` debe pasar en verde. Todo cambio a lógica de dominio debe incluir su test.

## data-testid (obligatorio y literal)

Ver archivo separado: [rules/frontend.md](rules/frontend.md)

Los `data-testid` son literales. Deben copiarse EXACTOS como aparecen en la sección 7.4 de la especificación. Nunca renombrarlos, nunca omitirlos.

Los botones `btn-accion-*` que no correspondan al estado actual o al rol del usuario **no deben existir en el DOM** (no basta con `disabled` ni `display:none`).

## Qué NO debe hacer la IA sin preguntar primero

1. No agregar entidades al modelo de datos sin justificar.
2. No cambiar SQLite por otra base de datos.
3. No mover lógica de negocio fuera de las capas definidas (Api/Aplicacion/Dominio/Infraestructura).
4. No inventar endpoints o campos fuera del contrato documentado en [rules/contrato-api.md](rules/contrato-api.md).
5. No usar una librería externa para la máquina de estados — debe implementarse a mano.
6. No filtrar, ordenar ni paginar en el cliente — todo es server-side.

## Flujo de trabajo esperado

- Cambios pequeños y verificables.
- Sin refactors masivos sin que se pidan explícitamente.
- Antes de dar por cerrado un cambio: ejecutar `dotnet test` y `tsc --noEmit`.

## Commits y documentación de decisiones

### Conventional Commits

Todo commit generado o sugerido por la IA debe seguir el formato definido en [rules/conventional-commits.md](rules/conventional-commits.md) sin excepción. Esto incluye el tipo correcto (`feat`, `fix`, `refactor`, `test`, `docs`, `chore`, `style`), el alcance cuando aplica, y la referencia a reglas de negocio (RN-XX) en el cuerpo cuando corresponda.

### Registro de decisiones técnicas

Cualquier decisión no trivial tomada durante una sesión de trabajo con IA debe quedar registrada según la plantilla de [rules/registro-decisiones.md](rules/registro-decisiones.md). Este registro alimenta luego el `DECISIONES.md` que exige la sección 8.4 del enunciado (máximo 1 página, curado con las 3 decisiones más importantes, uso de IA, qué se haría distinto y dónde se atascó el desarrollo).

## Checklist pre-commit

- [ ] **RN-01** — ¿Toda consulta a datos filtra por `tenantId` extraído del token? ¿Recursos de otra organización devuelven 404, no 403?
- [ ] **camelCase exacto** — ¿Propiedades de requests/responses usan exactamente los nombres del contrato (ej. `fechaCreacion`, `fechaLimiteSla`, `tenantId`)?
- [ ] **data-testid exactos** — ¿Cada `data-testid` coincide literalmente con la sección 7.4? (ej. `login-email`, no `loginEmail`)
- [ ] **Botones de acción** — ¿Los `btn-accion-*` que no aplican al estado/rol del usuario NO están en el DOM (no solo ocultos o deshabilitados)?
- [ ] **Paginación** — ¿`paginacion-info` contiene EXACTAMENTE el formato `"Página X de Y — Z resultados"` (con espacios, con em dash)?
- [ ] **Formato de error** — ¿Toda respuesta 4xx/5xx usa `application/problem+json` con campo `codigo`?
- [ ] **SLA** — ¿`fechaLimiteSla` se ignora si el cliente lo envía? ¿Se recalcula al cambiar categoría o prioridad sin tocar `fechaCreacion`?
- [ ] **Testing** — ¿`dotnet test` pasa? ¿Hay al menos 8 pruebas? ¿Todo cambio en Dominio/ tiene su test?
- [ ] **TypeScript** — ¿`tsc --noEmit` pasa sin errores? ¿No hay `any` explícito sin justificar?
- [ ] **Secretos** — ¿No hay secretos versionados? ¿Se usó `.env.example` con valores de ejemplo?
- [ ] **Conventional Commits** — ¿El mensaje de commit sigue el formato definido en [rules/conventional-commits.md](rules/conventional-commits.md)?
- [ ] **Decisiones registradas** — ¿Si esta sesión incluyó una decisión no trivial, quedó registrada según [rules/registro-decisiones.md](rules/registro-decisiones.md)?
