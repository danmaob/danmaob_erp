# Especificación: lector de la Constancia de Situación Fiscal (CSF)

| | |
|---|---|
| Subtarea | TK-15-1 |
| Proyecto | DANMAOB_ERP — release 1 (incluido en plan Free) |
| Estado | PF validada con documento real · PM validada visualmente (riesgo residual aceptado) |
| Fecha | 2026-10-02 |

## 1. Propósito

Definir qué datos se extraen de la CSF, cómo se identifican en el texto del PDF y cómo se valida que el archivo corresponde al formato del SAT. El resultado alimenta una pantalla editable para dar de alta datos fiscales de la empresa emisora, clientes y proveedores.

Requerimientos ya confirmados que esta especificación respeta:

- El sistema valida el formato SAT, extrae los datos a una pantalla editable y rechaza el archivo si no corresponde.
- El archivo **no se almacena**.
- El Super Admin sube su propia CSF como formato base; si el SAT cambia el formato, puede habilitar captura manual global y subir la nueva como referencia.

## 2. Fuente y estado de validación

| Tipo | Fuente | Estado |
|---|---|---|
| Persona física (RFC de 13 caracteres) | CSF real descargada del portal del SAT el 2026-10-02, 3 páginas, tamaño carta, ~170 KB, `Producer: Oracle XML Publisher 5.6.2`, con capa de texto | **Validada** |
| Persona moral (RFC de 12 caracteres) | CSF real de 2 páginas emitida el 2026-10-01, pero generada con `Producer: Microsoft: Print To PDF` (imagen, **sin capa de texto**) | **Etiquetas y estructura validadas visualmente.** No fue posible obtener una CSF moral original; se acepta el riesgo residual de diferencias menores de espacios u orden en el texto extraído (sección 2.1) |

### 2.1 Riesgo residual aceptado (PM)

La extracción por etiquetas tolera diferencias de orden y de espacios, por lo que el riesgo es bajo. Se mitiga con el fixture sintético de PM en las pruebas unitarias y se confirma con la primera CSF moral original que se procese durante la validación local del release 1; si hay diferencias, se actualiza esta especificación y el fixture.

Los documentos reales no están ni deben estar en el repositorio. Todos los ejemplos de este archivo usan datos ficticios.

## 3. Reglas generales

1. **Solo capa de texto, sin OCR.** La CSF descargada del SAT trae texto seleccionable. Si el PDF no tiene texto extraíble (escaneo o foto), se rechaza.
2. **Procesamiento en memoria.** El archivo se lee del stream de la petición y se descarta. No se escribe a disco, BD ni almacenamiento de objetos.
3. **Sin datos personales en logs.** No se registra el texto extraído ni los valores. Solo se registran el resultado (aceptado/rechazado), el motivo del rechazo y el tipo de persona.
4. **Extracción por etiquetas, nunca por posición ni por número de línea.** El orden del texto puede variar entre librerías de extracción; las etiquetas no.
5. **Minimización.** Solo se extraen los datos que el ERP usa (sección 5). Actividades económicas y obligaciones no se extraen.

## 4. Estructura observada del documento

| Página | Contenido |
|---|---|
| 1 | Cédula de identificación fiscal (RFC, nombre, idCIF, QR); título "CONSTANCIA DE SITUACIÓN FISCAL"; lugar y fecha de emisión; código de barras con el RFC; **Datos de Identificación del Contribuyente**; **Datos del domicilio registrado** |
| 2 | Actividades Económicas; **Regímenes**; Obligaciones; leyendas; **Cadena Original Sello**; Sello Digital |
| Última | Segundo código QR (en la PF real quedó solo en la página 3; en la PM real, al final de la página 2) |

El número de páginas **no es fijo**: se observaron 3 páginas en PF y 2 en PM. Las tablas pueden partirse entre páginas (en la PM real, el encabezado de Actividades Económicas quedó en la página 1 y sus filas en la página 2). El lector no debe depender del número de página y debe descartar las líneas `Página N de M` antes de delimitar secciones.

