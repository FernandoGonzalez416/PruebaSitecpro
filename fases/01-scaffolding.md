# Fase 01 — Scaffolding del proyecto

## Objetivo

Dejar el repositorio con la estructura de carpetas obligatoria del enunciado y dos proyectos que compilan y arrancan: la API .NET 8 en `http://localhost:5080` y el frontend Vue 3 en `http://localhost:5173`.

## Alcance

- Limpiar el estado inicial del repo (archivos basura, respaldos locales, carpetas de configuración del editor).
- Crear `.gitignore` correcto (ver checklist abajo).
- Crear `.env.example` con valores de ejemplo.
- Crear la solución .NET con los 4 proyectos de capas + proyecto de tests.
- Crear el proyecto Vue 3 + TypeScript strict + Vite + Pinia + Vue Router.

## Pasos

1. **`.gitignore`** — excluir:
   - `**/bin/`, `**/obj/`
   - `node_modules/`, `dist/`
   - `*.db`, `*.db-shm`, `*.db-wal` (nunca versionar SQLite)
   - `.env` (pero mantener `.env.example`)
   - `appsettings.*.local.json` si se usa
   - `*.user`, `.vs/`, `.vscode/`
2. **`.env.example`** — variables con valores de ejemplo, sin secretos reales:
   ```
   JWT_SECRET=super-secreto-cambiar-en-produccion-de-al-menos-32-caracteres
   SEED_FECHA_BASE=2026-01-15T08:00:00Z
   ```
3. **Backend** — crear solución y proyectos:
   ```powershell
   dotnet new sln -n MesaSitec
   dotnet new webapi -o src/Api -n MesaSitec.Api
   dotnet new classlib -o src/Aplicacion -n MesaSitec.Aplicacion
   dotnet new classlib -o src/Dominio -n MesaSitec.Dominio
   dotnet new classlib -o src/Infraestructura -n MesaSitec.Infraestructura
   dotnet new xunit -o tests/MesaSitec.Tests -n MesaSitec.Tests
   ```
   - Agregar todos al `.sln` y las referencias:
     - Api → Aplicacion, Infraestructura
     - Aplicacion → Dominio
     - Infraestructura → Dominio, Aplicacion
     - Tests → Dominio, Aplicacion, Infraestructura
   - Fijar `TargetFramework net8.0` en los 5 proyectos.
   - Probar que compila: `dotnet build`.
4. **Frontend** — crear con Vite:
   ```powershell
   npm create vite@latest frontend -- --template vue-ts
   ```
   - Instalar `pinia` y `vue-router`.
   - `tsconfig` en modo `strict` (activar `strict: true`).
   - Configurar proxy de Vite: `/api` → `http://localhost:5080`.
   - Crear la estructura `src/{api,components,views,stores,types,router}`.
   - Probar: `npm run dev` y `tsc --noEmit`.

## Definición de terminado

- [ ] `dotnet build` compila toda la solución sin errores.
- [ ] `tsc --noEmit` pasa en `frontend/` sin errores.
- [ ] Existe `.gitignore` sin archivos de salida versionados.
- [ ] Existe `.env.example`.
- [ ] La estructura `backend/src/{Api,Aplicacion,Dominio,Infraestructura}` y `frontend/src/{api,components,views,stores,types,router}` existe.
- [ ] No hay secretos en el repo.

## Qué NO hacer en esta fase

- No escribir lógica de negocio ni endpoints reales.
- No agregar librerías todavía (máquina de estados a mano, sin librerías externas).
- No versionar `.env` ni `.db`.

## Commit sugerido

```
chore: inicializa solución .NET y proyecto Vue con Vite

Estructura de capas Api/Aplicacion/Dominio/Infraestructura
y frontend con TypeScript strict, Pinia y Vue Router.
```
