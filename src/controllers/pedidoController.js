const Carrito = require("../models/Carrito");
const Pedido = require("../models/Pedido");

const EMAIL_RE = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;

exports.checkout = (req, res) => {
  const carrito = new Carrito(req.session);
  const detalle = carrito.detalle();
  if (!detalle.lineas.length) return res.redirect("/carrito");
  res.render("pedidos/checkout", { titulo: "Finalizar compra", ...detalle, datos: {}, errores: [] });
};

exports.crear = (req, res) => {
  const carrito = new Carrito(req.session);
  const detalle = carrito.detalle();
  if (!detalle.lineas.length) return res.redirect("/carrito");

  const datos = {
    nombre: String(req.body.nombre || "").trim(),
    email: String(req.body.email || "").trim(),
    telefono: String(req.body.telefono || "").trim() || null,
    direccion: String(req.body.direccion || "").trim() || null,
    notas: String(req.body.notas || "").trim() || null,
  };
  const errores = [];
  if (!datos.nombre) errores.push("Ingresá tu nombre.");
  if (!EMAIL_RE.test(datos.email)) errores.push("Ingresá un email válido.");

  if (errores.length) {
    return res.status(422).render("pedidos/checkout", { titulo: "Finalizar compra", ...detalle, datos, errores });
  }

  try {
    const items = detalle.lineas.map(({ producto, cantidad }) => ({ producto, cantidad }));
    const pedidoId = Pedido.crear(datos, items);
    carrito.vaciar();
    req.session.ultimoPedido = pedidoId;
    res.redirect(`/pedidos/${pedidoId}/confirmacion`);
  } catch (err) {
    if (err instanceof Pedido.StockInsuficienteError) {
      req.flash("error", err.message);
      return res.redirect("/carrito");
    }
    throw err;
  }
};

exports.confirmacion = (req, res, next) => {
  const id = Number(req.params.id);
  // Solo quien hizo el pedido puede ver su confirmación.
  if (req.session.ultimoPedido !== id) return next();
  const pedido = Pedido.porId(id);
  if (!pedido) return next();
  res.render("pedidos/confirmacion", { titulo: "¡Gracias por tu compra!", pedido });
};
