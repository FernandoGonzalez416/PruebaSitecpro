# Registro de decisiones técnicas — MesaSitec

## Propósito

Cualquier decisión no obvia (elegir un enfoque sobre otro, resolver una ambigüedad del enunciado, desviarse de lo sugerido) debe quedar registrada con su razonamiento, no solo el resultado final.

El objetivo es que un revisor humano —incluido el candidato durante la entrevista técnica— pueda entender **por qué** se hizo cada cosa, no solo **qué** se hizo. La especificación del proyecto lo dice explícitamente: *"No importa si terminas ese cambio; importa que sepas dónde tocar"* y *"no me atasqué en nada" sí es una mala respuesta.*

---

## Formato de entrada

Cada entrada sigue esta plantilla:

```
## [YYYY-MM-DD] — <título corto de la decisión>

**Contexto:** qué problema, ambigüedad o disyuntiva se presentó.

**Decisión:** qué se hizo concretamente.

**Alternativa descartada:** qué otra opción se consideró y por qué se rechazó.

**Por qué:** razón concreta de la elección (rendimiento, mantenibilidad, restricción del enunciado, etc.).

**Alcance:** qué archivos, módulos o capas afecta esta decisión.
```

---

## Dónde vive este registro

Este archivo (`rules/registro-decisiones.md`) es el **registro de trabajo** — todas las decisiones no triviales que se toman durante el desarrollo se vuelcan aquí sin filtrar.

Al final del desarrollo (o al preparar la entrega), se **cura y resume** en `DECISIONES.md` (máximo 1 página, según sección 8.4 del enunciado), que debe incluir:
1. Tres decisiones técnicas más importantes, con la alternativa descartada y el porqué.
2. Qué se hizo con ayuda de IA y qué se escribió a mano.
3. Qué se haría distinto si hubiera una semana más.
4. En qué punto se atascó el desarrollo y cómo se resolvió.

---

## Reglas de disparo para la IA

El agente de IA debe generar o proponer una entrada en este registro **sin que se le pida explícitamente** cuando ocurra cualquiera de estas situaciones:

| Situación | Ejemplo concreto en MesaSitec |
|---|---|
| Se resuelve una ambigüedad del enunciado que no estaba explícita | El enunciado no define si `q` busca en `codigo` como substring exacto o parcial; se decide que es parcial sin distinguir mayúsculas |
| Se elige entre dos formas válidas de implementar una regla de negocio | Decidir si RN-03 se valida con un atributo en la acción, un servicio de permisos centralizado, o una tabla de permisos en base de datos |
| Se desvía de la estructura de carpetas o convención sugerida | Decidir poner los Value Objects dentro de Dominio/ en vez de en una carpeta separada |
| Se usa una librería o patrón no mencionado en la especificación | Agregar FluentValidation para validación de requests, o AutoMapper para mapeo DTO-entidad |
| Se deja algo sin implementar y hay que declararlo | No alcanza el tiempo para implementar el filtro `vencidas` en el frontend y se declara en DECISIONES.md |

---

## Qué NO registrar

No llenar el documento con decisiones triviales u obvias que cualquier desarrollador daría por sentadas. La prueba de fuego: **"¿un revisor necesitaría leer esto para entender por qué el proyecto es como es?"**

| No registrar | Sí registrar |
|---|---|
| Nombre de variables locales | Elegir entre repositorio genérico vs repositorio por entidad |
| Formato de indentación | Elegir si las transiciones se modelan como métodos en la entidad o como un servicio separado |
| Qué editor se usó | Decidir cómo se obtiene el correlativo de RN-07 (secuencia en DB vs tabla de correlativos) |
| Decisión obvia del stack (ej: "usé JWT porque es el estándar") | Decisión sobre cómo manejar la concurrencia al generar el código SOL-XXXX (el enunciado dice que no hace falta ser infalible) |

---

## Ejemplos

### Bien escrito

```
## [2026-01-16] — Validación de permisos centralizada vs por atributo

**Contexto:** RN-03 define permisos por rol y estado. Había que decidir
dónde poner esa validación para que no se riegue por los controllers.

**Decisión:** Se creó un servicio PermisoService en Dominio/ que recibe
el rol, la acción y el estado de la solicitud, y devuelve si la
operación está permitida. Cada acción del flujo (asignar, iniciar, etc.)
se mapea a un permiso específico.

**Alternativa descartada:** Usar un atributo personalizado [RequierePermiso]
en los endpoints del controlador. Se descartó porque la validación
depende del estado actual de la solicitud, que no se conoce hasta
que se recupera de la base de datos.

**Por qué:** Centralizar la lógica en Dominio/ permite probarla sin
levantar la API y evita duplicar validaciones si desde otro caso de uso
se necesita verificar el mismo permiso.

**Alcance:** Dominio/Servicios/PermisoService.cs, Dominio/Entidades/Solicitud.cs
```

