# ADR-06: Persistencia base: entidades, identificadores y bajas lógicas

**Estado:** Aceptado (2026-10-03)
**Fecha:** 2026-10-03
**Historia:** TS-00-3 (Épica E00 — Fundación técnica y arquitectura)
**Requerimientos:** RNF-014, RNF-015, RNF-018
**Depende de:** ADR-01 (base compartida, un solo `DbContext`, esquemas `platform` y `erp`)

## Contexto

El ADR-01 dejó para TS-00-3 dos decisiones: cómo se generan los identificadores `Guid` y qué índice es el clúster de cada tabla, considerando la fragmentación en SQL Server. Además, el producto usa bajas lógicas en lugar de borrado físico (RNF-014), y varias reglas de negocio piden ofrecer la reactivación de un registro dado de baja (RF-025, RF-065, RF-074).

SQL Server ordena el tipo `uniqueidentifier` empezando por sus últimos bytes. Un Guid versión 7 tiene la marca de tiempo al inicio, así que SQL Server lo inserta en posiciones aleatorias, igual que un Guid versión 4. Si la llave primaria fuera el índice clúster, cada inserción fragmentaría la tabla.

## Decisión

### 1. Identificador de las entidades

- Toda entidad deriva de la clase abstracta `Entity` (`DanmaobErp.Domain.Common`), que tiene `Guid Id` sin `set` público.
- El `Id` se asigna en el constructor con `Guid.CreateVersion7()`. La entidad tiene identidad desde que se crea, antes de guardarse, lo que necesitan las relaciones y los eventos de negocio (ADR-05).
- EF Core nunca genera el `Id` (`ValueGeneratedNever`).

### 2. Llave primaria e índice clúster

- La llave primaria sobre `Id` es **no clúster**.
- Cada tabla tiene una columna sombra `ClusterKey` (`bigint`, identidad), sin propiedad en el dominio, con un **índice único clúster**. Las inserciones van siempre al final del índice clúster.
- Una configuración del modelo, `EntityKeyConfiguration`, aplica estas reglas a toda entidad raíz que derive de `Entity`. Ninguna configuración de entidad las repite.
- `ClusterKey` es un detalle físico: nunca se expone en la API, en el dominio ni en las relaciones.

### 3. Bajas lógicas

- Las entidades con baja lógica derivan de `SoftDeletableEntity`, que agrega `IsActive` (verdadero al crearse) y los métodos `Deactivate()` y `Reactivate()`.
- El filtro global con nombre `SoftDelete` (configuración `SoftDeleteQueryFilter`) excluye las entidades inactivas de toda consulta.
- Para ofrecer la reactivación, la consulta ignora **solo** el filtro `SoftDelete`, por su nombre. Nunca se usa `IgnoreQueryFilters()` sin nombres: también apagaría el filtro de empresa de TS-00-4 y provocaría una fuga de información entre empresas.

### 4. DbContext y configuración

- Un solo `ErpDbContext` en `src/DanmaobErp.Infrastructure/Persistence/`, como lo pide el ADR-01. Las constantes de esquema están en `DatabaseSchemas` (`platform` y `erp`).
- `OnModelCreating` aplica primero las configuraciones de cada entidad y al final las configuraciones del modelo (`EntityKeyConfiguration` y `SoftDeleteQueryFilter`). TS-00-4 agrega el filtro de empresa en ese mismo bloque final.
- La cadena de conexión es la variable `ConnectionStrings__Erp`, obligatoria al arrancar (`RequiredConfigurationKeys`) y registrada en un solo punto: `AddPersistence`.
- Los paquetes de EF Core son la versión 10.0.12, la misma probada en el equipo de Luis.
- Las migraciones viven en `src/DanmaobErp.Infrastructure/Persistence/Migrations/`. `.editorconfig` las marca como código generado, porque EF Core las escribe con namespace de bloque.

## Alternativas consideradas

- **Llave primaria clúster sobre el Guid.** Descartada: un Guid versión 7 fragmenta igual que uno aleatorio en SQL Server.
- **Guid secuencial generado por EF Core al insertar.** Descartada: la entidad no tendría `Id` hasta guardarse, lo que rompe el constructor válido y los eventos de negocio.
- **Generar en el dominio un Guid ordenado para SQL Server.** Descartada: pone conocimiento de la base de datos en la capa de dominio.
- **Llave primaria `bigint` en lugar de Guid.** Descartada: el `Id` dejaría de existir antes de guardar y sería predecible en la API.

## Consecuencias

**Lo que se facilita**

- Inserciones sin fragmentación del índice clúster.
- Identidad estable desde el constructor.
- Consultas de reactivación seguras, sin apagar otros filtros.

**Lo que se complica o exige cuidado**

- Cada tabla tiene un índice adicional (la llave primaria no clúster) y 8 bytes más por fila.
- Las búsquedas por `Id` usan el índice no clúster más una búsqueda en el clúster. Es aceptable para el volumen de 2026 y 2027.
- Toda entidad nueva debe derivar de `Entity` para recibir estas reglas.
