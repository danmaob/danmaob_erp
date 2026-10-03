# ADR-01: Estrategia de almacenamiento multi-tenant

**Estado:** Propuesto (pendiente de aprobación de Luis)
**Fecha:** 2026-10-02
**Historia:** SP-00-1 (Épica E00 — Fundación técnica y arquitectura)
**Requerimientos:** RNF-001, RNF-017, RNF-018, RNF-019, RNF-022, RNF-023
**Depende de:** ADR-00 (catálogo de plataforma y datos operativos)

## Contexto

DANMAOB_ERP espera decenas de cuentas en 2026 y miles en 2027 (RNF-018). El RFP y RNF-019 piden no asumir que todas las empresas vivirán siempre en una sola base de datos. RNF-022 exige que la misma base de código sirva para SaaS, Private Cloud y On-Premise.

El ADR-00 divide los datos en dos áreas lógicas:

- **Catálogo de plataforma:** cuentas, empresas, usuarios, accesos y Control Plane. No lleva filtro por empresa.
- **Datos operativos:** todo lo que pertenece a una empresa. Lleva filtro por empresa.

La fuga de información entre empresas es el riesgo más grave del producto. En TISAX ocurrió porque el método que aplicaba el filtro de tenant quedó definido pero sin invocar.

El release 1 corre primero en el equipo local de Luis (SQL Server en Docker), con un solo desarrollador humano y un ejecutor de 9B. La estrategia debe ser correcta sin agregar complejidad prematura (Instrucciones 17).

## Decisión

### 1. Release 1: una base de datos compartida con identificador de empresa

- Una sola base de datos SQL Server para toda la plataforma.
- Todas las empresas comparten las mismas tablas. Cada fila operativa lleva la columna `TenantId`, no nulable.
- El aislamiento lo impone el filtro global de EF Core sobre toda entidad que implemente la interfaz de pertenencia a empresa, alimentado por la empresa resuelta en la solicitud (ADR-00, sección 4). Si no hay empresa resuelta, el acceso a datos operativos lanza una excepción.
- Las escrituras también se protegen: al guardar, una entidad operativa nueva recibe el `TenantId` de la empresa resuelta. Si una entidad operativa llega con un `TenantId` distinto al de la solicitud, el guardado se rechaza con una excepción. El diseño de este mecanismo corresponde a TS-00-4.

### 2. Separación del catálogo y de los datos operativos dentro de la base

- Hay un solo `DbContext` en el release 1, para mantener una sola historia de migraciones y una sola configuración que verificar.
- Se usan dos esquemas de SQL Server:
  - `platform` para el catálogo de plataforma;
  - `erp` para los datos operativos.

  El esquema hace visible la frontera y deja preparado el camino de separación.
- Las entidades del catálogo forman una **lista explícita de excepciones** al filtro. TS-00-4 incluye una prueba que falla si una entidad fuera de esa lista no implementa la interfaz de pertenencia a empresa, y otra que falla si el método de filtros deja de invocarse (Instrucciones 14.3).

### 3. Reglas de diseño que mantienen abierta la evolución

Estas reglas son obligatorias desde el release 1:

1. **Todas las tablas operativas llevan `TenantId`, incluso las que hoy no lo "necesitan"**, como las tablas hijas de un documento. Así el filtro aplica de forma uniforme y una futura base por empresa conserva la misma estructura.
2. **Los índices únicos de datos operativos incluyen `TenantId` como primera columna.** Por ejemplo, el código de producto es único por (`TenantId`, código) (RF-073). Los índices de consulta frecuentes también empiezan con `TenantId`.
3. **Ninguna llave foránea va de `erp` hacia `platform`, salvo hacia la empresa (`TenantId`) y hacia el usuario** (por ejemplo, "vendedor" o "creado por"). Estas llaves son las únicas que habría que retirar en una separación futura; cualquier otra referencia cruzada requiere un ADR.
4. **Ningún código operativo consulta datos de dos empresas en la misma operación.** Las únicas consultas entre empresas pertenecen al Control Plane y al servicio de identidad, sobre el catálogo.
5. **La cadena de conexión se obtiene desde la configuración en un solo punto de registro del `DbContext`.** No se dispersa por el código. No se construye ahora un mecanismo de conexión por empresa.

