# ADR-05: Eventos de negocio para futuras automatizaciones

**Estado:** Aceptado (2026-10-03)
**Fecha:** 2026-10-02
**Historia:** SP-00-2 (Épica E00 — Fundación técnica y arquitectura)
**Requerimientos:** RNF-039, RF-033, RF-136
**Depende de:** ADR-01 (base compartida y empresa resuelta por solicitud)

## Contexto

Las automatizaciones son una extensión futura: una capacidad transversal que ejecutará acciones ante eventos de negocio. No son un módulo central. El release 1 no construye un motor de automatización ni infraestructura de mensajería, pero debe dejar definidos los eventos de negocio para que esa capa futura reaccione a ellos **sin modificar los módulos** (RNF-039).

Las Instrucciones (sección 17) prohíben introducir colas, event sourcing, CQRS o MediatR sin una necesidad demostrada. El release 1 también tiene usos internos inmediatos:

- los avisos de cuota al 80 % y al 100 % (ADR-04);
- los avisos internos por correo, por ejemplo, un producto bajo el mínimo (RF-136).

## Decisión

### 1. Qué es un evento de negocio

Un evento de negocio es un hecho ya ocurrido y confirmado:

- Se nombra en pasado y en inglés, como los demás tipos del código. Por ejemplo, `SaleConfirmed`, `InvoiceStamped`, `StockBelowMinimum`, `PurchaseReceived`, `CustomerCreated` y `QuotaThresholdReached`.
- Es inmutable y serializable.
- Contiene solo:
  - un identificador único del evento;
  - la fecha y hora en que ocurrió (reloj de negocio, TS-00-7);
  - la empresa (`TenantId`);
  - el usuario que lo provocó, si existe;
  - los **identificadores** de las entidades involucradas y los datos mínimos del hecho, por ejemplo, la cantidad y el almacén en una existencia bajo el mínimo.
- Nunca lleva objetos de entidad completos. Quien reaccione consulta lo que necesite.

Cada módulo declara sus eventos en Domain, junto a sus entidades. El catálogo inicial de eventos se completa en cada historia que los emite.

### 2. Emisión

- La entidad o el servicio de Application que ejecuta la operación registra el evento en una colección de eventos pendientes durante la operación.
- **Los eventos se publican solo después de que la transacción se guardó con éxito.** Si el guardado falla, los eventos se descartan. Así un evento nunca anuncia algo que no ocurrió (TS-00-9, CA1).
- Cada evento se publica una sola vez. Después de publicarlos, la colección de pendientes se vacía.

### 3. Publicación y suscriptores

- La publicación es **dentro del mismo proceso y en la misma solicitud**, con la misma empresa resuelta.
- Un despachador propio, sin MediatR, obtiene de la inyección de dependencias todos los suscriptores registrados para el tipo de evento y los ejecuta en orden.
- Los suscriptores se declaran mediante una interfaz genérica por tipo de evento, en Application. Infrastructure puede implementar suscriptores que usen servicios externos, como el correo.
- Si un suscriptor falla, la falla se registra en el log con el identificador del evento y no afecta a la operación ya guardada ni a los demás suscriptores.

### 4. Regla principal: los eventos no sustituyen a la operación

**Ningún efecto que forme parte de la consistencia del negocio se implementa como suscriptor.** El descuento de inventario, la cuenta por cobrar o el consumo de cuota se hacen dentro de la misma transacción, por el caso de uso que ejecuta la operación.

Los suscriptores del release 1 solo realizan efectos secundarios no críticos e idempotentes, como un aviso interno o una notificación.

Esta regla permite que en el release 1 un evento se pierda (por ejemplo, si el proceso termina entre el guardado y la publicación) sin dejar datos inconsistentes.

### 5. Camino hacia las automatizaciones (no se implementa en el release 1)

Cuando entre la capa de automatización, se agregará una tabla de salida de eventos (outbox) que se escribe en la misma transacción que la operación, y un proceso que la publica con reintentos.

Los eventos ya cumplen lo necesario para ese paso: son serializables y tienen un identificador único para descartar duplicados. Los módulos no cambian: siguen registrando sus eventos igual, y solo cambia el despachador.

Los eventos que dependen del paso del tiempo y no de una operación (por ejemplo, "factura vencida") requieren un proceso programado. Quedan fuera del release 1 y se diseñarán con la capa de automatización.

## Alternativas consideradas

- **MediatR para notificaciones.** Descartada por licenciamiento y por la decisión previa del proyecto (Instrucciones 9.1).
- **Cola o broker de mensajes** (RabbitMQ, SQS u otro). Descartada. RNF-039 excluye la infraestructura de mensajería en el release 1, y SQS ataría el producto a AWS (incumple RNF-023).
- **Outbox desde el release 1.** Descartada por ahora. Agrega una tabla, un proceso de fondo y reintentos sin un consumidor que los necesite. La regla de la sección 4 hace que la pérdida de un evento sea tolerable hasta entonces.
- **Publicar los eventos desde un interceptor de `SaveChanges`.** Descartada. Mezclaría la publicación con la auditoría (TS-14-1), que también usa un interceptor, y los eventos registrados en servicios de Application no pasan por el `ChangeTracker`. La publicación ocurre en un único punto explícito de Application, después de guardar.
- **Event sourcing.** Descartada. No hay necesidad demostrada (Instrucciones 17).

## Consecuencias

**Lo que se facilita**

- Cada módulo anuncia sus hechos importantes desde el primer día.
- La capa de automatización futura se conecta como un suscriptor más, sin tocar los módulos.
- Los avisos internos y los avisos de cuota del release 1 usan el mismo mecanismo.

**Lo que se complica o exige cuidado**

- En el release 1 los eventos pueden perderse si el proceso termina entre el guardado y la publicación. Es aceptable únicamente por la regla de la sección 4, que debe revisarse en cada historia que agregue un suscriptor.
- Los suscriptores se ejecutan dentro de la solicitud y alargan su tiempo de respuesta. Un suscriptor lento, como el envío de un correo, debe diseñarse con cuidado en su historia.

**Trabajo que queda desbloqueado**

- **TS-00-9:** declaración de eventos, colección de pendientes, despachador y suscriptor de prueba.
- **ADR-04:** el evento de umbral de cuota.
- **RF-136:** los avisos internos por correo como suscriptores.