### Mal escrito (demasiado vago)

```
## [2026-01-16] — Usé JWT

**Contexto:** Había que autenticar.

**Decisión:** Usé JWT.

**Alternativa descartada:** Ninguna.

**Por qué:** Es lo normal.

**Alcance:** auth.
```

---

## [2026-07-30] — GUIDs fijos en la semilla en lugar de resolver por email/nombre

**Contexto:** Los datos semilla referencian entidades entre sí (usuario que crea cada
solicitud, categoría, agente asignado). La fase 02 admite dos formas de resolver esas
relaciones: GUIDs fijos o búsqueda por email/nombre.

**Decisión:** Se usan GUIDs fijos y legibles en `SeedIds` (prefijos por entidad:
`11111111-…` tenants, `10000000-…` usuarios Norte, `30000000-…` categorías Norte,
etc.). El seeder los referencia por constante y no consulta la base para armar
relaciones.

**Alternativa descartada:** Resolver relaciones buscando usuarios/categorías por
email o nombre. Se descartó porque acopla el seeder al contenido de otros datos y
depende de que no existan duplicados; con GUIDs fijos el esquema de identidad queda
explícito y verificable.

**Por qué:** Unicidad, estabilidad entre corridas (misma semilla = mismos IDs) y
legibilidad en tests y consultas de verificación. Además permite que fases
posteriores (auth, endpoints) referencien IDs conocidos de antemano.

**Alcance:** `backend/src/Infraestructura/Data/Semilla/SeedIds.cs`, `SeedData.cs`.

---

## [2026-07-30] — Correlativo RN-07 resuelto con códigos literales en la semilla

**Contexto:** RN-07 exige `SOL-{año}-{correlativo de 5 dígitos}` independiente por
organización y por año. En los datos semilla hay dos organizaciones con secuencias
que empiezan en 00001.

**Decisión:** El seeder define cada código literalmente (`SOL-2026-00001` a
`SOL-2026-00025` en Cooperativa Norte, `SOL-2026-00001` a `SOL-2026-00008` en Bufete
Sur), respetando el reinicio por org. En runtime el correlativo se generará en el
servidor al crear solicitudes (fase 05); el enunciado aclara que no hace falta
hacerlo infalible ante concurrencia.

**Alternativa descartada:** Calcular el correlativo en el seeder consultando cuántas
solicitudes existen por org/año. Era innecesario y menos legible: los códigos son
parte de la especificación de los datos.

**Por qué:** La semilla debe ser declarativa y auditable; el correlativo se calcula
igual en el servidor en la fase de endpoints.

**Alcance:** `SeedData.cs` (campo `Codigo` de las 33 solicitudes).

---

## [2026-07-30] — Fechas en UTC con ValueConverter ISO-8601 "O" en SQLite

**Contexto:** SQLite no preserva `DateTimeKind`; al leer vuelve fechas con
`Kind=Unspecified`, lo que rompería la comparación con `DateTime.UtcNow` y la
serialización ISO-8601 con sufijo Z que exige el contrato de API.

**Decisión:** Un `ValueConverter<DateTime,string>` en el DbContext escribe con
`ToUniversalTime().ToString("O")` y lee con `DateTimeStyles.RoundtripKind`, forzando
`Kind=Utc`. Se aplica a `FechaCreacion`, `FechaLimiteSla` y `FechaResolucion`
(nullable). El seeder además construye todas las fechas con `Kind=Utc`.

**Alternativa descartada:** Guardar ticks (`HasConversion<long>`). Es menos legible
en la base y complica depurar; el formato "O" es ISO-8601 round-trip y ordena
cronológicamente como texto.

**Por qué:** El contrato exige fechas ISO-8601 con sufijo Z; sin `Kind=Utc` en
lectura, cualquier cálculo de SLA (fase 04) o serialización fallaría.

**Alcance:** `UtcDateTimeConverter.cs`, `MesaSitecDbContext.cs`.

---

## [2026-07-30] — Hash BCrypt con salt fijo en la semilla (tradeoff deliberado)

