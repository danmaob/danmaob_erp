# ADR-08: Contrato de errores e internacionalización de mensajes

**Estado:** Aceptado (2026-10-05)
**Fecha:** 2026-10-05
**Historia:** TS-00-5 (Épica E00 — Fundación técnica y arquitectura)
**Requerimientos:** RNF-032, RNF-036, RNF-038, RNF-010
**Depende de:** ADR-02 (la app móvil consume la misma API), ADR-07 (excepciones de aislamiento por empresa)

## Contexto

La web en React y la futura app MAUI deben interpretar los errores de la API de la misma forma. Los mensajes para el usuario van en español en el release 1, el inglés llega en el release 2, y Luis decidió que los textos deben poder editarse **sin tocar código, sin recompilar y sin volver a publicar**. Además, ningún error debe exponer detalles internos.

## Decisión

### 1. Contrato de error

- Toda respuesta de error usa `ProblemDetails` (RFC 9457, `application/problem+json`) con `status`, `code`, `title` y `traceId`. Los errores de validación agregan `errors`: los mensajes de cada campo.
- `code` es el contrato público, estable y en inglés. `title` es el mensaje para la persona, ya traducido.

| Situación | HTTP | `code` | Log |
|---|---|---|---|
| Datos inválidos o regla de negocio | 422 | `validation_failed` | Informativo |
| Sin empresa resuelta (directa o envuelta por EF Core) | 401 | `tenant_not_resolved` | Advertencia |
| Escritura en datos de otra empresa | 403 | `cross_tenant_write` | Advertencia (evento de seguridad) |
| Ruta inexistente | 404 | `not_found` | — |
| Método no permitido | 405 | `method_not_allowed` | — |
| Error no controlado | 500 | `internal_error` | Error, con la excepción completa |

- El error 500 nunca incluye el mensaje, el tipo ni la traza de la excepción, en ningún ambiente. Los mensajes internos del lector de JSON se suprimen.
- El texto de `cross_tenant_write` no revela que el registro existe en otra empresa.
- Los logs van siempre en inglés y nunca contienen cuerpos de solicitud, contraseñas ni datos fiscales.

### 2. Responsabilidades de traducción

- **El backend traduce sus mensajes** (errores y validaciones) y los entrega en el idioma de cada solicitud. Así la web y la app no duplican diccionarios.
- **Cada cliente traduce su propia interfaz.** La forma de cargar los diccionarios de la web se decide al iniciar React.
- Domain y Application nunca escriben textos: sus excepciones llevan claves de mensaje (`MessageKeys`), y la traducción ocurre solo en la frontera de la API.

### 3. Diccionarios

- Un archivo JSON plano por idioma (`es.json`), con claves con prefijo por área (`Errors.*`, `Validation.*`; cada módulo agrega el suyo).
- Los archivos **no se embeben en el ensamblado**: se publican junto a la API, en la carpeta `Localization/` (configurable con `Localization:Path`). Al editar un archivo, la API lo vuelve a cargar sola, sin reiniciar. Si el archivo editado no es válido, se conserva la versión anterior y se registra el error.
- La API no arranca si falta el diccionario del idioma por defecto.
- Respaldo: idioma de la solicitud, luego el idioma por defecto, luego la clave misma, con una advertencia en el log. Nunca se lanza una excepción por una clave faltante en tiempo de ejecución.
- Configuración: `Localization:DefaultCulture` (por defecto `es`) y `Localization:SupportedCultures` (por defecto solo el idioma por defecto). Agregar un idioma es crear su JSON y agregarlo a la configuración, sin tocar código.

### 4. Implementación

- `JsonStringLocalizer` implementa la interfaz estándar `IStringLocalizer` y vive en Infrastructure, que referencia el marco de ASP.NET Core (`FrameworkReference`, sin versión propia). Se registra con `Replace` para que prevalezca sobre el localizador por defecto de ASP.NET Core.
- Los atributos de validación declaran como `ErrorMessage` una clave del diccionario. Los mensajes no incluyen el nombre del campo, porque los errores ya llegan agrupados por campo.
- Los mensajes de conversión y de valores faltantes que genera ASP.NET Core se reemplazan por `Validation.InvalidValue` y `Validation.Required`. Se desactiva la regla implícita de obligatoriedad de las propiedades no anulables: cada campo obligatorio declara su atributo `Required`.
- El idioma de la solicitud llega en `Accept-Language` y **solo cambia el idioma de los mensajes**. La cultura de números y fechas de la API es invariante para toda solicitud.

## Alternativas consideradas

- **Archivos `.resx` o JSON embebidos.** Descartadas: cambiar un texto exigiría recompilar y volver a publicar.
- **Mensajes en la base de datos con edición desde el panel del Super Admin.** Pospuesta: es la evolución natural para el SaaS y solo exigiría cambiar el origen de los mensajes detrás de `IStringLocalizer`.
- **Solo códigos en la API, con traducción en cada cliente.** Descartada: duplica los diccionarios entre la web y la app.
- **FluentValidation.** Descartada para no sumar dependencias.

## Consecuencias

**Lo que se facilita**

- Un solo formato de error para todos los clientes.
- Textos editables en producción sin publicar.
- Agregar el inglés en el release 2 sin tocar código.

**Lo que exige cuidado**

- Con varias instancias de la API en la nube, los archivos deben vivir en un lugar compartido. Se resuelve al publicar en AWS.
- Toda clave nueva de `MessageKeys` debe existir en `es.json`; una prueba falla si falta.
- Todo modelo de entrada debe declarar sus mensajes con claves del diccionario.
