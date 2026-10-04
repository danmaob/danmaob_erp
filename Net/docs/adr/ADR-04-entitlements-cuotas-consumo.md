# ADR-04: Entitlements, cuotas y medición de consumo

**Estado:** Aceptado (2026-10-03)
**Fecha:** 2026-10-02
**Historia:** SP-03-1 (Épica E03 — Planes, módulos y cuotas)
**Requerimientos:** RF-007 a RF-011, RF-029 a RF-041, RF-121, RF-137, RF-142, RF-150, RNF-003, RNF-012, RNF-013, RNF-016, RNF-031
**Depende de:** ADR-00 (alcance por cuenta o por empresa) y ADR-01 (base compartida y esquemas)

## Contexto

"Módulo = qué puede hacer el sistema. Plan = cuánto puede utilizarlo." Ningún límite comercial puede quedar en el código (RNF-031). El Super Admin crea y modifica planes y define excepciones por empresa que ganan sobre el plan (RF-009 y RF-010).

Hay dos tipos de cuota con reglas distintas:

- **Cuotas de conteo:** empresas, usuarios, sucursales, almacenes, productos, clientes, prospectos y almacenamiento. Al llegar al límite se bloquea crear uno nuevo, sin afectar lo existente (RF-032).
- **Cuotas de consumo mensual:** documentos de venta, timbres y correos. Tienen aviso al 80 % y al 100 %, se bloquean al agotarse y se reinician el día 1 en hora del Centro de México (RF-033 y RF-034).

Además:

- El consumo con terceros solo se descuenta cuando el tercero confirmó, y nunca dos veces (RF-040).
- Dos operaciones simultáneas no pueden exceder una cuota (RNF-013).

## Decisión

### 1. Qué es código y qué es dato

| Elemento | Dónde vive | Motivo |
|---|---|---|
| Lista de módulos (`ModuleCode`) | Código | Un módulo existe porque existe su software |
| Lista de tipos de cuota (`QuotaCode`) con su tipo (conteo o consumo) y su alcance (cuenta o empresa) | Código | La forma de medir cada cuota es lógica del sistema |
| Planes, módulos incluidos por plan y límite de cada cuota por plan | Datos (catálogo `platform`) | Son decisiones comerciales |
| Excepciones por empresa o por cuenta | Datos | RF-010 |
| Paquetes adicionales | Datos | RF-011 |
| Regla de devolución al cancelar, por plan y cuota | Datos | Diferencia comercial entre Free y Básico (RF-036) |

Un límite vacío significa "ilimitado" (por ejemplo, los prospectos en Premium). Un límite 0 significa "no disponible".

### 2. Modelo de datos (catálogo `platform`)

| Entidad | Contenido |
|---|---|
| Plan | Nombre y estado (activo o desactivado). Baja lógica |
| Módulo del plan | Plan y código de módulo incluido |
| Cuota del plan | Plan, código de cuota, límite (vacío = ilimitado) y si se devuelve al cancelar |
| Excepción de módulo | Empresa, código de módulo y valor (habilitado o deshabilitado). Sin registro = sin excepción |
| Excepción de cuota | Cuenta o empresa (según el alcance de la cuota), código de cuota y límite personalizado |
| Paquete adicional | Cuenta o empresa, código de cuota de consumo, cantidad y periodo al que se suma |
| Registro de consumo | Ver la sección 5 |

El plan pertenece a la Cuenta Cliente (ADR-00). Todas sus empresas lo comparten.

### 3. Resolución del entitlement efectivo

