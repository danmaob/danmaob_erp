# ADR-02: Autenticación, sesiones y tokens compatibles con la futura app móvil

**Estado:** Aceptado (2026-10-03)
**Fecha:** 2026-10-02
**Historia:** SP-02-2 (Épica E02 — Registro, identidad y Cuenta Cliente)
**Requerimientos:** RF-017, RF-021, RF-022, RNF-004, RNF-005, RNF-007, RNF-010, RNF-040
**Depende de:** ADR-00 (empresa activa en el token y verificaciones de estado por solicitud)

## Contexto

En el release 1 la autenticación de usuarios de empresa es solo con correo y contraseña (RF-021), sin segundo factor (riesgo aceptado). La misma API y la misma autenticación las usarán la aplicación web y la futura app .NET MAUI, sin depender de mecanismos exclusivos del navegador (RNF-040).

Según el ADR-00, el token lleva al usuario, su cuenta y la empresa activa, y el backend verifica en cada solicitud el estado del usuario, la cuenta, la empresa y el acceso.

El precedente de TISAX (Instrucciones 9.2) es el punto de partida:

- desactivar a un usuario revoca sus refresh tokens;
- el refresh revisa que el usuario siga activo;
- el estado se revisa en cada solicitud;
- el administrador no puede desactivarse a sí mismo.

La identidad del Super Admin se trata en el ADR-03.

## Decisión

### 1. Contraseñas

- Se guardan solo como hash, con el `PasswordHasher` de `Microsoft.Extensions.Identity.Core` (PBKDF2 con sal y número de iteraciones del propio componente). Se usa el componente de hash, **no** ASP.NET Core Identity completo: su modelo de usuario y sus tablas no corresponden al modelo del ADR-00, y sus entidades tienen `set` públicos.
- Política del release 1:
  - longitud mínima de 10 caracteres y máxima de 128;
  - se admiten espacios y cualquier carácter;
  - no se exigen combinaciones obligatorias de mayúsculas, números o símbolos.

  Estos valores son configuración, no constantes.
- Las contraseñas nunca aparecen en logs, en auditoría ni en respuestas (RNF-004, RF-147).

### 2. Token de acceso

- Es un JWT firmado con HMAC-SHA256. La llave de al menos 256 bits llega por variable de entorno y la API no arranca si falta (RNF-007). Basta con una llave simétrica porque la misma API emite y valida.
- Vigencia: 15 minutos (configurable).
- Contiene:
  - identificador del usuario;
  - identificador de la cuenta;
  - identificador de la empresa activa;
  - identificador único del token;
  - emisor;
  - audiencia propia de empresa, distinta de la audiencia de plataforma del ADR-03.
- No contiene roles ni permisos. Los permisos se resuelven en el backend en cada solicitud, para que un cambio de rol aplique de inmediato y el token no crezca.
- Se envía en el encabezado `Authorization: Bearer`. Lo validan el middleware estándar de ASP.NET Core (`JwtBearer`) y después las verificaciones de estado del ADR-00, sección 5.
- La tolerancia de diferencia de reloj es de 30 segundos.

### 3. Refresh token

- Es un valor aleatorio opaco de 256 bits, no un JWT.
- En el catálogo (esquema `platform`) se guarda **solo su hash SHA-256**, junto con:
  - el usuario;
  - la empresa para la que se emitió;
  - la familia de sesión a la que pertenece;
  - las fechas de emisión, expiración y uso;
  - el motivo de revocación.
- **Rotación en cada uso:** renovar entrega un nuevo token de acceso y un nuevo refresh token, y el anterior queda usado.
- **Detección de reutilización:** si llega un refresh token ya usado, se revoca toda su familia de sesión y se registra un evento de seguridad (RNF-010).
- Vigencias:
  - cada refresh token caduca a los 14 días sin uso;
  - toda sesión caduca a los 30 días desde el inicio de sesión, aunque se use.

  Ambos valores son configurables.
- Al renovar se repiten las verificaciones de estado del ADR-00. Si alguna falla, la renovación se rechaza y la familia se revoca.

### 4. Revocación

| Evento | Efecto |
|---|---|
| Cierre de sesión | Revoca la familia de esa sesión |
| Desactivación del usuario | Revoca todas sus familias. Sus tokens de acceso vigentes se rechazan en la siguiente solicitud por la verificación de estado |
| Restablecimiento o cambio de contraseña | Revoca todas sus familias (US-02-5) |
| Suspensión de la cuenta o desactivación de la empresa | Revoca las familias afectadas. La verificación de estado ya rechaza las solicitudes |
| Reutilización de un refresh token | Revoca la familia y registra un evento de seguridad |

