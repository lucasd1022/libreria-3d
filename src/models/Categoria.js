const db = require("../db/connection");

const Categoria = {
  todas() {
    return db.prepare("SELECT * FROM categorias ORDER BY nombre").all();
  },

  porSlug(slug) {
    return db.prepare("SELECT * FROM categorias WHERE slug = ?").get(slug);
  },
};

module.exports = Categoria;