- **Módulo habilitado para una empresa:** si existe una excepción de módulo, se usa su valor; si no, se usa la inclusión del plan de la cuenta.
- **Límite efectivo de una cuota:** si existe una excepción de cuota, se usa; si no, se usa el límite del plan. En las cuotas de consumo se suman los paquetes adicionales del periodo.
- Un único servicio de Application resuelve ambos. Ningún módulo lee planes ni excepciones por su cuenta.
- **Verificación de módulo en el backend (RNF-003):** cada endpoint operativo declara su módulo mediante una política de autorización. Si el módulo no está habilitado, la API responde 403 con un código de error de "módulo no contratado". El frontend usa ese mismo dato para mostrar el módulo bloqueado como invitación (RF-030), pero la decisión siempre es del backend.
- Datos de módulos deshabilitados (RF-031): no se borran. Sus endpoints quedan bloqueados por la política y sus datos no se exponen en otros módulos. Cómo se ocultan referencias cruzadas (por ejemplo, proveedores en un movimiento de inventario) se decide en cada historia.

### 4. Cuotas de conteo

- **No hay contadores guardados.** El uso se calcula contando los registros activos en el momento de crear uno nuevo. Así la baja lógica libera el lugar automáticamente (RF-037), un prospecto convertido deja de contar como prospecto y pasa a contar como cliente (RF-038), y no existe un contador que pueda desincronizarse.
- Cada cuota de conteo define en código qué cuenta. Por ejemplo, la cuota de usuarios cuenta los usuarios activos distintos de la cuenta, no los accesos (ADR-00, RF-039). El almacenamiento suma los bytes de los archivos activos de la cuenta.
- **Concurrencia (RNF-013):** la verificación y la creación ocurren en la misma transacción, después de adquirir un bloqueo de aplicación de SQL Server (`sp_getapplock`) con una llave formada por el código de cuota y el identificador de la cuenta o empresa. Dos altas simultáneas en la misma cuota y el mismo alcance se serializan; las de alcances distintos no se bloquean entre sí.
- Al llegar al límite, la API rechaza la creación con un código de error de "cuota alcanzada" que indica la cuota, y el frontend ofrece "Mejorar plan".

### 5. Cuotas de consumo mensual

**Libro de consumo.** Cada consumo es un registro en el catálogo con:

- alcance (cuenta o empresa) y código de cuota;
- **periodo** (año y mes);
- **llave de operación**, por ejemplo, el identificador de la operación de venta o del CFDI;
- cantidad;
- estado: reservado, confirmado o liberado.

Un índice único sobre (código de cuota, llave de operación) garantiza que una misma operación nunca se descuente dos veces (RF-040, RNF-012).

**Periodo y corte.** El periodo se calcula con el reloj de negocio en hora del Centro de México (TS-00-7, RNF-016). El uso del mes es la suma de los registros de ese periodo. **No existe un proceso de corte:** el día 1 empieza un periodo nuevo sin registros y el uso comienza en cero (RF-034).

**Concurrencia.** Igual que en las cuotas de conteo: un bloqueo de aplicación por cuota y alcance durante la verificación y el registro.

**Operaciones con terceros (timbres y correos), en dos fases:**

1. **Reservar** antes de llamar al tercero. La reserva cuenta contra el límite, para que dos timbrados simultáneos no lo excedan, pero no se informa como consumida.
2. **Confirmar** cuando el tercero responde con éxito. Solo entonces es consumo real (RF-040).
3. **Liberar** cuando el tercero rechaza de forma definitiva.

Una reserva sin respuesta (por ejemplo, el PAC no respondió) no se libera automáticamente: se resuelve consultando al tercero con la misma llave de operación. Ese mecanismo de conciliación se diseña con la integración del PAC (RF-120).

**Documentos de venta (RF-035 y RF-036):**

- La llave de operación es el identificador de la operación de venta: la cadena cotización → pedido → ventas. Toda la cadena consume un solo lugar.
- El lugar se consume cuando se confirma el primer documento de la cadena. Los borradores no consumen.
- Al cancelar, si la cuota del plan tiene "se devuelve al cancelar", el registro pasa a liberado. Si no, permanece confirmado. La cancelación nunca agrega consumo.
- El lugar se libera en el periodo en que se consumió. Una cancelación en un mes posterior no modifica el uso del mes en curso.

