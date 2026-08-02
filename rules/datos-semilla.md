# Datos semilla — MesaSitec

## Activación

Si la base de datos está vacía al arrancar, la aplicación debe sembrarla automáticamente.

## Variable de entorno

```
SEED_FECHA_BASE=2026-01-15T08:00:00Z
```

Valor por defecto: `2026-01-15T08:00:00Z`.

Todas las fechas de los datos semilla deben generarse como **desplazamientos fijos** respecto a esta fecha base, nunca respecto a `DateTime.UtcNow`.

---

## Organizaciones

| Nombre |
|---|
| Cooperativa Norte |
| Bufete Sur |

---

## Usuarios

Contraseña de todos los usuarios semilla: `Sitec.2026`

| Email | Organización | Rol |
|---|---|---|
| `admin@norte.test` | Cooperativa Norte | Admin |
| `agente1@norte.test` | Cooperativa Norte | Agente |
| `agente2@norte.test` | Cooperativa Norte | Agente |
| `user1@norte.test` | Cooperativa Norte | Solicitante |
| `user2@norte.test` | Cooperativa Norte | Solicitante |
| `admin@sur.test` | Bufete Sur | Admin |
| `user1@sur.test` | Bufete Sur | Solicitante |

---

## Categorías (crear en ambas organizaciones)

| Nombre | slaHoras |
|---|---|
| Incidente | 8 |
| Requerimiento | 40 |
| Consulta | 24 |
| Falla crítica | 4 |

---

## Solicitudes

- **25 solicitudes** en Cooperativa Norte
- **8 solicitudes** en Bufete Sur

### Distribución requerida

| Condición | Cantidad | Organización |
|---|---|---|
| Repartidas entre todos los estados | varias | Cooperativa Norte |
| Repartidas entre todas las prioridades | varias | Cooperativa Norte |
| Vencidas | al menos 5 | Cooperativa Norte |
| Resueltas | al menos 3 | Cooperativa Norte |

### Definición de "vencida" (RN-04)

Una solicitud se considera vencida si `fechaLimiteSla` ya pasó y su estado no es `Resuelta`, `Cerrada` ni `Cancelada`.

Para los datos semilla, calcular `fechaLimiteSla` según la categoría y prioridad de cada solicitud, usando `SEED_FECHA_BASE` como referencia para `fechaCreacion`.
