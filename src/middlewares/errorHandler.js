const config = require("../config");

exports.noEncontrado = (req, res) => {
  res.status(404).render("errors/404", { titulo: "Página no encontrada" });
};

// eslint-disable-next-line no-unused-vars
exports.errorServidor = (err, req, res, next) => {
  console.error(err);
  res.status(500).render("errors/500", {
    titulo: "Error",
    detalle: config.entorno === "development" ? err.stack : null,
  });
};