**Contexto:** El DoD exige semilla determinista (dos corridas = mismos datos). Con el
hash BCrypt por defecto (salt aleatorio) el `PasswordHash` diferiría entre corridas
aunque el resto de los datos fuera idéntico.

**Decisión:** Los 7 usuarios semilla comparten un único hash BCrypt generado con un
salt fijo del seeder (`$2a$11$…`, work factor 11), válido para la contraseña
`Sitec.2026`. Es un tradeoff de desarrollo: la semilla es determinista y verificable.

**Alternativa descartada:** Generar el hash con salt aleatorio y aceptar que solo la
semántica (no los bytes) sea estable. Se descartó porque el DoD pide datos 100%
idénticos entre corridas.

**Por qué:** Cumple el criterio de determinismo de la fase sin sacrificar que las
contraseñas estén hasheadas (nunca texto plano).

**Alcance:** `SeedData.cs`. Nota: la verificación de la fase confirmó que en runtime
no hay `Sitec.2026` en claro en la base.

---

## [2026-07-30] — Errores con IExceptionHandler y código ERROR_INTERNO

**Contexto:** La fase 03 exige que toda respuesta 4xx/5xx sea `application/problem+json`
con campo `codigo` obligatorio, y que una excepción no controlada devuelva 500 sin stack
trace. El contrato define 8 códigos de error pero ninguno para el 500 genérico.

**Decisión:** Se usó `IExceptionHandler` (nuevo en .NET 8), registrado por DI y activado
con `app.UseExceptionHandler()`. `ErrorHandler` mapea `ExcepcionNegocio` a su
`Codigo`/`Status`/`Detail`, y toda excepción inesperada a `500` con
`codigo: "ERROR_INTERNO"` (kebab `error-interno`), logueando el detalle solo en el
servidor. El reto 401 de JwtBearer lo maneja `JwtBearerEvents.OnChallenge`, porque ese
reto se genera fuera del pipeline de excepciones y nunca llegaría al `IExceptionHandler`.

**Alternativa descartada:** Middleware clásico con `try/catch` en `InvokeAsync`. Funciona,
pero hay que registrarlo a mano, gestionar el logging y respeta menos la composición por
DI del host de ASP.NET Core. `IExceptionHandler` también permite probar el mapeo
error→respuesta como una unidad.

**Por qué:** `ERROR_INTERNO` no está en la tabla del contrato; se eligió un código
coherente con la nomenclatura existente (`_` separador, kebab en `type`) para que las
pruebas automáticas tengan un valor estable que verificar en el 500.

**Alcance:** `backend/src/Api/Errores/ErrorHandler.cs`, `Program.cs`,
`backend/src/Dominio/Excepciones/ExcepcionNegocio.cs` y `ExcepcionNoAutenticado.cs`.

---

## [2026-07-30] — Secreto JWT por variable de entorno con fallback solo de desarrollo

**Contexto:** El secreto HS256 exige mínimo 32 caracteres y no debe estar hardcodeado en
código ni en configuración de producción. La API se levanta con `dotnet run` sin un
entorno que defina `JWT_SECRET`.

**Decisión:** `Program.cs` lee `JWT_SECRET`/`JWT_ISSUER`/`JWT_AUDIENCE` de variables de
entorno (con precedencia) con fallback a la sección `Authentication:JwtBearer` de
`appsettings`. `appsettings.json` declara `Issuer`/`Audience` y un `SecretKey` vacío; el
secret de desarrollo vive solo en `appsettings.Development.json` (`solo-desarrollo-…`,
≥32 caracteres). Si al iniciar no hay secret válido, la app falla rápido
(`InvalidOperationException`) en lugar de arrancar con autenticación rota. El
`.env.example` documenta las tres variables.

**Alternativa descartada:** Poner un secret "de ejemplo" en `appsettings.json`. Se
descartó para que producción no pueda arrancar con un secreto conocido y comprometido;
el fallback de desarrollo queda confinado al archivo de desarrollo que nunca se
despliega.

**Por qué:** Separar configuración de secretos por entorno es una práctica de seguridad
básica; el fail-fast evita que un despliegue mal configurado sirva 401 en silencio.

**Alcance:** `Program.cs`, `appsettings.json`, `appsettings.Development.json`,
`.env.example`.

---

## [2026-07-31] — Claims JWT sin remapeo (`MapInboundClaims = false`) para leer `sub`