## 5. Mapa de campos

### 5.1 Comunes (PF y PM)

| Campo destino | Etiqueta en el PDF | Sección | Obligatorio | Uso en el ERP |
|---|---|---|---|---|
| `Rfc` | `RFC:` | Datos de Identificación | Sí | Identificador fiscal; CFDI |
| `IdCif` | `idCIF:` | Cédula | Sí | Validación de formato |
| `NombreCedula` | Texto antes de `Nombre, denominación o razón social` | Cédula | Sí | Verificación cruzada del nombre |
| `FechaInicioOperaciones` | `Fecha inicio de operaciones:` | Datos de Identificación | No | Informativo |
| `EstatusPadron` | `Estatus en el padrón:` | Datos de Identificación | Sí | Ver decisión D2 |
| `FechaUltimoCambioEstado` | `Fecha de último cambio de estado:` | Datos de Identificación | No | Informativo |
| `NombreComercial` | `Nombre Comercial:` | Datos de Identificación | No (puede venir vacío) | Nombre visible en el ERP |
| `CodigoPostal` | `Código Postal:` | Domicilio | Sí | Domicilio fiscal del receptor en CFDI |
| `TipoVialidad` | `Tipo de Vialidad:` | Domicilio | No | Domicilio |
| `NombreVialidad` | `Nombre de Vialidad:` | Domicilio | No | Domicilio |
| `NumeroExterior` | `Número Exterior:` | Domicilio | No | Domicilio |
| `NumeroInterior` | `Número Interior:` | Domicilio | No (suele venir vacío) | Domicilio |
| `Colonia` | `Nombre de la Colonia:` | Domicilio | No | Domicilio |
| `Localidad` | `Nombre de la Localidad:` | Domicilio | No | Domicilio |
| `Municipio` | `Nombre del Municipio o Demarcación Territorial:` | Domicilio | No | Domicilio |
| `EntidadFederativa` | `Nombre de la Entidad Federativa:` | Domicilio | No | Domicilio |
| `EntreCalle` | `Entre Calle:` | Domicilio | No | Domicilio |
| `YCalle` | `Y Calle:` | Domicilio | No | Domicilio |
| `Regimenes` | Tabla `Regímenes:` | Regímenes | Sí (al menos uno vigente) | Régimen fiscal en CFDI (sección 7) |
| `FechaEmision` | Primer campo de `Cadena Original Sello:` | Cadena original | Sí | Ver decisión D3 |

### 5.2 Solo persona física (validado)

| Campo destino | Etiqueta en el PDF | Obligatorio |
|---|---|---|
| `Curp` | `CURP:` | Sí |
| `Nombres` | `Nombre (s):` | Sí |
| `PrimerApellido` | `Primer Apellido:` | Sí |
| `SegundoApellido` | `Segundo Apellido:` | No (hay personas con un solo apellido) |

Nombre fiscal para CFDI: `Nombres + " " + PrimerApellido + " " + SegundoApellido`, con espacios normalizados.

### 5.3 Solo persona moral (validado visualmente)

| Campo destino | Etiqueta en el PDF | Obligatorio |
|---|---|---|
| `RazonSocial` | `Denominación/Razón Social:` | Sí |
| `RegimenCapital` | `Régimen Capital:` | No |

Orden observado en Datos de Identificación: `RFC`, `Denominación/Razón Social`, `Régimen Capital`, `Nombre Comercial`, `Fecha inicio de operaciones`, `Estatus en el padrón`, `Fecha de último cambio de estado`. En PF, `Nombre Comercial` va al final; en PM, después de `Régimen Capital`. La extracción por etiquetas absorbe esta diferencia.

La persona moral no trae CURP ni apellidos. La denominación ya viene **sin** el régimen capital (este va en su propio campo), así que el nombre fiscal para CFDI es `RazonSocial` tal cual. El nombre de la cédula coincide con `RazonSocial`.

