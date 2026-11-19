const Producto = require("../models/Producto");
const Categoria = require("../models/Categoria");

exports.inicio = (req, res) => {
  res.render("home", {
    titulo: "Inicio",
    destacados: Producto.destacados(4),
    categorias: Categoria.todas(),
  });
};

exports.contacto = (req, res) => {
  res.render("contacto", { titulo: "Contacto" });
};