### 4. Camino de evolución (no se implementa en el release 1)

| Etapa | Cuándo | Qué cambia |
|---|---|---|
| 1. Base compartida | Release 1 (Free y Básico, SaaS) | Lo descrito en este ADR |
| 2. Base dedicada para empresas específicas | Cuando un cliente Premium o una medición lo justifique | El catálogo indica la base de cada empresa y el `DbContext` operativo resuelve su conexión por empresa. Las tablas operativas no cambian, porque ya llevan `TenantId` |
| 3. Catálogo en base propia | Si la etapa 2 se generaliza | Se separan los esquemas `platform` y `erp` en bases distintas. Se retiran las llaves foráneas de la regla 3 y la integridad de esas referencias pasa a la aplicación |

En Private Cloud y On-Premise se usa el mismo esquema y el mismo código: es una instalación con una sola cuenta.

### 5. Respaldos

El objetivo de pérdida máxima de 1 hora y restablecimiento máximo de 24 horas (RNF-017) se cumple con la base compartida mediante respaldos completos y de registro de transacciones. La configuración concreta se resuelve al publicar en AWS. Restaurar una sola empresa a un punto anterior no es posible sin afectar a las demás; se acepta para el release 1 (ver Consecuencias).

## Alternativas consideradas

- **Base de datos por empresa desde el release 1.** Descartada. Multiplica migraciones, conexiones, respaldos y costo de infraestructura por cada cuenta Free. Contradice "evitar complejidad prematura" y el costo contenido del plan Free.
- **Esquema de SQL Server por empresa.** Descartada. Tiene la complejidad de migración de la opción anterior y escala mal a miles de empresas.
- **Dos `DbContext` (catálogo y operativo) desde el release 1.** Descartada por ahora. Mejora la separación, pero duplica configuración y migraciones, y el registro autónomo necesitaría transacciones que abarquen ambos. Los esquemas `platform` y `erp` y la regla 3 conservan esa opción para la etapa 3.
- **Row-Level Security de SQL Server como mecanismo principal.** Descartada como mecanismo principal porque traslada la lógica de aislamiento a la base y complica la depuración con un ejecutor de 9B. Queda como defensa adicional si una revisión de seguridad lo justifica, en su propio ADR.

## Consecuencias

**Lo que se facilita**

- Una sola base, una sola historia de migraciones y un solo respaldo en el release 1.
- El costo marginal de una cuenta Free es mínimo.
- La estructura de tablas ya es la que necesitaría una base dedicada por empresa.

**Lo que se complica o exige cuidado**

- Todo el aislamiento depende del filtro global y de la protección de escrituras. Por eso sus pruebas (TS-00-4) son obligatorias y forman parte de la Definition of Done de cualquier cambio al `DbContext`.
- Una empresa con mucho volumen afecta el rendimiento de las demás. Se vigila con los tiempos de respuesta objetivo (RNF-020); la salida es la etapa 2.
- No se puede restaurar una sola empresa a un punto anterior. Si un cliente lo necesita, se resuelve exportando sus filas desde una restauración paralela.
- Las llaves foráneas hacia la empresa y el usuario deberán retirarse si se llega a la etapa 3.

**Trabajo que queda desbloqueado**

- **TS-00-3:** `DbContext` único, esquemas `platform` y `erp`, entidad base y primera migración. La estrategia de generación de los identificadores `Guid` y del índice clúster se define ahí, considerando la fragmentación en SQL Server.
- **TS-00-4:** filtro global, protección de escrituras, lista de excepciones y pruebas de aislamiento.
- **US-02-1:** las tablas del catálogo van en el esquema `platform`.
