const db = require("../db/connection");

const ESTADOS = ["pendiente", "pagado", "enviado", "entregado", "cancelado"];

class StockInsuficienteError extends Error {
  constructor(producto) {
    super(`No hay stock suficiente de "${producto}".`);
    this.name = "StockInsuficienteError";
  }
}

const Pedido = {
  ESTADOS,
  StockInsuficienteError,

  /**
   * Crea un pedido con sus ítems y descuenta stock en una sola transacción.
   * @param {object} cliente  { nombre, email, telefono, direccion, notas }
   * @param {Array}  items    [{ producto, cantidad }] con el producto ya leído de la base
   */
  crear(cliente, items) {
    const descontarStock = db.prepare(
      "UPDATE productos SET stock = stock - ? WHERE id = ? AND stock >= ?"
    );
    const insertarPedido = db.prepare(`
      INSERT INTO pedidos (nombre, email, telefono, direccion, notas, total)
      VALUES (@nombre, @email, @telefono, @direccion, @notas, @total)
    `);
    const insertarItem = db.prepare(`
      INSERT INTO pedido_items (pedido_id, producto_id, nombre, precio_unitario, cantidad)
      VALUES (?, ?, ?, ?, ?)
    `);

    return db.transaction(() => {
      const total = items.reduce((s, i) => s + i.producto.precio * i.cantidad, 0);
      const pedidoId = insertarPedido.run({ ...cliente, total }).lastInsertRowid;
      for (const { producto, cantidad } of items) {
        const r = descontarStock.run(cantidad, producto.id, cantidad);
        if (r.changes === 0) throw new StockInsuficienteError(producto.nombre);
        insertarItem.run(pedidoId, producto.id, producto.nombre, producto.precio, cantidad);
      }
      return pedidoId;
    })();
  },

  porId(id) {
    const pedido = db.prepare("SELECT * FROM pedidos WHERE id = ?").get(id);
    if (!pedido) return null;
    pedido.items = db.prepare("SELECT * FROM pedido_items WHERE pedido_id = ?").all(id);
    return pedido;
  },

  listar() {
    return db.prepare("SELECT * FROM pedidos ORDER BY creado_en DESC").all();
  },

  cambiarEstado(id, estado) {
    if (!ESTADOS.includes(estado)) throw new Error(`Estado inválido: ${estado}`);
    db.prepare("UPDATE pedidos SET estado = ? WHERE id = ?").run(estado, id);
  },
};

module.exports = Pedido;
