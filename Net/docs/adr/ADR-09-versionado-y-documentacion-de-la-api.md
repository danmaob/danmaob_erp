# ADR-09: Versionado y documentación del contrato de la API

**Estado:** Aceptado (2026-10-09)
**Fecha:** 2026-10-09
**Historia:** TS-00-6 (Épica E00 — Fundación técnica y arquitectura)
**Requerimientos:** RNF-030, RNF-034, RNF-040
**Depende de:** ADR-02 (la API se versiona porque la app móvil no se actualiza al ritmo de la web), ADR-08 (contrato de errores)

## Contexto

La API REST es el único acceso a la lógica de negocio. La consumen la web y, después, la app MAUI, cuyas versiones instaladas conviven con la API publicada durante meses. Por eso el contrato necesita una versión explícita desde el primer endpoint, y una documentación que liste los endpoints con sus contratos.

## Decisión

### 1. Versión en la ruta

- Todo endpoint de negocio vive bajo `api/v{n}`. Hoy solo existe `v1`, en la constante `ApiRoutes.V1` (`api/v1`) de `src/DanmaobErp.Api/Routing/`.
- Cada controlador del producto declara su ruta a partir de esa constante. Una prueba falla si un controlador del proyecto Api tiene una ruta que no empieza con `api/v`.
- Los endpoints de infraestructura, como `/health`, quedan fuera del versionado.
- No se usa una librería de versionado mientras exista una sola versión. Si llega `v2`, se evalúa `Asp.Versioning`, tomando como referencia la versión que ya usa DANMAOB_TISAX (10.2.x), para no mezclar versiones en el mismo equipo.

### 2. Política de cambios

- Un cambio compatible (un campo opcional nuevo, un endpoint nuevo) se publica en la versión vigente.
- Un cambio incompatible (quitar o renombrar un campo, cambiar un tipo o una regla de respuesta) exige una versión nueva.
- Una versión se mantiene mientras haya versiones de la app móvil en uso que la consuman.

### 3. Documentación

- El documento OpenAPI `v1` lo genera `Microsoft.AspNetCore.OpenApi` (ya presente), sin dependencias nuevas. Una clase propia le pone el título "DANMAOB ERP API", la versión "v1" y una descripción.
- Todos los endpoints declaran como respuestas comunes las de ADR-08, con `application/problem+json`: **422** (`ValidationProblemDetails`) y **500** (`ProblemDetails`). Las respuestas 401, 403 y 404 se declaran en cada endpoint cuando aplican.
- El documento se publica en `/openapi/v1.json` **solo en desarrollo**. No hay visor integrado: el JSON se abre con cualquier herramienta compatible con OpenAPI.

## Alternativas consideradas

- **Versión en un encabezado o en la query.** Descartada: es menos visible al depurar y menos directa para la app móvil.
- **`Asp.Versioning` desde ahora.** Pospuesta: con una sola versión no aporta y suma paquetes.
- **Visor integrado (Scalar o Swagger UI).** Descartado por ahora para no sumar dependencias.
- **Declarar 401, 403 y 404 en todos los endpoints.** Descartado: el documento anunciaría respuestas que algunos endpoints nunca dan.

## Consecuencias

**Lo que se facilita**

- La app móvil puede fijar su versión del contrato desde el primer día.
- La documentación se genera del código, así que no se desactualiza.

**Lo que exige cuidado**

- Toda ruta nueva debe partir de `ApiRoutes`; la prueba de rutas lo vigila.
- La documentación no está disponible en producción: para revisarla se levanta la API en desarrollo.
