# ADR-00: Cuenta Cliente, identidad de usuarios y resolución de la empresa activa

**Estado:** Aceptado (2026-10-03)
**Fecha:** 2026-10-02
**Historia:** SP-02-1 (Épica E02 — Registro, identidad y Cuenta Cliente)
**Requerimientos:** RF-016, RF-019, RF-020, RF-023 a RF-028, RF-039, RF-047, RNF-002, RNF-005, RNF-029, RNF-040
**Fuentes:** Síntesis del Proyecto v1.0 (secciones 3, 4 y 11), Requerimientos del Release 1 v1.0 (secciones 3 y 15), Instrucciones del Project v1.0 (secciones 4.2, 9.2 y 14)

## Contexto

DANMAOB_ERP es un SaaS multiempresa. El release 1 implementa Free y Básico, con una empresa por cuenta, pero Premium permitirá que una misma Cuenta Cliente administre varias empresas jurídicamente distintas. Si el modelo de identidad no contempla esa capa desde el inicio, habilitar Premium obligaría a migrar usuarios y datos.

Estas decisiones de producto ya están tomadas y este ADR no las vuelve a discutir:

- Un usuario pertenece a una sola Cuenta Cliente, y su correo es único en toda la plataforma.
- El correo de un usuario desactivado queda bloqueado y no puede registrarse en otra cuenta. Si se intenta dar de alta en su misma cuenta, se ofrece reactivarlo.
- No existe un rol de propietario de cuenta. Las tareas de cuenta las realiza el administrador de cualquier empresa de la cuenta.
- Las empresas de una misma cuenta están aisladas por completo: no comparten catálogos, clientes, productos ni proveedores.
- El RFC de la empresa emisora es único en toda la plataforma.
- Las bajas de usuarios son lógicas. No hay borrado físico.
- Todo registro crea una Cuenta Cliente en Free, con una empresa, y el primer usuario queda como Administrador.
- El Super Admin es único y no pertenece a ninguna empresa.

El modelo de DANMAOB_TISAX (un usuario de un solo tenant, con correo único por tenant) no se hereda.

Este ADR decide tres cosas:

1. Las entidades y relaciones de identidad.
2. Dónde viven esos datos respecto de la información operativa de cada empresa.
3. Cómo se determina, en cada solicitud, la empresa sobre la que se trabaja.

## Decisión

### 1. Modelo de identidad

Se adoptan cuatro conceptos. Los nombres en código se proponen en inglés, en línea con la convención `TenantId` heredada de TISAX (Instrucciones 9.2).

| Concepto | Nombre en código | Responsabilidad |
|---|---|---|
| Cuenta Cliente | `ClientAccount` | Agrupa las empresas y los usuarios de un mismo cliente. Lleva el plan y el estado comercial (activa o suspendida) |
| Empresa | `Tenant` | Una empresa jurídicamente distinta. Pertenece a una sola cuenta y tiene un estado propio (activa o desactivada) |
| Usuario | `User` | Una persona con credenciales. Pertenece a una sola cuenta de forma permanente |
| Acceso a empresa | `TenantMembership` | Relaciona a un usuario con una empresa de su misma cuenta. Lleva los roles del usuario en esa empresa y, más adelante, su restricción por sucursal (RF-047) |

Reglas del modelo:

- La cuenta de un usuario se asigna al crearlo y nunca cambia.
- La cuenta de una empresa se asigna al crearla y nunca cambia.
- Un usuario puede tener acceso a una o varias empresas de su cuenta, nunca a una empresa de otra cuenta. La regla se impone también en la base de datos: el acceso guarda el identificador de la cuenta y se relaciona con el usuario y con la empresa mediante llaves foráneas compuestas (cuenta más usuario, cuenta más empresa). Un acceso que cruce cuentas no se puede guardar.
- Solo existe un acceso por cada par de usuario y empresa. El acceso tiene baja lógica.
- **Los roles se asignan por empresa, en el acceso**, no por cuenta. Esto es coherente con que cada empresa defina sus propios roles (RF-045) y con el aislamiento entre empresas.
- En el release 1 cada cuenta tiene una empresa y cada usuario un acceso. El modelo admite N sin cambios de estructura.

### 2. Correo único y ciclo de vida del usuario

- El correo se guarda tal como lo escribió el usuario y, además, en una forma normalizada: sin espacios al inicio ni al final y en mayúsculas invariantes.
- La unicidad se impone con un **índice único sobre el correo normalizado, en toda la plataforma**, sin filtrar por estado. Como la baja es lógica, el registro del usuario desactivado conserva su correo y el mismo índice impide que se registre en otra cuenta (RF-024). La aplicación valida antes de guardar para dar un mensaje claro; el índice es la última línea de defensa ante solicitudes simultáneas.
- Alta de un correo existente:
  - Si pertenece a un usuario desactivado de la misma cuenta, se ofrece reactivarlo (RF-025).
  - En cualquier otro caso se rechaza.
