# ADR-15: Modelo del catálogo de identidad

**Estado:** Aceptado (2026-10-15)
**Fecha:** 2026-10-15
**Historia:** US-02-1 (Épica E02 — Registro, identidad y Cuenta Cliente)
**Requerimientos:** RF-005, RF-006, RF-023, RF-024, RF-026, RF-027, RF-044, RF-050, RF-145, RNF-029
**Depende de:** ADR-00 (Cuenta Cliente e identidad), ADR-01 (esquemas), ADR-03 (Super Admin), ADR-06 (entidades), ADR-12 (unidad de trabajo), ADR-13 (auditoría), ADR-14 (planes)

## Contexto

El ADR-00 decidió las cuatro entidades de identidad (Cuenta Cliente, empresa, usuario y acceso a empresa), su ubicación en el catálogo de plataforma, el correo único normalizado, las llaves foráneas compuestas que impiden que un acceso cruce cuentas y los roles por empresa en el acceso. Quedaban abiertos: el alcance de US-02-1, cómo existen los roles antes de E04, los campos de cada entidad, cómo se protege la regla de cuentas fuera de SQL Server y a qué cuenta y empresa se asocia la auditoría del catálogo (pendiente del ADR-13).

Este ADR precisa al ADR-00 y al ADR-13 sin reemplazarlos.

## Decisión

### 1. Alcance de US-02-1

US-02-1 construye el modelo, la persistencia y las reglas de dominio del catálogo de identidad. No expone endpoints: el primer endpoint es el registro autónomo (US-02-2), y el alta de usuarios por el administrador llega con E04, cuando exista el inicio de sesión (US-02-4).

- El correo único (CA1) se garantiza con el índice único sobre el correo normalizado; el paso de base de datos comprueba que la base rechaza un duplicado. El rechazo por la API se prueba en US-02-2.
- Los repositorios por agregado se deciden en US-02-2, donde un caso de uso los usa por primera vez: estrechos, en Application, solo con los métodos que pide cada caso de uso.

### 2. Entidades (`DanmaobErp.Domain.Identity`, esquema `platform`, auditables)

| Entidad | Campos | Reglas |
|---|---|---|
| `ClientAccount` | Plan (llave foránea a `Plan`), estado (`Active` o `Suspended`) | Nace activa. La suspensión (RF-005) no es baja lógica: conserva todo |
| `Tenant` | Cuenta, nombre comercial (150), RFC (13, anulable), baja lógica (desactivación, RF-006) | Llave alterna (cuenta, id). RFC único que ignora vacíos (RF-050) |
| `User` | Cuenta, nombre de la persona (150), correo tal como se escribió (256), correo normalizado (256), hash de contraseña (`[Sensitive]`), fecha de verificación del correo (anulable), baja lógica | Llave alterna (cuenta, id). Correo normalizado único en toda la plataforma, sin filtro por estado (RF-023, RF-024) |
| `TenantMembership` | Cuenta, usuario, empresa, baja lógica | Llaves foráneas compuestas (cuenta, usuario) y (cuenta, empresa). Un acceso por par usuario-empresa |
| `TenantMembershipRole` | Acceso, rol predefinido, y la cuenta y la empresa del acceso | Un registro por par acceso-rol. Quitar un rol borra su registro, y el borrado queda auditado |

- **Correo normalizado:** sin espacios al inicio ni al final y en mayúsculas invariantes, en un solo lugar (`EmailAddress.Normalize`).
- **Verificación:** una fecha vacía significa "sin verificar" (RF-017).
- Las relaciones no tienen propiedades de navegación ni borran en cascada.

### 3. Roles predefinidos en el acceso

Los roles configurables son de E04 (US-04-6) y son datos operativos, pero el registro debe dejar al primer usuario como Administrador sin tener una empresa resuelta. Por eso el acceso tiene una tabla hija del catálogo con el código de rol predefinido (`PredefinedRole`): `Administrator`, `Seller`, `WarehouseKeeper`, `Purchasing`, `Billing` y `Collections` (RF-044).

- La regla de tareas de cuenta del ADR-00 ("Administrador en al menos una empresa activa") se resuelve con esta tabla.
- US-04-6 agrega los roles personalizados sin rehacer esta tabla.
- **Los roles de empresa nunca dan permisos de plataforma.** El Super Admin no es un `User` ni tiene accesos (ADR-00, ADR-03); sus permisos `Platform.*` solo valen con el token de plataforma.

### 4. La regla de cuentas también en el dominio

Las llaves foráneas compuestas solo actúan en SQL Server; el proveedor en memoria de las pruebas no las aplica. El constructor del acceso recibe el usuario y la empresa y rechaza que pertenezcan a cuentas distintas; el constructor del rol copia la cuenta y la empresa del acceso. Las llaves compuestas quedan como última línea de defensa.

### 5. Ámbito de la auditoría del catálogo

- `PlatformAuditEntry` tiene la columna de cuenta, anulable, además de la de empresa, con un índice (cuenta, fecha) para la consulta de US-14-1.
- Cada entidad del catálogo que pertenece a una cuenta implementa `IPlatformAuditScope` (`DanmaobErp.Domain.Auditing`) y declara su cuenta y su empresa de auditoría:

| Entidad | Cuenta | Empresa |
|---|---|---|
| `ClientAccount` | Ella misma | — |
| `Tenant` | Su cuenta | Ella misma |
| `User` | Su cuenta | — |
| `TenantMembership` | Su cuenta | Su empresa |
| `TenantMembershipRole` | Su cuenta | Su empresa |

- La captura de auditoría solo lee esa interfaz. Las entidades que no la implementan (planes, auditoría) quedan sin cuenta ni empresa.
- Así el administrador ve en su consulta los cambios de usuarios, accesos y roles de su cuenta (RF-145), sin abrir la auditoría de otras cuentas.

## Alternativas consideradas

- **Endpoint de alta de usuarios en esta historia.** Descartada. Sin inicio de sesión no hay quién lo autorice.
- **Marca "es administrador" en el acceso.** Descartada. E04 la tendría que reemplazar con una migración.
- **Roles en los datos operativos desde ahora.** Descartada. El registro no tiene empresa resuelta para escribirlos.
- **Estado de la cuenta como baja lógica.** Descartada. Suspender no es dar de baja.
- **Marca booleana de correo verificado.** Descartada. La fecha guarda además cuándo se verificó.
- **Auditoría del catálogo sin cuenta.** Descartada. El administrador nunca vería los cambios de sus usuarios.
- **Calcular el ámbito de auditoría con casos especiales en la captura.** Descartada. La interfaz deja la decisión en cada entidad y la captura no conoce las entidades de identidad.

## Consecuencias

**Lo que se facilita**

- US-02-2 crea cuenta, empresa, usuario, acceso y rol Administrador en una sola transacción, con auditoría de plataforma asociada a la cuenta.
- Premium agrega empresas y accesos sin migrar usuarios (RNF-029).

**Lo que se complica o exige cuidado**

- Toda entidad nueva del catálogo que pertenezca a una cuenta debe implementar `IPlatformAuditScope`.
- Las columnas de las llaves alternas (cuenta e id) nunca cambian.

**Pendiente**

- Mensajes que no revelen si un correo existe en otra cuenta: US-02-2 y SP-02-3.
- Restricción por sucursal (RF-047): su historia, en el acceso.
