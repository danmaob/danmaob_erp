# ADR-14: Catálogo de planes y datos semilla

**Estado:** Aceptado (2026-10-12)
**Fecha:** 2026-10-12
**Historia:** TS-03-1 (Épica E03 — Planes, módulos y cuotas)
**Requerimientos:** RF-009, RF-019, RF-029, RF-034, RF-036, RF-148, RNF-025, RNF-031
**Depende de:** ADR-01 (esquema `platform`), ADR-04 (entitlements, cuotas y consumo), ADR-06 (entidades), ADR-13 (auditoría)

## Contexto

El ADR-04 decidió que los módulos y los tipos de cuota viven en el código, y que los planes, sus módulos y sus límites son datos del catálogo `platform`. TS-03-1 carga esos datos para Free, Básico y Premium. Faltaba precisar qué es exactamente un módulo, cómo se modela la ventana de consulta de auditoría, cómo encuentra el registro autónomo el plan Free si los planes se pueden renombrar, cómo se cargan los datos y dos valores comerciales.

Este ADR precisa al ADR-04 sin reemplazarlo.

## Decisión

### 1. Alcance

Se construyen el plan, los módulos de cada plan y las cuotas de cada plan, con sus datos semilla. Las excepciones por empresa o cuenta, los paquetes adicionales y el libro de consumo del ADR-04 se construyen en las historias que los usan (US-03-x y E01).

### 2. Módulos (`ModuleCode`, `DanmaobErp.Domain.Plans`)

Solo son módulos las áreas que se habilitan o deshabilitan por plan: `Customers` (clientes y CRM, con prospectos), `Inventory`, `Sales`, `Purchasing`, `Invoicing`, `Finance`, `Reports` y `BusinessEmail`.

- **No son módulos:** la administración de la empresa y los usuarios, roles y permisos (núcleo, siempre disponibles); la exportación a Excel (capacidad de todos los listados); WhatsApp, API pública y automatizaciones (sin software en el release 1; se agregan cuando existan).
- El registro simple de abonos de Free (RF-099) es parte de Ventas; por eso `Finance` está deshabilitado en Free.
- Los niveles "básico, completo y avanzado" no se modelan (ADR-04).

### 3. Tipos de cuota (`QuotaCode`) y su definición

Cada código tiene en `QuotaDefinitions` su tipo, su alcance y si solo avisa:

| Código | Tipo | Alcance | Solo avisa |
|---|---|---|---|
| `Tenants` | Conteo | Cuenta | No |
| `ActiveUsers` | Conteo | Cuenta | No |
| `Branches` | Conteo | Empresa | No |
| `Warehouses` | Conteo | Empresa | No |
| `Products` | Conteo | Empresa | No |
| `Customers` | Conteo | Empresa | No |
| `Prospects` | Conteo | Empresa | No |
| `SalesDocuments` | Consumo mensual | Empresa | No |
| `FileStorageBytes` | Conteo (bytes) | Cuenta | No |
| `InvoiceStamps` | Consumo mensual | Empresa | No |
| `BusinessEmails` | Consumo mensual | Cuenta | No |
| `TaxDocumentStorageBytes` | Conteo (bytes) | Empresa | Sí |
| `AuditQueryWindowDays` | Parámetro | Empresa | No |

- **Tipo "parámetro":** la ventana de consulta de auditoría no se cuenta ni se consume; es un valor comercial del plan. Se modela en el mismo catálogo para que siga siendo dato y admita excepciones.
- Los enums tienen valores numéricos explícitos y nunca se reordenan: los identificadores de los datos semilla dependen de ellos. En la base se guardan como texto.

### 4. Plan por defecto para el registro

Los planes se pueden renombrar (RF-009), así que el registro autónomo no busca el plan por su nombre. El plan tiene la marca "plan por defecto para el registro"; un índice único filtrado permite un solo plan marcado. Es dato: el Super Admin puede cambiarlo.

### 5. Entidades (`platform`, en `PlatformCatalog`, auditables)

