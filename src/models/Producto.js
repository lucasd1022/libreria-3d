const db = require("../db/connection");

const SELECT_BASE = `
  SELECT p.*, c.nombre AS categoria_nombre, c.slug AS categoria_slug
  FROM productos p
  LEFT JOIN categorias c ON c.id = p.categoria_id
`;

function slugify(texto) {
  return texto
    .toLowerCase()
    .normalize("NFD")
    .replace(/[̀-ͯ]/g, "")
    .replace(/[^a-z0-9]+/g, "-")
    .replace(/^-|-$/g, "");
}

const Producto = {
  /** Lista productos activos, con filtros opcionales por categoría y texto. */
  listar({ categoria, busqueda, incluirInactivos = false } = {}) {
    const condiciones = [];
    const params = {};
    if (!incluirInactivos) condiciones.push("p.activo = 1");
    if (categoria) {
      condiciones.push("c.slug = @categoria");
      params.categoria = categoria;
    }
    if (busqueda) {
      condiciones.push("(p.nombre LIKE @busqueda OR p.descripcion LIKE @busqueda)");
      params.busqueda = `%${busqueda}%`;
    }
    const where = condiciones.length ? `WHERE ${condiciones.join(" AND ")}` : "";
    return db.prepare(`${SELECT_BASE} ${where} ORDER BY p.nombre`).all(params);
  },

  destacados(limite = 4) {
    return db
      .prepare(`${SELECT_BASE} WHERE p.activo = 1 AND p.stock > 0 ORDER BY p.creado_en DESC LIMIT ?`)
      .all(limite);
  },

  porId(id) {
    return db.prepare(`${SELECT_BASE} WHERE p.id = ?`).get(id);
  },

  porSlug(slug) {
    return db.prepare(`${SELECT_BASE} WHERE p.slug = ? AND p.activo = 1`).get(slug);
  },

  porIds(ids) {
    if (!ids.length) return [];
    const marcadores = ids.map(() => "?").join(",");
    return db.prepare(`${SELECT_BASE} WHERE p.id IN (${marcadores})`).all(...ids);
  },

  crear(datos) {
    const info = db
      .prepare(`
        INSERT INTO productos (slug, nombre, descripcion, precio, stock, imagen, activo, categoria_id)
        VALUES (@slug, @nombre, @descripcion, @precio, @stock, @imagen, @activo, @categoria_id)
      `)
      .run({ ...datos, slug: datos.slug || slugify(datos.nombre) });
    return info.lastInsertRowid;
  },

  actualizar(id, datos) {
    db.prepare(`
      UPDATE productos SET
        nombre = @nombre, descripcion = @descripcion, precio = @precio, stock = @stock,
        imagen = @imagen, activo = @activo, categoria_id = @categoria_id
      WHERE id = @id
    `).run({ ...datos, id });
  },

  eliminar(id) {
    db.prepare("DELETE FROM productos WHERE id = ?").run(id);
  },

  /** Valida y normaliza los datos de un formulario. Devuelve { datos, errores }. */
  validar(body) {
    const errores = [];
    const datos = {
      nombre: String(body.nombre || "").trim(),
      descripcion: String(body.descripcion || "").trim(),
      precio: Number.parseInt(body.precio, 10),
      stock: Number.parseInt(body.stock, 10),
      imagen: String(body.imagen || "").trim() || null,
      activo: body.activo ? 1 : 0,
      categoria_id: body.categoria_id ? Number(body.categoria_id) : null,
    };
    if (!datos.nombre) errores.push("El nombre es obligatorio.");
    if (!Number.isInteger(datos.precio) || datos.precio < 0) errores.push("El precio debe ser un número entero mayor o igual a 0.");
    if (!Number.isInteger(datos.stock) || datos.stock < 0) errores.push("El stock debe ser un número entero mayor o igual a 0.");
    return { datos, errores };
  },
};

module.exports = Producto;
