# ADR-13: Framework de auditoría

**Estado:** Aceptado (2026-10-11)
**Fecha:** 2026-10-11
**Historia:** TS-14-1 (Épica E14 — Auditoría)
**Requerimientos:** RF-144, RF-145, RF-146, RF-147, RF-148
**Depende de:** ADR-01 (esquemas `platform` y `erp`), ADR-03 (auditoría de plataforma), ADR-05 y ADR-12 (eventos y unidad de trabajo), ADR-06 (entidades), ADR-07 (aislamiento por empresa), ADR-11 (reloj de negocio)

## Contexto

RF-144 pide que cada registro de auditoría diga quién, cuándo, sobre qué entidad, qué operación, el estado anterior y posterior y la empresa. RF-145 fija lo mínimo que se audita y RF-147 exige enmascarar los datos sensibles. La auditoría se captura igual en todos los planes; el plan solo limita la ventana de consulta (RF-148, US-14-1).

Había una contradicción. El ADR-05 y la nota del backlog de TS-14-1 decían que la auditoría usaría un `SaveChangesInterceptor`, pero el ADR-07 descartó los interceptores para la protección de escrituras: un interceptor que no se registra deja de actuar sin que nada falle. Este ADR la resuelve.

Quedaban abiertos además: cómo se identifica a quien hizo la operación mientras no hay autenticación, cómo se audita el registro autónomo sin empresa resuelta y cómo se decide qué entidades se auditan.

## Decisión

### 1. Captura dentro de las sobrescrituras de `SaveChanges`

- La auditoría se captura en las dos sobrescrituras de `SaveChanges` de `ErpDbContext`, no con un interceptor. Es la misma razón del ADR-07: la sobrescritura no se puede omitir.
- Las dos sobrescrituras llaman a un único método privado compartido, que aplica en este orden:
  1. **Inmutabilidad:** si un registro de auditoría aparece modificado o borrado, lanza `InvalidOperationException` y no se guarda nada.
  2. **Protección de empresa** (`TenantWriteGuard`, ADR-07), que además asigna la empresa a las entidades nuevas.
  3. **Captura:** agrega al rastreador de cambios un registro de auditoría por cada entidad auditable creada, modificada o borrada.
  4. **Guardado.**
- La publicación de eventos sigue fuera del `DbContext`, en la unidad de trabajo, después del guardado (ADR-12).
- La auditoría queda en la misma transacción que el cambio: se guardan los dos o ninguno. Si la auditoría falla, la operación falla.
- Si el guardado falla después de la captura, los registros agregados quedan en el rastreador. El contexto que falló no se reutiliza; la unidad de trabajo deja pasar la excepción.

### 2. Quién hizo la operación: el actor actual

- `ICurrentActor` (`DanmaobErp.Application.Identity`) expone el tipo de actor y su identificador anulable.
- `ActorType` (`DanmaobErp.Domain.Auditing`) tiene cuatro valores:
  - **`Anonymous`:** solicitud sin usuario, como el registro autónomo;
  - **`User`:** usuario de empresa;
  - **`PlatformAdmin`:** Super Admin;
  - **`System`:** proceso sin solicitud, como la siembra del Super Admin al arrancar.
- El tipo existe desde ahora por dos razones: RF-146 exige distinguir el uso del permiso de soporte de DANMAOB de la operación de un usuario de la empresa, y el Super Admin no es un `User` (ADR-03). Agregarlo después exigiría otra migración sobre las tablas de auditoría.
- En la API, `HttpCurrentActor` (alcance por solicitud):
  - lee el claim `user_id` (`ActorClaimTypes.UserId`), que E02 emitirá en el token;
  - con un claim válido el actor es `User`;
  - con solicitud pero sin claim válido, `Anonymous`;
  - sin solicitud, `System`.
- `PlatformAdmin` lo agrega la historia del token de plataforma (E01) sin cambiar este contrato.

### 3. Qué se audita

- Una entidad auditable implementa la interfaz `IAuditable` (`DanmaobErp.Domain.Common`), que solo exige su `Id`.
- Una entidad que no se audita se agrega a `AuditExclusions.EntityTypes` (`DanmaobErp.Infrastructure.Auditing`).
- Una prueba del modelo real falla si alguna entidad no está en ninguno de los dos lados. Olvidar auditar una entidad deja de ser una falla silenciosa; cada entidad nueva obliga a una decisión explícita.
- Las tablas de auditoría están en la lista de excluidas.

### 4. Datos sensibles

- El atributo `[Sensitive]` (`DanmaobErp.Domain.Common`) marca la propiedad, junto a su declaración.
- Su valor se escribe siempre como `***`, incluso si es nulo, y nunca revela su longitud. Un cambio de contraseña queda registrado como "cambió", sin valores.