| Entidad | Contenido | Índices |
|---|---|---|
| `Plan` | Nombre, marca de plan por defecto. Baja lógica | Nombre único; marca única filtrada |
| `PlanModule` | Plan y código de módulo | Único (plan, módulo) |
| `PlanQuota` | Plan, código de cuota, límite (vacío = ilimitado; 0 = no disponible) y si se devuelve al cancelar | Único (plan, cuota) |

Quitar un módulo de un plan borra su registro, y el borrado queda auditado. Las relaciones con el plan no tienen propiedad de navegación y no borran en cascada.

### 6. Datos semilla

- Se cargan con la semilla de EF Core (`HasData`), concentrada en `PlanSeedData` y `PlanQuotaSeedData` (`DanmaobErp.Infrastructure.Persistence.Seeding`). Entran con la migración y se reproducen en cualquier instalación (RNF-025).
- Los identificadores son fijos y se derivan del número de plan, del grupo (plan, módulo o cuota) y del valor numérico del código.
- Ningún código de negocio lee esas clases: los límites se leen siempre de la base (US-03-1). Los valores aparecen ahí como datos iniciales, no como constantes de lógica (CA1 de TS-03-1).
- La semilla no pasa por `SaveChanges`, así que no genera registros de auditoría.
- Un cambio posterior de los valores iniciales en el código genera una migración que los actualiza; los cambios que haga el Super Admin en la plataforma no se tocan.

### 7. Valores iniciales

Módulos: Free tiene `Customers`, `Inventory` y `Sales`; Básico y Premium tienen los ocho.

| Cuota | Free | Básico | Premium |
|---|---|---|---|
| Empresas | 1 | 1 | 3 |
| Usuarios activos | 2 | 5 | 15 |
| Sucursales | 1 | 2 | 10 |
| Almacenes | 1 | 3 | 10 |
| Productos y servicios | 300 | 5,000 | 50,000 |
| Clientes | 200 | 5,000 | 50,000 |
| Prospectos | 100 | 1,000 | Ilimitado |
| Documentos de venta al mes | 100 | 2,000 | 20,000 |
| Almacenamiento de archivos | 500 MB | 5 GB | 25 GB |
| Timbres al mes | 0 | 100 | 500 |
| Correos de negocio al mes | 0 | 1,000 | 5,000 |
| Comprobantes fiscales | 0 | 2 GB | 10 GB |
| Ventana de consulta de auditoría | 90 días | 365 días | Ilimitada |

- Los tamaños se guardan en bytes binarios: 1 MB = 1,048,576 bytes y 1 GB = 1,073,741,824 bytes.
- **Devolución al cancelar** (documentos de venta): no en Free, sí en Básico y Premium. En Free no se devuelve el lugar aunque la operación solo haya llegado a cotización o pedido (resuelve el pendiente 1 del ADR-04).
- **Premium se siembra desactivado:** existe en el modelo pero nadie puede asignarlo hasta que se implemente.
- El plan Free es el plan por defecto para el registro.

## Alternativas consideradas

- **Script SQL de datos iniciales ejecutado a mano.** Descartada. No viaja con las migraciones a otras instalaciones (Private Cloud, On-Premise).
- **Proceso de siembra al arrancar la API.** Descartada. Agrega lógica de arranque y escrituras fuera del control de las migraciones.
- **Código fijo de plan para el registro.** Descartada. La marca es dato y no amarra un nombre ni un código en la lógica.
- **Ventana de auditoría como columna del plan.** Descartada. Quedaría fuera del mecanismo de excepciones y obligaría a otra migración por cada parámetro nuevo.
- **Administración, usuarios y exportación como módulos.** Descartada. Nunca se bloquean, así que serían configuración sin efecto.

## Consecuencias

**Lo que se facilita**

- US-03-1 resuelve módulos y límites leyendo tres tablas.
- US-02-2 encuentra el plan inicial por su marca.
- Los valores comerciales se cambian como datos.

**Lo que se complica o exige cuidado**

- Un módulo o una cuota nuevos exigen un código nuevo, su definición, sus datos semilla para cada plan y una migración.
- Los valores numéricos de los enums no se pueden reordenar.

**Pendiente**

- Vigencia de los paquetes adicionales (pendiente 2 del ADR-04): se resuelve en US-03-6.
