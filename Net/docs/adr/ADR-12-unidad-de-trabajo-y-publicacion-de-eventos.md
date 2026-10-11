# ADR-12: Unidad de trabajo y publicación de eventos de negocio

**Estado:** Aceptado (2026-10-11)
**Fecha:** 2026-10-11
**Historia:** TS-00-9 (Épica E00 — Fundación técnica y arquitectura)
**Requerimientos:** RNF-039
**Depende de:** ADR-05 (eventos de negocio), ADR-06 (persistencia), ADR-07 (aislamiento por empresa), ADR-11 (reloj de negocio)

## Contexto

El ADR-05 decidió que los eventos de negocio se publican dentro del proceso, una sola vez y solo después de que la operación se guardó con éxito, "en un único punto explícito de Application". Ese punto no existía: Application no puede referenciar Entity Framework Core (las pruebas de arquitectura lo impiden) y ningún ADR había decidido cómo un caso de uso confirma sus cambios.

TS-00-9 es la primera historia que lo necesita, y el registro autónomo (US-02-2) será el primer caso de uso que lo use.

Este ADR precisa al ADR-05 sin reemplazarlo.

## Decisión

### 1. Unidad de trabajo como único punto de confirmación

- `IUnitOfWork` (`DanmaobErp.Application.Persistence`) tiene un solo método asíncrono, `SaveChangesAsync(CancellationToken)`. Es la única forma en que un caso de uso confirma una operación.
- `UnitOfWork` (`DanmaobErp.Infrastructure.Persistence`) lo implementa en este orden:
  1. Guarda con `ErpDbContext.SaveChangesAsync`, que sigue aplicando la protección de empresa del ADR-07.
  2. Si el guardado falla, descarta los eventos pendientes y deja pasar la excepción original.
  3. Si tiene éxito, toma una copia de los pendientes, vacía la colección y los publica.
- Atar la publicación a la confirmación impide que un caso de uso la olvide, con el mismo criterio del ADR-07: lo que se puede omitir, tarde o temprano se omite.
- `ErpDbContext` no cambia: ni su constructor ni sus sobrescrituras.
- Los repositorios por agregado, si hacen falta, se deciden en US-02-1.

### 2. Eventos pendientes

- El caso de uso registra sus eventos con `IPendingBusinessEvents.Add` (`DanmaobErp.Application.Events`), antes de llamar a la unidad de trabajo.
- `PendingBusinessEvents` (`DanmaobErp.Infrastructure.Events`) es la colección: una instancia por solicitud. La interfaz y la clase concreta se registran como la misma instancia, para que la unidad de trabajo publique exactamente lo que el caso de uso registró.
- En el release 1 las entidades no acumulan eventos. El ADR-05 permite ambas vías y esta no modifica `Entity`.
- Un segundo guardado en la misma solicitud nunca vuelve a publicar un evento ya publicado (TS-00-9, CA1).

### 3. Contrato del evento

- `BusinessEvent` (`DanmaobErp.Domain.Common`) es la clase base abstracta de todo evento. Sus cuatro propiedades son de solo lectura:
  - `EventId`: Guid v7, asignado en el constructor;
  - `OccurredAt`: fecha y hora en UTC;
  - `TenantId`: la empresa, **anulable**;
  - `UserId`: el usuario, anulable.
- Domain no puede leer el reloj ni la empresa, así que el caso de uso los pasa al constructor. La fecha sale de `IBusinessClock.UtcNow` (ADR-11). El constructor rechaza una fecha que no esté en UTC, para que nunca se guarde por error la hora local de `IBusinessClock.Now`.
- La empresa es anulable por dos razones: el registro autónomo ocurre sin empresa resuelta, aunque su evento puede llevar la empresa recién creada; y habrá eventos de plataforma que no pertenecen a una sola empresa, como la suspensión de una cuenta por el Super Admin.
- Cada evento concreto es una clase sellada, en pasado y en inglés, con propiedades de solo lectura que contienen solo identificadores y valores simples, nunca entidades. Se declara en Domain, junto a las entidades de su módulo.
- Un evento es serializable a JSON con sus nombres de propiedad, lo que deja preparado el paso futuro a una tabla de salida (outbox, ADR-05 sección 5).

