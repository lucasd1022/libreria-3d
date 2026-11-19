const Producto = require("../models/Producto");
const Categoria = require("../models/Categoria");

exports.listar = (req, res) => {
  const { categoria, q } = req.query;
  res.render("productos/index", {
    titulo: "Catálogo",
    productos: Producto.listar({ categoria, busqueda: q }),
    categorias: Categoria.todas(),
    filtro: { categoria: categoria || "", q: q || "" },
  });
};

exports.detalle = (req, res, next) => {
  const producto = Producto.porSlug(req.params.slug);
  if (!producto) return next(); // cae en el 404
  res.render("productos/detalle", { titulo: producto.nombre, producto });
};
