# Fase 10 — Entrega: README, DECISIONES y limpieza

## Objetivo

Dejar el repositorio listo para entregar: README reproducible en ≤ 4 comandos y < 5 minutos, `DECISIONES.md` de una página, `.env.example`, docker-compose opcional, y pasar el checklist completo de la sección 11 del enunciado.

## Alcance

- `README.md` completo.
- `DECISIONES.md` (máx. 1 página).
- Docker Compose opcional (`docker compose up -d --build`).
- Verificación del checklist de entrega y de los commits.

## Pasos

1. **`README.md`** (sección 8.3):
   - Requisitos previos: versiones de .NET SDK, Node, npm.
   - Cómo levantar el backend (máx. 4 comandos en total para todo el proyecto):
     ```powershell
     # backend
     cd backend
     $env:JWT_SECRET="..."     # o copiar .env.example
     dotnet run --project src/Api
     # frontend (otra terminal)
     cd frontend
     npm install
     npm run dev
     ```
   - La base se migra y siembra sola al arrancar (sin pasos manuales).
   - Credenciales de prueba (los 7 usuarios semilla, contraseña `Sitec.2026`).
   - **Qué está implementado y qué no** — ser honesto; una omisión no declarada resta el doble.
   - URL base `http://localhost:5080/api/v1`, Swagger `/swagger`, frontend `:5173`.
2. **`DECISIONES.md`** (sección 8.4, **máx. 1 página**):
   - Curar el registro de [rules/registro-decisiones.md](../rules/registro-decisiones.md) a las **3 decisiones técnicas** más importantes (alternativa descartada + porqué).
   - Qué se hizo con IA y qué a mano.
   - Qué se haría distinto con una semana más.
   - En qué punto te atascaste y cómo lo resolviste (obligatorio; "no me ataqué" es mala respuesta).
3. **Docker Compose (opcional, suma puntos)**:
   - Contenedores de la API y el frontend.
   - Volumen para el `.db` (no versionar el archivo).
   - `docker compose up -d --build` levanta todo.
4. **Limpieza del repo**:
   - Verificar que no haya `bin/`, `obj/`, `node_modules/`, `.db` versionados.
   - Verificar que no haya secretos (`.env` no versionado; `.env.example` con valores de ejemplo).
   - Revisar que el `.gitignore` cubra todo.
5. **Verificación final — checklist sección 11**:
   - [ ] El proyecto levanta siguiendo el README desde cero en < 5 minutos.
   - [ ] `GET /health` responde 200 sin token.
   - [ ] `/swagger` documenta los 9 endpoints con Bearer.
   - [ ] Los datos semilla se crean solos y las credenciales del README funcionan.
   - [ ] `user1@sur.test` abriendo una solicitud de Norte → 404.
   - [ ] Todos los `data-testid` de la sección 7.4 existen exactos.
   - [ ] Botones no permitidos NO se renderizan.
   - [ ] `paginacion-info` con formato exacto `Página X de Y — Z resultados`.
   - [ ] `tsc --noEmit` sin errores, sin `any` sin justificar.
   - [ ] `dotnet test` verde con ≥ 8 pruebas.
   - [ ] Sin secretos, `bin/`, `obj/`, `node_modules/`, `.db`.
   - [ ] README y DECISIONES completos y honestos.
   - [ ] ≥ 8 commits con mensajes significativos.
6. **Conceder acceso** al repositorio a `osanchezm` en GitHub.

## Verificación del requisito eliminatorio

Probar en limpio (sin base previa): borrar la carpeta de salida del backend (y el `.db`), clonar el repo en una carpeta nueva y seguir el README de punta a punta, cronometrando. Si no levanta en < 5 minutos con ≤ 4 comandos, corregir el README o el código.

## Definición de terminado

- [ ] Checklist completo de la sección 11 marcado en verde.
- [ ] `dotnet test` verde.
- [ ] `tsc --noEmit` sin errores.
- [ ] `git log --oneline` muestra ≥ 8 commits significativos que se lean como una narrativa.
- [ ] README honesto (incluye lo no implementado, si existe).
- [ ] DECISIONES.md de 1 página con los 4 puntos de la sección 8.4.

## Qué NO hacer en esta fase

- No refactorizar código (fase de documentación y verificación).
- No apurar los últimos 20 minutos: reservar tiempo real para esta fase.

## Commit sugerido

```
docs: agrega README con instrucciones de levantamiento y DECISIONES

Documenta requisitos, comandos, credenciales de prueba y decisiones
tecnicas. Declara lo implementado y lo pendiente.
```