### 5. Estado anterior y posterior

- JSON con el nombre de cada propiedad como clave, los enums como texto y caracteres del español sin escapar.

| Operación | Estado anterior | Estado posterior |
|---|---|---|
| `Created` | nulo | todas las propiedades |
| `Updated` | solo las propiedades modificadas, con su valor original | solo las propiedades modificadas, con su valor nuevo |
| `Deleted` | todas las propiedades | nulo |

- Las propiedades sombra (como `ClusterKey`) no se escriben.
- Una baja lógica es un `Updated` en el que `IsActive` pasa de verdadero a falso.
- El tipo de entidad es el nombre de su clase.

### 6. Dos tablas, según dónde vive la entidad auditada

- **`erp.TenantAuditEntries`** registra los cambios de las entidades operativas (`ITenantOwned`). La empresa es obligatoria y se toma de la propia entidad, ya protegida por `TenantWriteGuard`. La tabla implementa `ITenantOwned`, así que hereda el filtro de empresa en las lecturas (US-14-1).
- **`platform.PlatformAuditEntries`** registra los cambios del catálogo: cuentas, empresas, usuarios, accesos, planes y acciones del Super Admin. La empresa es anulable. La entidad está en `PlatformCatalog`.
- Esta separación permite auditar el registro autónomo, que solo toca el catálogo, sin exigir una empresa resuelta. También respeta el ADR-01: la auditoría operativa no queda en el catálogo, que en la etapa 3 podría vivir en otra base.
- Las dos comparten la base abstracta `AuditEntry` (`DanmaobErp.Domain.Auditing`): identificador (Guid v7), fecha UTC (`IBusinessClock.UtcNow`), tipo e identificador del actor, tipo e identificador de la entidad, operación y estados anterior y posterior.
- Índices: empresa con fecha, y tipo con identificador de entidad, en la operativa; fecha, y tipo con identificador de entidad, en la de plataforma.
- **Pendiente para US-02-1:** a qué empresa se asocian, en la auditoría de plataforma, los cambios de `User` y `TenantMembership`, para que el administrador los vea en su empresa. Hasta entonces la columna queda vacía.

### 7. La auditoría nunca se borra ni se modifica

- Lo impide el paso 1 de la sección 1, y una prueba lo verifica.
- Las entidades de auditoría no son de baja lógica.
- La ventana de 90 días o un año es solo de consulta (US-14-1); los registros siempre se conservan.

## Alternativas consideradas

- **`SaveChangesInterceptor`.** Descartada por la razón del ADR-07: si no se registra, deja de auditar sin que nada falle. Esto precisa al ADR-05 y a la nota del backlog de TS-14-1.
- **Captura en la unidad de trabajo (ADR-12).** Descartada. Un guardado directo con `ErpDbContext` quedaría sin auditar.
- **Una sola tabla con empresa anulable.** Descartada. Mezcla la auditoría operativa con la del catálogo, impide el filtro automático de empresa y dificulta la separación de la etapa 3 del ADR-01.
- **Auditar todo salvo lo marcado como excluido, sin prueba.** Descartada. Las tablas de alto volumen sin valor de auditoría (tokens, contadores) llenarían la tabla, y una exclusión olvidada no se nota. La lista explícita más la prueba obliga a decidir.
- **Guardar el estado completo en cada cambio.** Descartada. Multiplica el volumen sin ayudar a leer qué cambió.
- **Solo un identificador de usuario, sin tipo de actor.** Descartada por RF-146 y por el Super Admin (sección 2).

## Consecuencias

**Lo que se facilita**

- Ningún módulo construye su propia trazabilidad: marcar la entidad con `IAuditable` basta.
- El registro autónomo y las acciones del Super Admin se auditan sin empresa resuelta.
- La consulta por empresa (US-14-1) hereda el aislamiento del ADR-07.

**Lo que se complica o exige cuidado**

- El constructor de `ErpDbContext` recibe el actor actual y el reloj. Las pruebas usan `TestErpDbContext`, que conserva su constructor anterior con dobles fijos.
- Cada entidad nueva debe declararse auditable o excluida; si no, la prueba del modelo falla.
- Cada propiedad sensible debe llevar `[Sensitive]`; la revisión de código de cada entidad nueva lo comprueba.
- Las tablas de auditoría crecen sin límite. La retención legal y el archivado se deciden con el abogado (Requerimientos, sección 17).

**Trabajo que queda desbloqueado**

- **US-02-1:** las entidades del catálogo se marcan como auditables desde su creación.
- **US-01-7 y US-14-1:** consulta y exportación sobre las dos tablas.
- **RF-146:** el acceso de soporte se distingue por el tipo de actor.
