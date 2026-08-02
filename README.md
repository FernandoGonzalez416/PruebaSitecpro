# MesaSitec

Mesa de servicio SaaS **multi-tenant**: varias organizaciones comparten la misma instancia sin que los datos de una se filtren a otra. Solicitudes de soporte con máquina de estados, cálculo automático de SLA y control de acceso por rol.

- **Backend:** .NET 8 Web API · EF Core · SQLite · JWT (HS256)
- **Frontend:** Vue 3 · TypeScript `strict` · Pinia · Vue Router · Vite
- **Arquitectura:** `backend/src/{Api,Aplicacion,Dominio,Infraestructura}` (lógica de negocio en `Dominio/`)

## Requisitos previos

| Herramienta | Versión mínima |
|---|---|
| .NET SDK | 8.0 |
| Node.js | 20.19+ (o 22.12+) |
| npm | 10+ |

No se requiere SQLite instalado: la base es un archivo local que se migra y siembra solo al arrancar.

## Levantamiento (4 comandos, < 5 minutos)

Dos terminales. La base de datos se migra y siembra automáticamente en el primer arranque (`mesasitec.db` junto a la API). No hay pasos manuales.

```powershell
# Terminal 1 — Backend (puerto 5080)
cd backend
dotnet run --project src/Api

# Terminal 2 — Frontend (puerto 5173)
cd frontend
npm install; npm run dev
```

> `npm install` solo corre la primera vez; en corridas siguientes basta `npm run dev`. En PowerShell, `;` separa comandos en la misma línea (en cmd.exe usar `&`).

**Variables de entorno.** La app lee `JWT_SECRET`, `JWT_ISSUER`, `JWT_AUDIENCE` y `SEED_FECHA_BASE` desde variables de entorno (`.env.example` documenta los valores de ejemplo; la app no lee un archivo `.env`). `JWT_SECRET` es **obligatoria en producción** (mínimo 32 caracteres; si falta, la API falla al iniciar) y **opcional en desarrollo**: sin definirla se usa el fallback de `appsettings.Development.json` (solo desarrollo, nunca desplegar). Para definirla en PowerShell antes de `dotnet run`:

```powershell
$env:JWT_SECRET="super-secreto-cambiar-en-produccion-de-al-menos-32-caracteres"
```

## URLs

| Recurso | URL |
|---|---|
| API base | `http://localhost:5080/api/v1` |
| Swagger (solo Development) | `http://localhost:5080/swagger` |
| Frontend | `http://localhost:5173` |

## Credenciales de prueba

Contraseña de todos los usuarios semilla: **`Sitec.2026`**

| Email | Organización | Rol |
|---|---|---|
| `admin@norte.test` | Cooperativa Norte | Admin |
| `agente1@norte.test` | Cooperativa Norte | Agente |
| `agente2@norte.test` | Cooperativa Norte | Agente |
| `user1@norte.test` | Cooperativa Norte | Solicitante |
| `user2@norte.test` | Cooperativa Norte | Solicitante |
| `admin@sur.test` | Bufete Sur | Admin |
| `user1@sur.test` | Bufete Sur | Solicitante |

Semilla: 2 organizaciones, 4 categorías por org, 25 solicitudes en Cooperativa Norte y 8 en Bufete Sur (repartidas entre estados, prioridades, con vencidas y resueltas).

## Qué está implementado

- **Autenticación JWT:** `POST /auth/login`, `GET /me`, `GET /health` (sin token).
- **Categorías:** `GET /categorias` filtrado por tenant.
- **Solicitudes:** listado paginado/filtrado 100% server-side (`estado`, `prioridad`, `categoriaId`, `agenteId`, `q`, `vencidas`, `sort`), crear, detalle, editar y transiciones (`asignar`, `iniciar`, `resolver`, `cerrar`, `reabrir`, `cancelar`).
- **Reglas de negocio en `Dominio/`:** máquina de estados (RN-02) como diccionario, permisos por rol × estado (RN-03), SLA con recálculo por prioridad/categoría (RN-04), aislamiento multi-tenant (RN-01, recursos ajenos → 404), asignación validada (RN-05), motivos mínimos (RN-06), código `SOL-{año}-{correlativo5}` por org/año (RN-07).
- **Errores:** toda respuesta 4xx/5xx en `application/problem+json` con campo `codigo` obligatorio.
- **Frontend:** login, listado con filtros y paginación, detalle con acciones por estado/rol (botones no permitidos no se renderizan), crear/editar con validación en cliente y mapeo de errores de la API.
- **Pruebas:** 56 pruebas xUnit (máquina de estados, SLA, permisos, generador de código, edición con recálculo de SLA, reasignación y validación de paginación). `dotnet test` en verde.

## Qué NO está implementado (declaración honesta)

- **Listado de agentes:** no existe endpoint `GET /agentes` (el contrato fija 9 rutas exactas). El modal de asignación usa una **constante en el frontend** (`src/api/agentes.ts`) con los usuarios `Admin`/`Agente` de la semilla, filtrada por el `tenantId` del token. Es suficiente para la demo; en producción vendría de un endpoint de directorio de usuarios.
- **Correlativo RN-07 ante concurrencia:** se obtiene contando las solicitudes del año en el rango `[año, año+1)`. No es infalible ante dos creaciones simultáneas, pero el enunciado lo declara fuera de alcance.
- **Filtro `agenteId`:** está soportado por la API, pero el frontend no expone un filtro por agente en el listado.
- **Pruebas E2E automatizadas del frontend:** la funcionalidad se verificó con `vue-tsc`, build y revisión manual de los `data-testid`; no hay suite Playwright/Cypress.

## Verificación rápida

```powershell
# Backend: pruebas unitarias
dotnet test

# Frontend: typecheck estricto sin any explícito
cd frontend
npx vue-tsc --noEmit
```

## Documentación

- `rules/` — reglas de negocio (RN-01..RN-07), contrato de API, datos semilla, frontend (`data-testid`), testing y conventional commits.
- `DECISIONES.md` — las 3 decisiones técnicas más importantes con alternativas descartadas.
- `fases/` — plan de trabajo por fases y cómo se ejecutó cada una.