El administrador no puede desactivarse a sí mismo; la regla se impone en el backend.

### 5. Flujos

- **Inicio de sesión:**
  - Recibe correo y contraseña.
  - Si las credenciales no coinciden, el mensaje es uno solo y genérico, sin distinguir si el correo existe.
  - Solo cuando la contraseña es correcta y el correo no está verificado se informa que debe verificarse (US-02-3, CA2). Así el mensaje no revela qué correos existen.
  - El contrato admite la respuesta "seleccione una empresa" para usuarios con varios accesos (ADR-00). En el release 1 no ocurre.
- **Renovación:** recibe el refresh token en el cuerpo de la solicitud y devuelve el par nuevo en el cuerpo de la respuesta.
- **Cierre de sesión:** recibe el refresh token en el cuerpo y revoca su familia.
- **Verificación de correo y restablecimiento de contraseña:** usan tokens de un solo uso, aleatorios, guardados como hash y con expiración configurable (24 horas para verificación y 1 hora para restablecimiento como valores iniciales). Solicitar un restablecimiento responde lo mismo exista o no el correo (US-02-5, CA2).
- La limitación de intentos y el mecanismo contra robots se deciden en SP-02-3.

### 6. Transporte y almacenamiento en los clientes

- La API usa **el mismo contrato para todos los clientes**: los tokens viajan en el cuerpo y en el encabezado `Authorization`, nunca en cookies. Así la app MAUI los usa sin cambios.
- **App MAUI (futura):** guarda el refresh token en el almacenamiento seguro del dispositivo.
- **Aplicación web (release 1):**
  - el token de acceso vive solo en memoria;
  - el refresh token se guarda en `localStorage` para no pedir inicio de sesión en cada recarga.

  Esto expone el refresh token si existiera una vulnerabilidad de XSS. Se mitiga con:
  - la rotación y la detección de reutilización;
  - la vigencia acotada;
  - una política de seguridad de contenido (CSP) estricta;
  - React sin inserción de HTML sin sanitizar.

  **Es un riesgo que Luis debe aceptar explícitamente al aprobar este ADR.** La alternativa futura es agregar un transporte por cookie `HttpOnly` solo para la web, sin cambiar el contrato de los demás clientes.

### 7. Versionado

Los endpoints de autenticación forman parte de la API versionada (TS-00-6). Un cambio incompatible en su contrato exige una versión nueva, porque la app móvil no se actualiza al mismo ritmo que la web.

## Alternativas consideradas

- **ASP.NET Core Identity completo.** Descartada. Impone su modelo de usuario y sus tablas, que no corresponden al modelo de Cuenta Cliente, y sus entidades tienen `set` públicos.
- **Cookies de sesión.** Descartada. Es un mecanismo del navegador (incumple RNF-040) y obligaría a protección CSRF.
- **Solo JWT de larga duración, sin refresh token.** Descartada. No permite revocar sesiones sin consultar el estado y obliga a vigencias largas.
- **Refresh token como JWT.** Descartada. Para revocarlo hay que guardarlo de todas formas, y un valor opaco con hash no expone información.
- **Firma asimétrica (RS256) desde el release 1.** Descartada por ahora. Solo hace falta si un tercero valida los tokens; se reconsidera con la API pública de Premium.
- **Proveedor de identidad externo** (Auth0, Cognito u otro). Descartada. Agrega costo por usuario, dependencia de nube (incumple RNF-023) y complica Private Cloud y On-Premise.

## Consecuencias

**Lo que se facilita**

- La web y la app móvil comparten exactamente el mismo contrato.
- La revocación es efectiva de inmediato gracias a la verificación de estado por solicitud.
- Un refresh token robado y reutilizado se detecta y cancela la sesión completa.

**Lo que se complica o exige cuidado**

- El riesgo del refresh token en `localStorage` en la web (ver sección 6).
- Hay una tabla de refresh tokens que crece. Se requiere una depuración periódica de los expirados; su forma se define en la historia correspondiente.
- La rotación exige manejar en el frontend las renovaciones simultáneas: una sola renovación a la vez, con las demás solicitudes esperando su resultado (TS-00-12).

**Trabajo que queda desbloqueado**

- **US-02-3:** verificación de correo.
- **US-02-4:** inicio de sesión, renovación y revocación.
- **US-02-5:** restablecimiento de contraseña.
- **TS-00-12:** cliente API con renovación de sesión.
- **TS-00-2:** variables de entorno de la llave de firma y de las vigencias.