## 6. Reglas de extracción

1. **Normalización del texto:** Unicode NFC; convertir saltos de línea y espacios múltiples a un espacio simple al leer un valor; recortar espacios al inicio y al final.
2. **Etiquetas con distinción de mayúsculas.** Las etiquetas vienen en mayúsculas y minúsculas (`Nombre de Vialidad:`) y los valores en mayúsculas (`CALLE ...`). Comparar las etiquetas exactamente, con su acento y sus dos puntos, evita confundir un valor como `CALLE MORELOS` con la etiqueta `Y Calle:`.
3. **Delimitación del valor:** el valor empieza al terminar la etiqueta y termina donde empieza la **siguiente etiqueta conocida** o el fin de línea, lo que ocurra primero. En el domicilio vienen dos campos por línea:
   ```
   Código Postal:37000 Tipo de Vialidad: CALLE
   Número Interior: Nombre de la Colonia: CENTRO
   ```
   En la segunda línea, `Número Interior` queda vacío porque la siguiente etiqueta aparece de inmediato.
4. **Espacio después de los dos puntos opcional.** `Código Postal:37000` y `Número Interior:1` vienen sin espacio.
5. **Coincidencia de la etiqueta más larga.** Si dos etiquetas comparten texto, gana la más larga en esa posición.
6. **Valores con salto de línea.** El nombre en la cédula y el lugar y fecha de emisión pueden partirse en dos líneas (`... ORTIZ DE LA` / `HUERTA`). Se unen con un espacio.
7. **Fechas:**
   - Datos de identificación: texto en español, `25 DE MARZO DE 2002`.
   - Tablas: `dd/MM/yyyy`.
   - Cadena original: `yyyy/MM/dd HH:mm:ss`.
8. **Campo vacío ≠ campo ausente.** Si la etiqueta existe y el valor está vacío, el campo es `null`. Si una etiqueta obligatoria no existe, es error de formato.

## 7. Regímenes

La tabla tiene columnas `Régimen`, `Fecha Inicio` y `Fecha Fin`. Texto observado:

```
Régimen Fecha Inicio Fecha Fin
Régimen de Sueldos y Salarios e Ingresos Asimilados a Salarios 01/01/2010
Régimen de las Personas Físicas con Actividades Empresariales y Profesionales 01/01/2014
```

Reglas:

1. Delimitar la sección entre `Regímenes:` y `Obligaciones:`.
2. Identificar cada régimen por **coincidencia con la tabla de equivalencias** (no por posición de columnas). Después del nombre vienen una fecha (régimen **vigente**) o dos fechas (régimen **terminado**).
3. Solo se cargan los regímenes vigentes. Si no hay ninguno vigente, se rechaza (ver sección 8).
4. Un nombre no reconocido no rechaza el documento: se muestra al usuario como "régimen no reconocido" para que lo elija del catálogo en la pantalla editable.

Tabla de equivalencias con el catálogo `c_RegimenFiscal` del SAT:

| Texto en la CSF | Clave | Estado |
|---|---|---|
| Régimen de Sueldos y Salarios e Ingresos Asimilados a Salarios | 605 | Validado |
| Régimen de las Personas Físicas con Actividades Empresariales y Profesionales | 612 | Validado |
| Régimen General de Ley Personas Morales | 601 | Validado visualmente |
| Demás regímenes | — | Se agregan conforme se validen con documentos reales |

Los nombres de la CSF no son idénticos a las descripciones del catálogo (la CSF agrega "Régimen de" o "Régimen de las"). Por eso se usa esta tabla y no una comparación directa.

## 8. Validación de formato y rechazo

### 8.1 Huella de formato (elementos comunes a PF y PM)

El documento es una CSF válida si cumple **todo** lo siguiente:

