# Fase 03 — Autenticación JWT, /me y /health

## Objetivo

Tener el login con JWT, el endpoint `/me`, `/health` sin autenticación, Swagger con Bearer configurado, CORS para `:5173` y el middleware global de errores en formato `problem+json`.

## Alcance

- `POST /auth/login` y `GET /me` (contrato exacto en [rules/contrato-api.md](../rules/contrato-api.md) §1 y §2).
- `GET /health` → `{ "estado": "ok" }`, sin autenticación.
- JWT HS256, secreto de entorno, expiración 8 h, claims `sub`, `tenantId`, `rol`, `email`.
- Swagger con botón Authorize (Bearer).
- CORS para `http://localhost:5173`.
- Middleware global de errores → `application/problem+json` con `codigo`.

## Pasos

1. **Paquetes** — en Api: `Microsoft.AspNetCore.Authentication.JwtBearer`. En Infraestructura (o Api): BCrypt (`BCrypt.Net-Next`).
2. **Configuración JWT** en `appsettings.json` / variables de entorno:
   - `JWT_SECRET` (secreto HS256, mínimo 32 caracteres).
   - `JWT_ISSUER` y `JWT_AUDIENCE` (valores de ejemplo).
   - Registro de `Authentication:JwtBearer` con validación de emisor/audiencia/clave/expiración.
3. **Servicio de login** en Aplicación:
   - Buscar usuario por email; si no existe o está inactivo → 401 `NO_AUTENTICADO`.
   - Verificar contraseña con BCrypt → 401 `NO_AUTENTICADO`.
   - Generar token con claims `sub` (id), `tenantId`, `rol`, `email`.
   - Respuesta exacta: `accessToken`, `expiraEn: 28800`, `usuario`.
4. **Endpoints**:
   - `POST /auth/login` (anónimo).
   - `GET /me` (autenticado) — mismo objeto `usuario`; leer datos desde el token y devolver también `tenantNombre`.
   - `GET /health` (anónimo) → `{ "estado": "ok" }`.
5. **Middleware de errores** — `IExceptionHandler` o middleware:
   - Toda excepción no controlada → 500 con `problem+json`, **sin stack trace**.
   - Convertir las excepciones de dominio/aplicación a sus códigos (`NO_AUTENTICADO`, `OPERACION_NO_PERMITIDA`, `RECURSO_NO_ENCONTRADO`, `TRANSICION_INVALIDA`, `AGENTE_INVALIDO`, `MOTIVO_REQUERIDO`, `PARAMETRO_INVALIDO`, `VALIDACION`).
   - El campo `codigo` es **obligatorio** en todas las respuestas de error.
6. **Swagger** — esquema de seguridad Bearer configurado (`SwaggerGen` con `SecurityScheme`).
7. **CORS** — permitir `http://localhost:5173`.

## Definición de terminado

- [ ] Login correcto devuelve token + usuario (formato exacto, camelCase).
- [ ] Login con credenciales inválidas → 401 `NO_AUTENTICADO` en `problem+json`.
- [ ] `/me` con token devuelve el usuario; sin token → 401.
- [ ] `/health` responde 200 sin token.
- [ ] Swagger en `/swagger` tiene botón Authorize.
- [ ] Una excepción no controlada devuelve 500 sin stack trace y con `codigo`.
- [ ] `dotnet test` sigue pasando (aún no hay pruebas de esta fase).

## Qué NO hacer en esta fase

- No implementar endpoints de solicitudes/categorías todavía.
- No guardar el token en localStorage sin pensarlo (el frontend lo decide después, pero no afecta esta fase).
- No hardcodear el secreto JWT en código.

## Notas para el registro de decisiones

- Elección: `IExceptionHandler` (nuevo en .NET 8) vs middleware clásico — ambas válidas, decide y justifica.
- `tenantNombre` se obtiene consultando el tenant (no es un claim).

## Commit sugerido

```
feat(auth): implementa login con JWT y endpoint /me

HS256 con secreto en variable de entorno, expiración de 8 horas
y claims sub, tenantId, rol y email. Agrega manejo global de
errores en problem+json con campo codigo obligatorio.
```
