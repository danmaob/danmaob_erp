# ADR-03: Identidad del Super Admin

**Estado:** Aceptado (2026-10-03)
**Fecha:** 2026-10-02
**Historia:** SP-01-1 (Épica E01 — Control Plane y Super Admin)
**Requerimientos:** RF-001, RF-002, RF-003, RF-012, RF-015, RNF-007, RNF-010, RNF-028
**Depende de:** ADR-00 (el Super Admin no es usuario de ninguna cuenta) y ADR-02 (tokens de empresa)

## Contexto

La plataforma tiene un solo Super Admin. Su correo se define por configuración y nunca en el código (RF-001). El Control Plane está separado del ERP bajo su propia ruta, y ningún cliente lo administra (RF-003).

El Super Admin solo ve datos operativos de una empresa mientras exista un permiso de soporte vigente otorgado por ella (RF-015). No tiene segundo factor en el release 1; es un riesgo aceptado.

El precedente de TISAX (Instrucciones 9.2) es el siguiente:

- tabla propia, sin tenant;
- inicio de sesión propio, sin refresh token;
- permisos `Platform.*` válidos solo con token de plataforma;
- token solo en la memoria del navegador;
- auditoría de plataforma con endpoint propio y exportación a CSV.

## Decisión

Se adopta el precedente de TISAX con las precisiones siguientes.

### 1. Almacenamiento

- El Super Admin es una entidad propia del catálogo de plataforma (esquema `platform`), sin cuenta, sin empresa y sin accesos a empresa. No es un `User` del ADR-00.
- Su correo **no** puede existir como usuario de ninguna cuenta. El registro y el alta de usuarios rechazan ese correo con el mismo mensaje que cualquier correo no disponible.

### 2. Configuración y arranque

- El correo llega por variable de entorno. La API no arranca si falta (RNF-007).
- Al arrancar, la API garantiza que exista exactamente un registro de Super Admin con ese correo:
  - **Primera vez:** crea el registro. Su contraseña inicial llega por otra variable de entorno, que solo se lee cuando el registro no existe. Se guarda como hash (`PasswordHasher`, igual que en el ADR-02) y después la variable puede retirarse.
  - **Si el correo configurado cambia:** el registro existente adopta el nuevo correo y queda registrado en la auditoría de plataforma.
  - La contraseña inicial nunca se escribe en logs ni en la auditoría.
- El Super Admin cambia su contraseña desde el área de plataforma. Si la olvida, usa el restablecimiento por correo de sistema, con las mismas reglas de token de un solo uso del ADR-02.
- Los nombres exactos de las variables de entorno se fijan en TS-00-2.

### 3. Autenticación y token de plataforma

- El inicio de sesión de plataforma tiene su propio endpoint, bajo la ruta de plataforma de la API.
- El token de plataforma es un JWT con las siguientes características:
  - **llave de firma propia**, distinta de la de empresa y recibida por su propia variable de entorno;
  - **audiencia de plataforma**;
  - vigencia de 60 minutos, configurable;
  - **sin refresh token**: al vencer, el Super Admin vuelve a iniciar sesión.
- En el frontend, el token de plataforma vive **solo en memoria**. Al recargar la página se pide un nuevo inicio de sesión (US-01-8, CA2).
- La API registra dos esquemas de autenticación separados, uno de empresa y otro de plataforma:
  - Los endpoints de plataforma exigen el esquema de plataforma y una política `Platform.*`.
  - Los endpoints operativos exigen el esquema de empresa.
  - Un token de un esquema nunca es válido en el otro, porque difieren la llave y la audiencia (US-01-1, CA1).
- En cada solicitud de plataforma se verifica que el registro del Super Admin siga activo.
- Los inicios de sesión fallidos y exitosos de plataforma son eventos de seguridad (RNF-010). La limitación de intentos se define en SP-02-3.

### 4. Permisos de plataforma

- Las operaciones del Control Plane se protegen con políticas con nombre `Platform.<Área>.<Operación>`. Ejemplos: consultar cuentas, suspender una cuenta, cambiar un plan.
- El único Super Admin tiene todas las políticas. Se modelan desde ahora para que agregar más operadores de plataforma en el futuro sea un cambio de configuración y no de código; ese caso no se implementa en el release 1.

### 5. Acceso de soporte a datos operativos (RF-015, RF-043)

- El Super Admin **nunca** obtiene un token de empresa ni un acceso a empresa.
- Sus consultas de soporte pasan por endpoints de plataforma que reciben la empresa de forma explícita y verifican que exista un permiso de soporte vigente otorgado por esa empresa.
- Fijan la empresa resuelta de la solicitud solo para esa operación, de modo que el filtro global del ADR-01 se sigue aplicando.
- Cada uso queda auditado (RF-146).
- El detalle (alcance de solo lectura o de escritura, duración del permiso) se define en la historia del acceso de soporte.

### 6. Auditoría de plataforma

Las acciones del Super Admin se registran en la auditoría de plataforma, consultable y exportable a CSV desde un endpoint propio (RF-012). Su diseño se integra con el framework de auditoría de TS-14-1.

## Alternativas consideradas

- **Super Admin como usuario de una cuenta especial "DANMAOB".** Descartada. Mezcla el Control Plane con el ERP (incumple RF-003 y RNF-028) y lo expone a las reglas y al filtro de las empresas.
- **Correo y hash de contraseña solo en configuración, sin registro en la base.** Descartada. Impide cambiar la contraseña desde la aplicación y obliga a manipular un hash en variables de entorno.
- **Misma llave de firma con una marca de tipo dentro del token.** Descartada. Basta un error al validar la marca para que un token sirva en ambos lados. Con llaves y audiencias distintas, el error tendría que ocurrir en dos lugares.
- **Refresh token para el Super Admin.** Descartada, siguiendo el precedente. Una sesión corta y sin renovación reduce la exposición de la cuenta más poderosa, que además no tiene segundo factor.

## Consecuencias

**Lo que se facilita**

- La separación entre plataforma y empresa queda garantizada criptográficamente, no solo por convención.
- El correo del Super Admin nunca aparece en el código fuente (US-01-1, CA2).
- Agregar operadores de plataforma en el futuro no exige rediseño.

**Lo que se complica o exige cuidado**

- Hay dos llaves de firma que custodiar y, en su momento, rotar.
- El Super Admin no tiene segundo factor; es un riesgo aceptado para el release 1. Debe priorizarse cuando se agregue el segundo factor.
- Mientras exista la variable de contraseña inicial, es un secreto sensible. La guía de instalación (TS-00-13) debe indicar que se retire después del primer arranque.

**Trabajo que queda desbloqueado**

- **US-01-1:** Super Admin configurado e inicio de sesión de plataforma.
- **US-01-8:** área de plataforma en React.
- **TS-00-2:** variables de entorno del correo, de la contraseña inicial y de la llave de plataforma.
