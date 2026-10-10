# ADR-11: Reloj de negocio en hora del Centro de México

**Estado:** Aceptado (2026-10-10)
**Fecha:** 2026-10-10 (registra decisiones tomadas en TS-00-7)
**Historia:** TS-00-7 (Épica E00 — Fundación técnica y arquitectura)
**Requerimientos:** RNF-016
**Depende de:** ADR-04 (corte mensual de cuotas), ADR-05 (fecha de los eventos)

## Contexto

Los cortes mensuales de cuotas, las fechas de los documentos y los vencimientos se calculan en hora del Centro de México, mientras que el servidor puede estar en UTC. A las 23:30 del último día del mes en México ya es el día siguiente en UTC; usar la hora del servidor cerraría el mes antes de tiempo.

## Decisión

- La hora actual sale siempre de `IBusinessClock` (Application), implementado por `BusinessClock` (Infrastructure) sobre `TimeProvider` de .NET, sin paquetes nuevos.
- `UtcNow` es el instante que se guarda en la base de datos. `Now` y `Today` dan la hora y la fecha de negocio en `America/Mexico_City` (UTC-6, sin horario de verano desde 2022).
- La zona es una constante, porque la Síntesis la fija como decisión de producto.
- El reloj se crea al arrancar: la API no arranca si el servidor no tiene la zona horaria. No hay zona de respaldo ni desfase fijo.
- Una prueba de arquitectura recorre `src/` y falla si se usa `DateTime.Now`, `DateTime.UtcNow`, `DateTimeOffset.Now` o `DateTimeOffset.UtcNow`.
- Las pruebas controlan el tiempo con un `TimeProvider` de prueba propio.

## Alternativas consideradas

- **Zona configurable.** Descartada: no hay un caso de negocio para otra zona en el release 1.
- **Paquete `Microsoft.Extensions.TimeProvider.Testing`.** Descartado para no sumar dependencias.
- **Solo una regla escrita, sin prueba.** Descartada: el corte mensual no tolera un uso accidental del reloj del sistema.

## Consecuencias

- Las imágenes de Linux para la nube deben incluir los datos de zonas horarias.
- Todo cálculo de fecha de negocio pasa por el reloj, lo que lo hace comprobable con pruebas.
