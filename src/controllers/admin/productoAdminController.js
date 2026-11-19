const Producto = require("../../models/Producto");
const Categoria = require("../../models/Categoria");

exports.listar = (req, res) => {
  res.render("admin/productos/index", {
    titulo: "Productos",
    productos: Producto.listar({ incluirInactivos: true }),
  });
};

exports.nuevo = (req, res) => {
  res.render("admin/productos/form", {
    titulo: "Nuevo producto",
    producto: { activo: 1 },
    categorias: Categoria.todas(),
    errores: [],
  });
};

exports.crear = (req, res) => {
  const { datos, errores } = Producto.validar(req.body);
  if (errores.length) {
    return res.status(422).render("admin/productos/form", {
      titulo: "Nuevo producto", producto: datos, categorias: Categoria.todas(), errores,
    });
  }
  Producto.crear(datos);
  req.flash("exito", "Producto creado.");
  res.redirect("/admin/productos");
};

exports.editar = (req, res, next) => {
  const producto = Producto.porId(Number(req.params.id));
  if (!producto) return next();
  res.render("admin/productos/form", {
    titulo: `Editar: ${producto.nombre}`, producto, categorias: Categoria.todas(), errores: [],
  });
};

exports.actualizar = (req, res, next) => {
  const id = Number(req.params.id);
  if (!Producto.porId(id)) return next();
  const { datos, errores } = Producto.validar(req.body);
  if (errores.length) {
    return res.status(422).render("admin/productos/form", {
      titulo: "Editar producto", producto: { ...datos, id }, categorias: Categoria.todas(), errores,
    });
  }
  Producto.actualizar(id, datos);
  req.flash("exito", "Producto actualizado.");
  res.redirect("/admin/productos");
};

exports.eliminar = (req, res) => {
  Producto.eliminar(Number(req.params.id));
  req.flash("exito", "Producto eliminado.");
  res.redirect("/admin/productos");
};
