# Conventional Commits — HelpDesk

## Formato obligatorio

```
<tipo>(<alcance opcional>): <descripción corta en imperativo>

[cuerpo opcional — explica el porqué, no el qué]

[footer opcional — BREAKING CHANGE, referencias a issues]
```

La línea de asunto no debe exceder 72 caracteres. El cuerpo se separa del asunto con una línea en blanco.

---

## Tipos permitidos

| Tipo | Cuándo usar | Ejemplo en este proyecto |
|---|---|---|
| `feat` | Nueva funcionalidad visible para el usuario o la API | `feat(auth): implementa login con JWT` |
| `fix` | Corrección de un bug | `fix(solicitudes): corrige filtro por estado devolviendo 400` |
| `refactor` | Cambio de código que no altera comportamiento externo | `refactor(sla): extrae cálculo a servicio de dominio` |
| `test` | Agregar, corregir o reorganizar pruebas | `test(transiciones): agrega prueba para reasignación RN-02` |
| `docs` | Cambios solo en documentación (README, DECISIONES, rules/) | `docs: agrega regla de conventional commits` |
| `chore` | Configuración, dependencias, tooling, .gitignore | `chore: configura .gitignore para bin/ y node_modules/` |
| `style` | Formato, espacios, punto y coma, sin cambio de lógica | `style: aplica prettier a todo el frontend` |

---

## Alcances sugeridos

Basados en la arquitectura del proyecto:

| Alcance | Cubre |
|---|---|
| `auth` | Login, JWT, /me, guard de rutas |
| `tenant` | Filtro multi-tenant (RN-01), contexto de organización |
| `solicitudes` | CRUD de solicitudes (endpoints, vistas, stores) |
| `sla` | Cálculo y recálculo del SLA (RN-04) |
| `transiciones` | Máquina de estados y endpoint /transiciones (RN-02) |
| `categorias` | Endpoint de categorías, seed |
| `frontend` | Cambios generales de UI que no encajan en otro alcance |
| `api-client` | Módulo HTTP centralizado, interceptor de token |
| `tests` | Pruebas unitarias o de integración |
| `seed` | Datos semilla, migraciones automáticas |
| `docker` | Dockerfile, docker-compose, configuración de contenedores |

---

## Reglas específicas del proyecto

### 1. Referenciar reglas de negocio

Cada commit relacionado con una regla de negocio (RN-01 a RN-07) debe referenciarla explícitamente en el cuerpo del mensaje:

```
feat(transiciones): implementa máquina de estados

Valida todas las transiciones según la tabla de RN-02.
Lanza 409 con TRANSICION_INVALIDA para transiciones no permitidas.
```

### 2. No mezclar backend y frontend

Prohibido mezclar cambios de backend y frontend en un mismo commit, salvo que sea un cambio atómico que no pueda separarse (en ese caso, justificarlo en el cuerpo).

```
# INCORRECTO
feat: agrega endpoint y pantalla de login

# CORRECTO (separados)
feat(auth): implementa POST /auth/login
feat(frontend): implementa vista de login con formulario
```

### 3. Sin "initial commit" único

El enunciado exige mínimo 8 commits significativos. Queda prohibido el commit único "initial commit" con todo el proyecto. Los commits deben reflejar el proceso de construcción, no solo el resultado final.

### 4. Narrativa de desarrollo

Los commits deben leerse como una narrativa. Alguien que ejecute `git log --oneline` debe entender en qué orden se construyó el sistema. No intentar comprimir funcionalidades enteras en un solo commit.

```
# Buena narrativa
feat(model): define entidades Tenant, Usuario, Categoria, Solicitud
feat(auth): implementa login con JWT y endpoint /me
feat(tenant): agrega filtro multi-tenant a consultas (RN-01)
feat(transiciones): implementa máquina de estados (RN-02)
feat(sla): implementa cálculo de fechaLimiteSla (RN-04)
feat(solicitudes): implementa CRUD de solicitudes con paginación server-side
feat(frontend): implementa listado con filtros y paginación
test(transiciones): agrega pruebas unitarias para RN-02 y RN-04
```

---

## Ejemplos

### Correctos

```
feat(auth): implementa POST /auth/login con JWT

Usa HS256 con secreto desde variable de entorno.
El token expira en 8 horas e incluye sub, tenantId, rol, email.
```

```
fix(solicitudes): corrige filtro vencidas ignorando estados finales

El filtro ?vencidas=true ahora excluye solicitudes en estado
Resuelta, Cerrada o Cancelada según RN-04.
```

```
refactor(sla): extrae cálculo a servicio de dominio

Mueve la lógica de factor × slaHoras desde el controlador
a Dominio/Services/SlaCalculator para poder probarlo sin la API.
```

```
test(transiciones): agrega 3 pruebas para transiciones inválidas RN-02

Cubre: Nueva→Resuelta, Cerrada→cualquiera, y
solicitud inexistente lanzando RECURSO_NO_ENCONTRADO.
```

```
docs: agrega regla de conventional commits y registro de decisiones
```

```
chore: configura .gitignore y .env.example

Excluye bin/, obj/, node_modules/, archivos .db y appsettings locales.
```

### Incorrectos

```
# MAL: sin tipo
implementa login

# MAL: mensaje vago
fix: arregla cosas

# MAL: mezcla backend y frontend sin justificar
feat: agrega CRUD de solicitudes completo

# MAL: no referencia RN cuando corresponde
feat: agrega filtro por tenant en consultas

# MAL: asunto demasiado largo (más de 72 caracteres)
feat(solicitudes): implementa el listado paginado con filtros por estado prioridad categoria agente busqueda y vencidas

# MAL: explica el qué en vez del porqué en el cuerpo
feat(transiciones): agrega validación de RN-02

Agregué un switch con los estados y acciones permitidas.
```
