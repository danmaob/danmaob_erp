# Especificación: costeo de inventario (PEPS y Promedio) con existencias negativas

| | |
|---|---|
| Subtarea | TK-15-2 |
| Proyecto | DANMAOB_ERP — release 1 |
| Estado | Decisiones principales confirmadas · decisiones menores pendientes (sección 9) |
| Fecha | 2026-10-02 |

## 1. Decisiones confirmadas

1. Métodos permitidos: **PEPS (predeterminado)** y **Costo Promedio**. **UEPS descartado.**
2. El método se elige al iniciar la empresa y **no se puede cambiar después**.
3. **Se aceptan existencias negativas por razones operativas**, marcadas como tales.
4. Una salida sin existencia toma como costo provisional el **último costo conocido** del producto.
5. El inventario se descuenta al **entregar**; la Venta de Mostrador descuenta en el momento.
6. Los pedidos apartan existencias, pero **el apartado no mueve costo**.
7. Cancelar una venta entregada regresa la mercancía al inventario.
8. Los servicios no generan inventario ni costo.
9. El kardex es una consulta disponible en todos los planes, incluido Free.

## 2. Principio contable

Un inventario negativo no es un saldo válido: el inventario es un activo y no puede haber menos de cero unidades. El sistema lo acepta como **estado operativo transitorio** (por ejemplo, vender antes de capturar la recepción) y está obligado a:

- Identificar cada salida que se hizo sin existencia.
- Corregir su costo cuando llegue la entrada que la cubre.
- Mantener visibles los productos en negativo hasta que se regularicen.

Esto evita dos distorsiones: utilidad sobrestimada por salidas mal costeadas y, en Promedio, un costo unitario contaminado que se arrastra a todas las salidas futuras.

## 3. Ámbito del costo

El costo se lleva **por producto y por almacén** (ver decisión D1). Cada almacén pertenece a una sucursal.

## 4. Salida con existencia suficiente

- **PEPS:** consume capas en orden de llegada (fecha y hora de la entrada; ante empate, orden de registro).
- **Promedio:** sale al costo promedio vigente; el promedio no cambia por una salida.

## 5. Salida sin existencia suficiente

1. Se consume lo disponible con la regla normal (sección 4).
2. El faltante sale con **costo provisional = último costo conocido** del producto en ese almacén.
3. Ese faltante queda registrado como **pendiente de liquidar**: cantidad, costo provisional y referencia a la salida que lo originó.
4. El producto queda **marcado como negativo** en ese almacén.

Si el producto nunca ha tenido costo (no hay último costo conocido), aplica la decisión D2.

## 6. Entrada cuando hay negativo pendiente (liquidación)

Toda entrada (compra, recepción parcial, ajuste positivo, devolución) liquida **primero** el negativo pendiente, en orden de antigüedad:

1. Por cada unidad liquidada se calcula la diferencia: `costo de la entrada − costo provisional`.
2. Esa diferencia genera un **movimiento de ajuste de costo** vinculado a la salida original, de modo que el costo de lo vendido de esa venta queda corregido.
3. Las unidades restantes de la entrada se registran normalmente: nueva capa en PEPS o recálculo del promedio en Promedio.
4. Si la entrada no alcanza a cubrir todo el negativo, se liquida parcialmente y el resto sigue pendiente con su costo provisional.
5. Cuando la existencia vuelve a ser ≥ 0, se quita la marca de negativo.

### 6.1 Ejemplo PEPS

| Movimiento | Unidades | Costo unitario | Resultado |
|---|---|---|---|
| Compra | +10 | 100 | Capa: 10 @ 100 |
| Venta | −15 | — | 10 salen @ 100; 5 salen @ 100 provisional; existencia −5 |
| Compra | +10 | 120 | Liquida 5: ajuste 5 × (120 − 100) = **+100** al costo de la venta; nueva capa: 5 @ 120 |

### 6.2 Ejemplo Promedio

| Movimiento | Unidades | Valor | Promedio |
|---|---|---|---|
| Saldo | 10 | 1,000 | 100 |
| Venta −15 (5 a costo provisional 100) | −5 | −500 | — (negativo) |
| Compra 10 @ 120: liquida 5 → ajuste **+100** | 5 | 600 | **120** |

Cálculo **incorrecto** que no debe implementarse: sumar la compra al saldo negativo (−500 + 1,200 = 700 para 5 unidades → promedio 140). Sobrestima el inventario en 100 y subestima el costo de lo vendido en 100.

### 6.3 Regla de protección en Promedio

El promedio solo se recalcula cuando la existencia resultante es **mayor que cero**. Con existencia cero o negativa, el promedio vigente se conserva y no se divide entre cero ni entre un número negativo.

## 7. Cancelación de venta entregada

La mercancía regresa **al mismo costo con que salió**, no al costo vigente:

- **PEPS:** se crea una capa con ese costo.
- **Promedio:** entra con ese costo y se recalcula el promedio (aplicando 6.3).
- Si la salida original tenía unidades a costo provisional aún no liquidadas, al cancelarla se elimina ese pendiente en lugar de crear una entrada.
- Si la existencia está en negativo al momento de la cancelación, la entrada liquida primero el negativo (sección 6).

## 8. Visibilidad

- **Kardex:** muestra por movimiento la cantidad, el costo, la existencia resultante, la marca "costo provisional" en salidas sin existencia y los movimientos de ajuste con referencia a la venta corregida.
- **Pendientes (pantalla de inicio):** lista de productos con existencia negativa por almacén.
- **Venta y pedido:** al entregar sin existencia suficiente se avisa del faltante antes de confirmar (consistente con la regla de pedidos con faltantes).

## 9. Decisiones pendientes (Luis)

| Id | Pregunta | Propuesta |
|---|---|---|
| D1 | ¿El costo se lleva por almacén o por empresa? | Por almacén; un traspaso sale al costo del almacén origen y entra con ese mismo costo al destino |
| D2 | Producto sin último costo conocido que sale en negativo | Usar el costo capturado en el alta del producto; si no tiene, costo provisional 0 con advertencia en Pendientes |
| D3 | ¿Se limita cuánto tiempo puede estar un producto en negativo? | Sin límite en release 1; solo visibilidad en Pendientes |
| D4 | Precisión decimal | Costo unitario con 6 decimales; importes con 2, redondeo a la mitad hacia arriba |
| D5 | ¿Bloquear algún cierre mientras haya negativos? | No aplica en release 1 (no hay cierre contable); revisar cuando exista cierre de periodo |

## 10. Fuera de alcance (release 1)

- UEPS, costo estándar y costo identificado.
- Cambio de método de costeo después de iniciar la empresa.
- Devoluciones por defecto o daño (release posterior).
- Pólizas contables del ajuste de costo.
- Multimoneda en costos (release 1 solo MXN).