1. Tiene texto extraíble.
2. Contiene los textos: `CONSTANCIA DE SITUACIÓN FISCAL`, `CÉDULA DE IDENTIFICACIÓN FISCAL`, `idCIF:`, `Datos de Identificación del Contribuyente:`, `Datos del domicilio registrado`, `Regímenes:`, `Cadena Original Sello:`.
3. El RFC cumple el patrón del SAT: 13 caracteres para PF y 12 para PM. **La longitud del RFC determina el mapa de campos** (5.2 o 5.3).
4. El RFC es el mismo en la cédula, en `RFC:` y en el segundo campo de la cadena original.
5. La cadena original contiene `CONSTANCIA DE SITUACIÓN FISCAL`.
6. El nombre coincide con `NombreCedula`, normalizando espacios: en PF, el nombre armado (5.2); en PM, `RazonSocial`. En PF, además, la CURP cumple su patrón.
7. El código postal tiene 5 dígitos.

La huella **no** usa etiquetas exclusivas de PF o PM, para no rechazar a uno por tener la estructura del otro.

### 8.2 Validación de archivo (antes de extraer)

- Tipo `application/pdf` y encabezado `%PDF`.
- Tamaño máximo propuesto: 2 MB (el real pesa ~170 KB).
- Máximo de páginas propuesto: 10.

### 8.3 Mensajes al usuario

| Caso | Mensaje |
|---|---|
| No es PDF, excede tamaño o páginas | "El archivo no es válido. Sube la Constancia de Situación Fiscal en PDF, tal como la descargas del SAT." |
| PDF sin texto extraíble | "No pudimos leer el texto del archivo. Descarga la Constancia directamente del portal del SAT; las escaneadas, fotografiadas o reimpresas como PDF no se pueden leer." |
| Falla la huella de formato | "El archivo no parece ser una Constancia de Situación Fiscal vigente del SAT. Verifica el documento o captura los datos manualmente." |
| Sin régimen vigente | "La Constancia no tiene ningún régimen fiscal vigente. Verifica tu situación con el SAT." |

El detalle técnico del rechazo va al log, sin datos personales; nunca se muestra al usuario.

## 9. Formato base del Super Admin

- La huella de la sección 8.1 es la referencia del formato vigente.
- Cuando el Super Admin sube una CSF como base, el sistema verifica que cumple la huella y registra la versión de referencia (fecha de carga y `Producer` del PDF). La CSF base tampoco se almacena.
- Si el SAT cambia el formato y las CSF reales empiezan a fallar, el Super Admin habilita la captura manual global mientras se actualiza esta especificación y el lector.

## 10. Fuera de alcance (release 1)

- OCR de documentos escaneados o fotografiados.
- Verificación en línea contra el SAT mediante el código QR.
- Lectura del QR o del código de barras (ver decisión D4).
- Extracción de actividades económicas y obligaciones.
- Contribuyentes extranjeros.

## 11. Decisiones pendientes (Luis)

| Id | Pregunta | Propuesta |
|---|---|---|
| D1 | Si un cliente tiene varios regímenes vigentes, ¿se guardan todos o se elige uno? | Guardar todos y que el usuario marque el predeterminado para facturar |
| D2 | ¿Qué pasa si el estatus en el padrón no es ACTIVO ni REACTIVADO (por ejemplo, SUSPENDIDO o CANCELADO)? | Aceptar con advertencia visible; no bloquear el alta |
| D3 | ¿Se limita la antigüedad de la CSF? | Advertir si tiene más de 3 meses; no rechazar |
| D4 | ¿Se valida el QR en release 1? | No: requiere convertir la página a imagen y decodificar el QR, lo que agrega dos dependencias. La verificación cruzada del RFC por texto (8.1, punto 4) cubre el caso principal |
| D5 | Tamaño y páginas máximas | 2 MB y 10 páginas |
| D6 | Las CSF reimpresas con "Imprimir a PDF" o escaneadas no tienen texto y se rechazan. Como todo cliente facturable se da de alta con Constancia, ese cliente no podrá facturarse hasta entregar el original. ¿Se acepta esa consecuencia? | Sí en release 1, con el mensaje de la sección 8.3; evaluar OCR después según la frecuencia observada de rechazos |