### 4. Suscriptores y despachador

- Los suscriptores implementan `IBusinessEventHandler<TEvent>` (`DanmaobErp.Application.Events`). Cada módulo registra los suyos en su historia.
- `BusinessEventDispatcher` (`DanmaobErp.Infrastructure.Events`) vive en Infrastructure porque necesita el contenedor de servicios, que Application no referencia. Por cada evento obtiene los suscriptores registrados para su tipo real y los ejecuta en orden de registro, uno después de otro.
- Si un suscriptor falla, la falla se registra en el log, en inglés, con el tipo del suscriptor, el tipo del evento y su identificador. El despachador continúa con el siguiente suscriptor y nunca propaga la excepción a la operación ya guardada.
- **En el release 1 los suscriptores no guardan datos.** Así no hay publicación anidada ni un segundo guardado dentro de la publicación. La regla del ADR-05 sección 4 sigue vigente: ningún efecto que forme parte de la consistencia del negocio se implementa como suscriptor.

### 5. Registro

- `AddUnitOfWork` registra la colección, el despachador y la unidad de trabajo con alcance por solicitud. Se llama en `Program.cs` justo después de `AddPersistence`, porque la unidad de trabajo depende de `ErpDbContext`.
- Una prueba de la API confirma que la unidad de trabajo se resuelve del contenedor y que la interfaz y la colección concreta son la misma instancia.

### 6. Relación con la auditoría (TS-14-1)

El flujo completo de guardado queda así:

1. Protección de empresa (ADR-07), dentro de las sobrescrituras de `SaveChanges`.
2. Captura de auditoría (TS-14-1, se decide en su ADR).
3. Guardado.
4. Publicación de eventos, solo si el guardado tuvo éxito, en la unidad de trabajo.

Este ADR no decide dónde se captura la auditoría; solo fija que la publicación queda fuera del `DbContext` y después del guardado.

## Alternativas consideradas

- **Publicar desde las sobrescrituras de `SaveChanges` de `ErpDbContext`.** Descartada. Obligaría a cambiar el constructor del contexto para recibir el despachador, mezclaría la publicación con la persistencia y el ADR-05 ya descartó publicar desde el guardado de EF Core.
- **Que cada caso de uso llame al despachador después de guardar.** Descartada. Un caso de uso que olvide la llamada pierde sus eventos sin ningún error, que es el mismo tipo de falla que el ADR-07 evita.
- **Eventos acumulados en las entidades y leídos del `ChangeTracker`.** Descartada para el release 1. Exige cambiar `Entity` y recorrer el rastreador; la colección por solicitud basta. Puede agregarse después sin cambiar los suscriptores.
- **Sobre aparte con los metadatos (envoltura del evento) llenado por la colección.** Descartada. La empresa no siempre está resuelta (registro autónomo), así que la colección no podría llenarla de forma confiable; pasar los datos al constructor es explícito y verificable.
- **Despachador en Application.** Descartada. Necesita el contenedor de servicios, y Application no referencia `Microsoft.Extensions.DependencyInjection`.

## Consecuencias

**Lo que se facilita**

- Los casos de uso confirman siempre por el mismo camino, y la publicación no se puede olvidar.
- Los módulos declaran eventos sin depender de infraestructura.
- La capa futura de automatización se conecta como un suscriptor más.

**Lo que se complica o exige cuidado**

- Todo caso de uso que guarde debe usar `IUnitOfWork`. Un servicio que llame directo a `ErpDbContext.SaveChangesAsync` no publica eventos.
- El despachador resuelve los suscriptores por reflexión sobre el tipo real del evento. Un suscriptor registrado para una clase base no recibe los eventos derivados.
- Los suscriptores se ejecutan dentro de la solicitud; uno lento alarga la respuesta (ADR-05).

**Trabajo que queda desbloqueado**

- **TS-14-1:** la auditoría se diseña con el flujo de guardado completo a la vista.
- **US-02-2:** el registro autónomo confirma con la unidad de trabajo.
- **ADR-04 y RF-136:** los avisos de cuota y los avisos internos se implementan como suscriptores.
