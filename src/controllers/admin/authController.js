const crypto = require("crypto");
const config = require("../../config");

function igualSeguro(a, b) {
  const ha = crypto.createHash("sha256").update(String(a)).digest();
  const hb = crypto.createHash("sha256").update(String(b)).digest();
  return crypto.timingSafeEqual(ha, hb);
}

exports.formLogin = (req, res) => {
  res.render("admin/login", { titulo: "Ingresar", error: null });
};

exports.login = (req, res, next) => {
  const { usuario, password } = req.body;
  if (igualSeguro(usuario, config.admin.usuario) && igualSeguro(password, config.admin.password)) {
    return req.session.regenerate((err) => {
      if (err) return next(err);
      req.session.admin = true;
      res.redirect("/admin");
    });
  }
  res.status(401).render("admin/login", { titulo: "Ingresar", error: "Usuario o contraseña incorrectos." });
};

exports.logout = (req, res) => {
  req.session.destroy(() => res.redirect("/"));
};
