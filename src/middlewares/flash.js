// Mensajes de un solo uso guardados en la sesión (ej: "Producto agregado").
module.exports = (req, res, next) => {
  req.flash = (tipo, mensaje) => {
    req.session.flash = req.session.flash || [];
    req.session.flash.push({ tipo, mensaje });
  };
  res.locals.mensajes = req.session.flash || [];
  delete req.session.flash;
  next();
};