## 12. Hallazgos del QR (referencia, no se implementa en release 1)

Se decodificaron los dos QR del documento real:

- Primera página: `https://siat.sat.gob.mx/app/qr/faces/pages/mobile/validadorqr.jsf?D1=10&D2=1&D3={idCIF}_{RFC}`. El `D3` coincide con el `idCIF` y el RFC del texto.
- Última página: misma URL con `D1=26` y un `D3` con otro número más el RFC. El significado de ese número no está confirmado. Mismo patrón observado en la PM real.

## 13. Ejemplos de texto extraído (datos ficticios)

### 13.1 Persona física (estructura real, valores ficticios)

```
CÉDULA DE IDENTIFICACIÓN FISCAL
PEGM850101AB1
Registro Federal de
Contribuyentes
MARIA PEREZ GOMEZ
Nombre, denominación o razón
social
idCIF: 12345678901
VALIDA TU INFORMACIÓN
FISCAL
CONSTANCIA DE SITUACIÓN FISCAL
Lugar y Fecha de Emisión
LEON , GUANAJUATO A 02 DE OCTUBRE DE
2026
PEGM850101AB1
Datos de Identificación del Contribuyente:
RFC: PEGM850101AB1
CURP: PEGM850101MGTRMR09
Nombre (s): MARIA
Primer Apellido: PEREZ
Segundo Apellido: GOMEZ
Fecha inicio de operaciones: 15 DE ENERO DE 2010
Estatus en el padrón: ACTIVO
Fecha de último cambio de estado: 15 DE ENERO DE 2010
Nombre Comercial:
Datos del domicilio registrado
Código Postal:37000 Tipo de Vialidad: CALLE
Nombre de Vialidad: CALLE HIDALGO Número Exterior: 100
Número Interior: Nombre de la Colonia: CENTRO
Nombre de la Localidad: LEON Nombre del Municipio o Demarcación Territorial: LEON
Nombre de la Entidad Federativa: GUANAJUATO Entre Calle: JUAREZ
Y Calle: MADERO
...
Regímenes:
Régimen Fecha Inicio Fecha Fin
Régimen de las Personas Físicas con Actividades Empresariales y Profesionales 01/01/2014
Obligaciones:
...
Cadena Original Sello: ||2026/10/02 10:00:00|PEGM850101AB1|CONSTANCIA DE SITUACIÓN
FISCAL|000000000000000000000|XXXXXXXX||
```

### 13.2 Persona moral (estructura real observada en imagen, valores ficticios)

```
Datos de Identificación del Contribuyente:
RFC: EEJ200101AB1
Denominación/Razón Social: EMPRESA EJEMPLO
Régimen Capital: SOCIEDAD ANONIMA DE CAPITAL VARIABLE
Nombre Comercial:
Fecha inicio de operaciones: 01 DE ENERO DE 2020
Estatus en el padrón: ACTIVO
Fecha de último cambio de estado: 01 DE ENERO DE 2020
Datos del domicilio registrado
Código Postal:37000 Tipo de Vialidad: CALLE
Nombre de Vialidad: BOULEVARD EJEMPLO Número Exterior: 200
Número Interior:1 Nombre de la Colonia: CENTRO
Nombre de la Localidad: Nombre del Municipio o Demarcación Territorial: LEON
Nombre de la Entidad Federativa: GUANAJUATO Entre Calle: JUAREZ
Y Calle: MADERO
...
Regímenes:
Régimen Fecha Inicio Fecha Fin
Régimen General de Ley Personas Morales 01/01/2020
```

El texto se reconstruyó a partir de la imagen del documento, con el mismo orden de lectura que la PF. Se reemplaza por texto extraído si se obtiene una CSF moral original (sección 2.1).