**Avisos (RF-033 y RF-142).** El porcentaje de uso se calcula al consultar los medidores y al registrar cada consumo. Cuando un consumo cruza el 80 % o el 100 %, se emite un evento de negocio (ADR-05) y el aviso se muestra en la pantalla de inicio. Cada umbral se avisa una sola vez por periodo.

**Comprobantes fiscales (RF-150).** Es una cuota de conteo de bytes con una diferencia: al llegar al límite solo avisa y **nunca bloquea** el timbrado. Esta excepción se modela como un atributo del tipo de cuota ("solo avisa"), no como un caso especial en la lógica de facturación.

### 6. Cambio de plan (RF-007, RF-008 y RF-041)

- El Super Admin cambia el plan de la cuenta. Ningún dato se borra.
- Antes de confirmar un cambio a un plan menor, el sistema calcula para cada cuota de conteo el uso actual contra el nuevo límite, y lista los excesos y los módulos que dejarían de estar habilitados.
- Lo que excede el nuevo límite queda en solo lectura hasta que el administrador decida qué conservar activo. El mecanismo concreto (estado de "excedido por plan" en cada entidad o una selección guardada por cuota) se diseña en la historia de RF-041, porque afecta a varias entidades.

## Pendientes para Luis (no bloquean el ADR)

1. **Cancelación de cotizaciones y pedidos en Free.** La regla aprobada dice que una *venta confirmada* cancelada no devuelve su lugar en Free. ¿Una operación que se cancela cuando solo llegó a cotización o pedido devuelve su lugar en Free? Este ADR lo deja como dato configurable, pero hay que fijar el valor inicial.
2. **Vigencia de los paquetes adicionales.** ¿Un paquete de timbres o correos asignado por el Super Admin vale solo para el mes en que se asigna, o se acumula hasta agotarse? Este ADR asume "solo el mes asignado" hasta que Luis decida.

## Alternativas consideradas

- **Contadores guardados para las cuotas de conteo.** Descartada. Se desincronizan con bajas, reactivaciones y conversiones, y obligan a mantenerlos en cada módulo. Contar con índices que empiezan por `TenantId` es suficiente para los volúmenes del plan.
- **Proceso programado que reinicia contadores el día 1.** Descartada. Con el periodo dentro de cada registro de consumo no hay nada que reiniciar, y no depende de un proceso que podría fallar o correr en otra zona horaria.
- **Descontar el timbre solo al confirmar, sin reserva.** Descartada. Dos timbrados simultáneos con un solo timbre disponible podrían confirmarse ambos y exceder el límite.
- **Concurrencia optimista con reintentos.** Descartada para las cuotas. Exige un número de versión en un registro que hoy no existe (no hay contadores) y lógica de reintento. El bloqueo de aplicación es más simple y su alcance es acotado.
- **Sistema genérico de feature flags para los niveles "básico, completo y avanzado".** Descartada (Instrucciones 5.1). Los niveles se modelan solo cuando exista una función concreta que los distinga.

## Consecuencias

**Lo que se facilita**

- Los planes, sus módulos y sus límites se modifican sin tocar código.
- El corte mensual no depende de procesos programados.
- La idempotencia está garantizada por un índice único, no por la disciplina de cada módulo.

**Lo que se complica o exige cuidado**

- Cada alta con cuota hace un conteo dentro de una transacción con bloqueo. Debe medirse contra los tiempos objetivo (RNF-020) cuando existan.
- `sp_getapplock` ata la solución a SQL Server. Es aceptable, porque es la base elegida para todos los modelos de despliegue.
- La conciliación de reservas sin respuesta es responsabilidad de la integración del PAC y del correo.

**Trabajo que queda desbloqueado**

- **TS-03-1:** datos semilla de planes, módulos y cuotas.
- **US-03-x:** resolución de entitlements, cuotas de conteo, cuota de documentos de venta y cambio de plan.
- **US-02-2:** el registro verifica las cuotas de empresas y usuarios.
- **RF-121** (bloqueo de timbrado) y **RF-137** (consumo de correos) quedan resueltos por la sección 5.