**Contexto:** El token se emite con claims verbatim (`sub`, `tenantId`, `rol`, `email`).
El pipeline por defecto de JwtBearer remapea los claims de entrada —entre otros, `sub`
→ `ClaimTypes.NameIdentifier`—, por lo que `User.FindFirstValue("sub")` en `/me`
devolvería `null` y el sujeto no se podría leer desde el token emitido por
`GeneradorTokenJwt`.

**Decisión:** Se configuró `options.MapInboundClaims = false` en `AddJwtBearer`. Los
claims se mantienen con su nombre original (`"sub"`, `"tenantId"`, `"rol"`, `"email"`)
y el controller lee `User.FindFirstValue("sub")` (con fallback 401 si no parsea).
Se verificó en runtime: se decodificó el JWT emitido por login y `/me` devolvió el
mismo `id` de usuario que el claim `sub`.

**Alternativa descartada:** (a) `JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear()`:
muta un estático global del proceso y afecta a cualquier otro handler o test que consuma
JWT en el mismo app domain. (b) `TokenValidationParameters.NameClaimType = "sub"`: no
basta, porque el mapa de entrada ya transformó el claim antes de construir la
`ClaimsIdentity`; el claim remapeado ya no tiene el nombre original.

**Por qué:** `MapInboundClaims = false` es una opción scoped al handler de JWT de esta
app (sin efectos colaterales), mantiene los nombres de claims que documenta el contrato
y hace explícito qué claim representa al sujeto.

**Alcance:** `backend/src/Api/Program.cs` (`AddJwtBearer`), `backend/src/Api/Controllers/AuthController.cs`.

---

## [2026-07-31] — Content-Type `application/problem+json` explícito en `WriteAsJsonAsync`

**Contexto:** `ErrorHandler` y `JwtBearerEvents.OnChallenge` armaban la respuesta de
error seteando `Response.ContentType = "application/problem+json"` y luego llamando a
`WriteAsJsonAsync`. Al probar en runtime, todas las respuestas 4xx/5xx salían con
`Content-Type: application/json; charset=utf-8`: `WriteAsJsonAsync` sobrescribe el
content-type seteado previamente.

**Decisión:** Se pasa `contentType: "application/problem+json"` como argumento a
`WriteAsJsonAsync` en `ErrorHandler` y en `OnChallenge`. Para el 422 de validación,
el `InvalidModelStateResponseFactory` devuelve un `ObjectResult` con
`ContentTypes = { "application/problem+json" }`.

**Alternativa descartada:** Seguir seteando `Response.ContentType` antes de escribir.
Es frágil: depende de que el método de escritura no pise el header. La sobrecarga con
`contentType` fija el content type en el mismo punto donde se escribe el body.

**Por qué:** El contrato exige `Content-Type: application/problem+json` en toda
respuesta de error y las pruebas automáticas lo verifican. El bug solo se detectó con
la verificación HTTP real (la lectura de `Content-Type` en la respuesta, no solo el
código).

**Alcance:** `backend/src/Api/Errores/ErrorHandler.cs`, `backend/src/Api/Program.cs`.

---

## [2026-07-31] — Validación de campos de `[ApiController]` mapeada a 422 `VALIDACION`

**Contexto:** Con `[ApiController]`, el ModelState inválido produce una respuesta 400
automática generada por el framework que **no incluye el campo `codigo`**, violando la
regla del contrato de que `codigo` es obligatorio en toda respuesta de error. La tabla
de códigos define `VALIDACION` (422) para errores de validación de campos.

**Decisión:** Se configuró `ApiBehaviorOptions.InvalidModelStateResponseFactory` en
`Program.cs`: devuelve 422 con `codigo: "VALIDACION"`, `Content-Type
application/problem+json`, y el diccionario `errores` con claves en camelCase
(`JsonNamingPolicy.CamelCase.ConvertName`) para que `Password` → `password` según el
contrato.

**Alternativa descartada:** Aceptar la respuesta 400 por defecto del framework. Se
descartó porque rompe el contrato (errores sin `codigo`) y el 400 no está en la tabla
de códigos para este caso.

**Por qué:** El paso 5 de la fase exige `codigo` obligatorio en todas las respuestas de
error; mapear el ModelState al código `VALIDACION` existente mantiene el contrato sin
inventar un código nuevo.

**Alcance:** `backend/src/Api/Program.cs` (`ApiBehaviorOptions`), aplica a todo request
con validación automática de `[ApiController]`.