- La redacción de los mensajes de rechazo debe evitar revelar si un correo está registrado en otra cuenta. Se precisa en US-02-2 y en SP-02-3.
- El usuario tiene un estado de verificación de correo, independiente de su estado activo o inactivo. Un usuario sin verificar no puede iniciar sesión (RF-017).

### 3. Ubicación de los datos de identidad

Los datos se dividen en dos áreas lógicas:

| Área | Contenido | Filtro por empresa |
|---|---|---|
| **Catálogo de plataforma** | Cuentas, empresas (su registro de identidad: cuenta, nombre comercial, RFC y estado), usuarios, accesos a empresa y los datos del Control Plane | No. Se consulta antes de conocer la empresa, por ejemplo al iniciar sesión por correo |
| **Datos operativos** | Todo lo que pertenece a una empresa: configuración operativa, clientes, productos, inventarios, ventas, roles y permisos de la empresa, etcétera | Sí. Cada entidad pertenece a una empresa y queda bajo el filtro global |

Consecuencias de esta división:

- El inicio de sesión resuelve al usuario por su correo sin depender de ninguna empresa. Por eso la identidad no puede vivir dentro de los datos de cada empresa.
- El RFC de la empresa emisora vive en el registro de la empresa del catálogo, con un índice único que ignora los valores vacíos, porque su unicidad es de plataforma (RF-050) y en Free puede no existir. El resto de los datos fiscales y operativos de la empresa vive en los datos operativos y se define en E04.
- Las entidades del catálogo **no** implementan la interfaz de pertenencia a empresa. Solo las consultan los servicios de identidad, de registro y del Control Plane, nunca un módulo operativo.
- Esta división es compatible con cualquier opción de SP-00-1. Si en el futuro se separan bases de datos por empresa, el catálogo permanece como base de plataforma. Que en el release 1 compartan o no la misma base física lo decide SP-00-1.

### 4. Resolución de la empresa activa

**La empresa activa viaja en el token de acceso.**

- El token de acceso lleva el identificador del usuario, el de su cuenta y el de la empresa activa. Cada token sirve para una sola empresa.
- Al iniciar sesión:
  - Si el usuario tiene un acceso activo, el token se emite para esa empresa. Es el único caso del release 1.
  - Si tiene varios (Premium), la respuesta indica las empresas disponibles y el usuario elige. El contrato del inicio de sesión se diseña desde el release 1 para admitir ese segundo paso, aunque no se implemente.
- El cambio de empresa activa (Premium) es una operación explícita que valida el acceso del usuario a la empresa destino y emite un token nuevo. No se implementa en el release 1.
- En cada solicitud a un endpoint operativo, el backend toma la empresa del token y la expone mediante un servicio con alcance por solicitud. El filtro global de EF Core (TS-00-4) usa ese servicio.
- **Si no hay empresa resoluble, el acceso a datos operativos lanza una excepción explícita.** No existe valor por defecto, empresa "comodín" ni mecanismo temporal (RNF-002, Instrucciones 14.5).
- El token se envía como encabezado de autorización, sin cookies ni mecanismos exclusivos del navegador, para que la app .NET MAUI lo use igual (RNF-040).
- El formato del token, su firma, su expiración, el refresh y la revocación se deciden en SP-02-2.

### 5. Verificaciones de estado en cada solicitud

Que un token sea válido no basta. En cada solicitud a un endpoint operativo, el backend verifica contra el catálogo, en este orden:

1. El usuario existe, está activo y tiene el correo verificado.
2. La cuenta del usuario no está suspendida (RF-005).
3. La empresa activa existe y está activa (RF-006).
4. El acceso del usuario a esa empresa existe y está activo.
5. El usuario tiene el permiso del módulo y de la operación (RNF-003; diseño en su historia).

La primera verificación que falla detiene la solicitud. Los casos de cuenta suspendida y empresa desactivada devuelven un mensaje comprensible para el usuario, sin detalles internos. Las verificaciones 1 a 4 se resuelven con una sola consulta al catálogo. No se agrega caché mientras no exista una necesidad medida.

### 6. Tareas de cuenta sin propietario

- Una tarea de cuenta se autoriza si el usuario tiene el rol Administrador en **al menos una empresa activa de su cuenta**. Es una regla derivada de los accesos; no se guarda ningún "propietario".
- Ejemplos de tareas de cuenta: crear otra empresa en Premium, consultar las cuotas compartidas y decidir qué queda activo tras un cambio a un plan menor.
- La cuota de usuarios activos se cuenta por **usuarios distintos y activos de la cuenta**, no por accesos. Así un usuario con acceso a varias empresas cuenta como uno y los desactivados no cuentan (RF-039). El diseño completo de cuotas corresponde a SP-03-1.

