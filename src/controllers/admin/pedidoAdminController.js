const Pedido = require("../../models/Pedido");

exports.panel = (req, res) => res.redirect("/admin/pedidos");

exports.listar = (req, res) => {
  res.render("admin/pedidos/index", { titulo: "Pedidos", pedidos: Pedido.listar() });
};

exports.detalle = (req, res, next) => {
  const pedido = Pedido.porId(Number(req.params.id));
  if (!pedido) return next();
  res.render("admin/pedidos/detalle", { titulo: `Pedido #${pedido.id}`, pedido, estados: Pedido.ESTADOS });
};

exports.cambiarEstado = (req, res) => {
  Pedido.cambiarEstado(Number(req.params.id), req.body.estado);
  req.flash("exito", "Estado actualizado.");
  res.redirect(`/admin/pedidos/${req.params.id}`);
};
