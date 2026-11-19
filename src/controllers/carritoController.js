const Carrito = require("../models/Carrito");
const Producto = require("../models/Producto");

exports.ver = (req, res) => {
  const carrito = new Carrito(req.session);
  res.render("carrito/index", { titulo: "Carrito", ...carrito.detalle() });
};

exports.agregar = (req, res) => {
  const producto = Producto.porId(Number(req.body.productoId));
  const cantidad = Math.max(1, Number.parseInt(req.body.cantidad, 10) || 1);
  if (!producto || !producto.activo) {
    req.flash("error", "El producto no existe.");
  } else {
    const carrito = new Carrito(req.session);
    const enCarrito = carrito.items[String(producto.id)] || 0;
    if (enCarrito + cantidad > producto.stock) {
      req.flash("error", `Solo quedan ${producto.stock} unidades de "${producto.nombre}".`);
    } else {
      carrito.agregar(producto.id, cantidad);
      req.flash("exito", `"${producto.nombre}" se agregó al carrito.`);
    }
  }
  res.redirect(req.get("Referrer") || "/carrito");
};

exports.actualizar = (req, res) => {
  const carrito = new Carrito(req.session);
  const producto = Producto.porId(Number(req.params.id));
  const cantidad = Number.parseInt(req.body.cantidad, 10) || 0;
  if (producto && cantidad > producto.stock) {
    req.flash("error", `Solo quedan ${producto.stock} unidades de "${producto.nombre}".`);
  } else {
    carrito.actualizar(req.params.id, cantidad);
  }
  res.redirect("/carrito");
};

exports.quitar = (req, res) => {
  new Carrito(req.session).quitar(req.params.id);
  res.redirect("/carrito");
};
