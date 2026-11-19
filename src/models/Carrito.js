// El carrito vive en la sesión del usuario como { [productoId]: cantidad }.
// Este modelo encapsula su lectura y escritura.
const Producto = require("./Producto");

class Carrito {
  constructor(session) {
    this.session = session;
    if (!session.carrito) session.carrito = {};
  }

  get items() {
    return this.session.carrito;
  }

  agregar(productoId, cantidad = 1) {
    const id = String(productoId);
    this.items[id] = (this.items[id] || 0) + cantidad;
  }

  actualizar(productoId, cantidad) {
    const id = String(productoId);
    if (cantidad > 0) this.items[id] = cantidad;
    else delete this.items[id];
  }

  quitar(productoId) {
    delete this.items[String(productoId)];
  }

  vaciar() {
    this.session.carrito = {};
  }

  cantidadTotal() {
    return Object.values(this.items).reduce((s, n) => s + n, 0);
  }

  /** Devuelve los ítems con los datos actuales de cada producto, más el total. */
  detalle() {
    const ids = Object.keys(this.items).map(Number);
    const productos = Producto.porIds(ids);
    const lineas = productos
      .filter((p) => p.activo)
      .map((producto) => {
        const cantidad = this.items[String(producto.id)];
        return { producto, cantidad, subtotal: producto.precio * cantidad };
      });
    const total = lineas.reduce((s, l) => s + l.subtotal, 0);
    return { lineas, total };
  }
}

module.exports = Carrito;
