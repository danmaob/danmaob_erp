# ADR-07: Aislamiento por empresa: resolución, filtro y protección de escrituras

**Estado:** Aceptado (2026-10-04)
**Fecha:** 2026-10-04
**Historia:** TS-00-4 (Épica E00 — Fundación técnica y arquitectura)
**Requerimientos:** RNF-001, RNF-002
**Depende de:** ADR-00 (empresa activa en el token), ADR-01 (filtro global, protección de escrituras y lista de excepciones), ADR-06 (filtros con nombre)

## Contexto

El ADR-00 decidió que la empresa activa viaja en el token de acceso, y el ADR-01 que el aislamiento lo imponen un filtro global de EF Core y una protección de escrituras. TS-00-4 implementa ese aislamiento antes de que exista la autenticación (E02, ADR-02), y debe hacerlo sin ningún mecanismo temporal inseguro (Instrucciones 14.5).

Quedaban abiertos cuatro puntos: cómo se alimenta la empresa resuelta mientras no hay tokens, cómo se llama el claim, dónde vive la protección de escrituras y cómo se controla que ninguna entidad operativa quede sin filtro.

## Decisión

### 1. Pertenencia a empresa

- Toda entidad operativa implementa `ITenantOwned` (`DanmaobErp.Domain.Common`), con `Guid TenantId` de solo lectura pública (`private set` en la entidad).
- La entidad nunca asigna su propio `TenantId`: lo asigna la protección de escrituras al guardarla por primera vez.
- No hay una clase base de pertenencia, porque no todas las entidades operativas tienen baja lógica.

### 2. Empresa resuelta

- `ITenantContext` (`DanmaobErp.Application.Tenancy`) es la única fuente de la empresa de la solicitud. Su método `RequireTenantId()` devuelve la empresa o lanza `TenantNotResolvedException`. Nunca devuelve un valor por defecto.
- En la API, `HttpTenantContext` (alcance por solicitud) lee el claim `tenant_id` del usuario autenticado. El nombre del claim es la constante `TenantClaimTypes.TenantId`, que E02 usará al emitir el token.
- Mientras no exista autenticación, el usuario de la solicitud nunca tiene ese claim, así que todo acceso a datos operativos lanza la excepción. Es el comportamiento definitivo, no un mecanismo temporal: empieza a resolver la empresa en cuanto E02 emita tokens. Un cliente no puede inyectar el claim: sin un token firmado y validado, el usuario de la solicitud está vacío.
- Se descartan, por inseguros, una empresa por defecto y una empresa en un encabezado enviado por el cliente.

### 3. Filtro de empresa

- El filtro con nombre `Tenant` (`TenantQueryFilter`) compara `TenantId` con la propiedad `CurrentTenantId` del `ErpDbContext`, que llama a `RequireTenantId()`. EF Core la evalúa en cada consulta.
- Se aplica en `OnModelCreating` como la última instrucción, después de `EntityKeyConfiguration` y `SoftDeleteQueryFilter`.
- Ignorar el filtro `SoftDelete` por nombre conserva el filtro `Tenant`. Ignorar el filtro `Tenant` solo es válido en los servicios del catálogo de plataforma, nunca en un módulo operativo.

### 4. Protección de escrituras

- `ErpDbContext` sobrescribe `SaveChanges(bool)` y `SaveChangesAsync(bool, CancellationToken)`. Ambas llaman a un único método, `TenantWriteGuard.Apply`, antes de guardar (Instrucciones 14.4).
- Se prefirió la sobrescritura a un `SaveChangesInterceptor`: un interceptor que no se registra deja las escrituras sin protección sin que nada falle, que es el mismo tipo de error que provocó la fuga de TISAX. La sobrescritura no se puede omitir.
- Reglas, solo para entidades operativas agregadas, modificadas o borradas:
  - Una entidad nueva sin `TenantId` recibe la empresa resuelta.
  - Una entidad nueva con otra empresa se rechaza con `CrossTenantWriteException`.
  - Una entidad modificada o borrada de otra empresa, o a la que se le cambió el `TenantId`, se rechaza con `CrossTenantWriteException`.
- Si el guardado no incluye entidades operativas, no se exige empresa. Así funciona el registro autónomo, que solo escribe en el catálogo.

### 5. Lista de excepciones del catálogo

- `PlatformCatalog.EntityTypes` (`DanmaobErp.Infrastructure.Persistence`) enumera las entidades del catálogo, que no llevan filtro de empresa. Hoy está vacía; US-02-1 agrega las primeras.
- Una prueba recorre el modelo real de `ErpDbContext` y falla si una entidad no implementa `ITenantOwned` ni está en la lista. Otra prueba falla si una entidad operativa no tiene el filtro `Tenant` en el modelo.

## Alternativas consideradas

- **Empresa por defecto o en un encabezado mientras no hay autenticación.** Descartada: cualquier cliente podría leer los datos de otra empresa.
- **`SaveChangesInterceptor`.** Descartada por el riesgo de no registrarlo (sección 4).
- **Clase base de pertenencia a empresa.** Descartada: obligaría a combinar la baja lógica con la pertenencia en una sola jerarquía.

## Consecuencias

**Lo que se facilita**

- El aislamiento queda completo y probado antes de la primera entidad operativa.
- E02 solo tiene que emitir el claim `tenant_id`; ninguna pieza de este ADR cambia.

**Lo que se complica o exige cuidado**

- EF Core puede envolver la excepción de la empresa no resuelta al evaluar una consulta. El manejo de errores (TS-00-5) debe reconocerla también como excepción interna.
- Cada consulta operativa llama a `RequireTenantId()`. Es una lectura de un claim, sin costo relevante.
- Toda entidad del catálogo debe agregarse de forma deliberada a `PlatformCatalog.EntityTypes`.
