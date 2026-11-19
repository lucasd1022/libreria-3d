// Variables disponibles en todas las vistas.
const config = require("../config");
const Carrito = require("../models/Carrito");

const formatoPrecio = new Intl.NumberFormat(config.tienda.locale, {
  style: "currency",
  currency: config.tienda.moneda,
  maximumFractionDigits: 0,
});

module.exports = (req, res, next) => {
  res.locals.tienda = config.tienda;
  res.locals.rutaActual = req.path;
  res.locals.esAdmin = Boolean(req.session.admin);
  res.locals.cantidadCarrito = new Carrito(req.session).cantidadTotal();
  res.locals.precio = (valor) => formatoPrecio.format(valor);
  next();
};
