# ADR-10: Servicios externos detrás de abstracciones

**Estado:** Aceptado (2026-10-10)
**Fecha:** 2026-10-10
**Historia:** TS-00-8 (Épica E00 — Fundación técnica y arquitectura)
**Requerimientos:** RNF-022, RNF-023
**Depende de:** ADR-08 (mensajes y errores)

## Contexto

DANMAOB_ERP debe poder ejecutarse completo en el equipo de Luis, publicarse en AWS y, en el futuro, instalarse en la nube privada o en los servidores de un cliente con la misma base de código. Ningún componente puede depender de un servicio exclusivo de una nube. El release 1 necesita enviar correos de sistema, guardar archivos (logotipos, comprobantes) y proteger el registro contra robots, cuya solución concreta se decide en SP-02-3.

## Decisión

### 1. Abstracciones en Application

- `IEmailSender` envía un `EmailMessage` (destinatario, asunto, cuerpo HTML y cuerpo de texto).
- `IFileStorage` guarda, lee y comprueba archivos por **clave**.
- `IHumanVerifier` valida el token del mecanismo contra robots.

El código de negocio solo conoce estas interfaces. Un proveedor real se agrega como una implementación nueva en Infrastructure, sin tocar el negocio.

### 2. Implementaciones locales (release 1, validación en el equipo de Luis)

- **Correo:** `FolderEmailSender` escribe un archivo `.eml` por mensaje con `SmtpClient` en modo carpeta de recogida, que no se conecta a ningún servidor. Los archivos se abren en Mail de macOS. Carpeta: `Email:OutboxPath`, por defecto `App_Data/mail-outbox`. Remitente: `Email:From`, por defecto `no-responder@danmaob.com.mx`.
- **Archivos:** `LocalFileStorage` guarda en disco bajo `FileStorage:RootPath`, por defecto `App_Data/files`.
- `App_Data/` está excluida de Git.

### 3. Claves de archivo seguras

- Una clave es una ruta relativa con segmentos de letras, números, `.`, `-` y `_` separados por `/`. Por ejemplo: `tenants/{empresa}/logos/logo.png` o `platform/…`.
- Se rechazan las claves vacías, absolutas, con `..`, con `\` o con otros caracteres, y toda ruta que resuelva fuera de la carpeta raíz.
- Quien guarda archivos de una empresa compone la clave con su `TenantId`. El almacenamiento no conoce empresas.

### 4. Anti-robots sin hueco de seguridad

- En **desarrollo**, `DevelopmentHumanVerifier` acepta cualquier token no vacío, para probar el registro localmente.
- En **cualquier otro ambiente**, `RejectingHumanVerifier` rechaza todo hasta que SP-02-3 traiga la implementación real. Publicar sin configurarla deja el registro cerrado, no abierto a robots.

### 5. Criterio para proveedores reales

Cada proveedor de nube (correo transaccional, almacenamiento de objetos, captcha) entra como una implementación nueva de la misma interfaz, seleccionada por configuración, con sus credenciales solo en variables de entorno. Antes de enviar correo real, el dominio del remitente debe estar verificado (SPF y DKIM).

## Alternativas consideradas

- **Contenedor de correo de pruebas (Mailpit, MailHog).** Descartado: requiere Docker, y el backlog pide revisar el correo sin Docker.
- **Usar desde el inicio un SDK de nube (S3, SES).** Descartado: rompe la portabilidad (RNF-023).
- **Verificador que acepte todo en cualquier ambiente.** Descartado: sería un mecanismo temporal inseguro (Instrucciones 14.5).

## Consecuencias

**Lo que se facilita**

- Todo funciona en el equipo de Luis sin servicios externos ni Docker.
- Cambiar de proveedor no toca el negocio.

**Lo que exige cuidado**

- Con varias instancias en la nube, el almacenamiento local no sirve: hay que agregar uno compartido al publicar.
- La cuota de almacenamiento (ADR-04) se mide fuera de esta abstracción, cuando llegue su historia.