### 7. Registro autónomo

El registro (US-02-2) crea, en una sola transacción:

- la Cuenta Cliente en Free;
- la empresa con su nombre comercial;
- el usuario sin verificar;
- su acceso a la empresa con el rol Administrador.

Si cualquiera de estas operaciones falla, no queda nada creado.

### 8. Fuera de este ADR

| Tema | Dónde se decide |
|---|---|
| Formato del token, expiración, refresh y revocación | SP-02-2 |
| Identidad, almacenamiento e inicio de sesión del Super Admin | SP-01-1. El Super Admin no es un `User`, no pertenece a ninguna cuenta y nunca tiene un acceso a empresa |
| Acceso de soporte a una empresa (RF-043) | Su historia. No se implementa como acceso a empresa del Super Admin, sino como permiso temporal otorgado por la empresa y auditado |
| Estrategia física de almacenamiento | SP-00-1 |
| Planes, módulos y cuotas | SP-03-1 |
| Mecanismo contra robots y mensajes que eviten la enumeración de correos | SP-02-3 |
| Restricción por sucursal | La historia de RF-047. Este ADR solo fija que vive en el acceso a empresa |

## Alternativas consideradas

- **Modelo de TISAX: usuario de un solo tenant, con correo único por tenant.** Descartado. Contradice el correo único en la plataforma y obligaría a migrar usuarios al habilitar Premium.
- **Usuario global que puede pertenecer a varias cuentas** (por ejemplo, un contador que atiende a varios clientes con un solo correo). Descartado por decisión de producto (Síntesis 3): un usuario pertenece a una sola cuenta.
- **Rol de propietario de cuenta.** Descartado por decisión de producto. Además introduciría un punto único de falla cuando ese usuario se desactiva.
- **Roles por cuenta en lugar de por empresa.** Descartado. Rompe el aislamiento entre empresas de una cuenta Premium y contradice los roles propios de cada empresa.
- **Identidad dentro de los datos de cada empresa.** Descartado. El inicio de sesión ocurre antes de conocer la empresa, y la unicidad del correo es de plataforma.
- **Empresa activa en un encabezado enviado por el cliente.** Descartado. Es seguro si se valida, pero añade una segunda fuente de verdad junto al token, y cada cliente (web y móvil) debe recordar enviarlo. Un olvido produce errores difíciles de diagnosticar.
- **Empresa activa en la ruta** (por ejemplo, `/empresas/{id}/clientes`). Descartado para el release 1. Obliga a que cada endpoint reciba y compare el identificador, y en el release 1, con una sola empresa, solo agrega ruido. Puede reconsiderarse si en el futuro hace falta operar sobre varias empresas en una misma sesión.

## Consecuencias

**Lo que se facilita**

- Premium se habilita creando más empresas y más accesos, sin migrar usuarios ni datos.
- La empresa activa tiene una sola fuente de verdad (el token), y el backend la valida contra el catálogo en cada solicitud.
- La unicidad del correo y la regla "un acceso nunca cruza cuentas" quedan garantizadas por la base de datos, no solo por la aplicación.
- La web y la app móvil comparten el mismo mecanismo.

**Lo que se complica o exige cuidado**

- Coexisten entidades con y sin filtro por empresa. Es exactamente el tipo de configuración donde en TISAX un filtro quedó sin invocar. TS-00-4 debe incluir:
  - la prueba de aislamiento entre empresas;
  - una prueba que falle si una entidad operativa no implementa la interfaz de pertenencia a empresa. Las entidades del catálogo forman una lista explícita de excepciones, y cualquier entidad nueva fuera de esa lista debe tener el filtro.
- Cada solicitud operativa hace una consulta al catálogo para verificar estados. Es aceptable para el volumen de 2026 y 2027. Si una medición lo justifica, se evalúa una caché con invalidación al desactivar, en su propio ADR.
- El cambio de empresa activa requiere emitir un token nuevo. Es aceptable porque solo aplica en Premium.

**Trabajo que queda desbloqueado**

- **US-02-1:** entidades y configuración del catálogo, con índices únicos y llaves compuestas.
- **TS-00-4:** servicio de empresa por solicitud, filtro global, excepción explícita y pruebas de aislamiento.
- **SP-00-1:** decide la ubicación física del catálogo y de los datos operativos.
- **SP-02-2:** define el token que transporta usuario, cuenta y empresa activa.
- **SP-03-1:** usa la regla de conteo de usuarios por cuenta.
- **SP-01-1:** parte de que el Super Admin está fuera del modelo de cuentas.
