// Carga categorías y productos de ejemplo. Se puede correr varias veces sin duplicar datos.
const db = require("./connection");

const categorias = [
  { slug: "cuadernos", nombre: "Cuadernos" },
  { slug: "escritura", nombre: "Escritura" },
  { slug: "arte", nombre: "Arte y dibujo" },
  { slug: "organizacion", nombre: "Organización" },
  { slug: "oficina", nombre: "Oficina" },
];

const productos = [
  ["cuaderno-a4-rayado", "Cuaderno A4 rayado 80 hojas", "cuadernos", 4500, 25, "Tapa dura, espiral metálico, hojas de 80 g."],
  ["cuaderno-a5-punteado", "Cuaderno A5 punteado", "cuadernos", 3900, 18, "Ideal para bullet journal. 96 hojas de 100 g."],
  ["boligrafos-azules-x10", "Bolígrafos azules x10", "escritura", 2800, 40, "Trazo medio 1.0 mm, tinta de secado rápido."],
  ["resaltadores-pastel-x6", "Resaltadores pastel x6", "escritura", 3300, 30, "Punta biselada, tonos pastel."],
  ["fibras-x24", "Fibras de colores x24", "arte", 6200, 12, "Punta fina, lavables, colores intensos."],
  ["lapices-color-x36", "Lápices de colores x36", "arte", 7400, 15, "Mina blanda de 3.3 mm, estuche metálico."],
  ["carpeta-a4-anillos", "Carpeta A4 con anillos", "organizacion", 3600, 20, "Tres anillos, tapa plástica resistente."],
  ["abrochadora-metalica", "Abrochadora metálica", "oficina", 5900, 10, "Para broches 21/6, hasta 20 hojas."],
];

const insertarCategoria = db.prepare(
  "INSERT INTO categorias (slug, nombre) VALUES (@slug, @nombre) ON CONFLICT(slug) DO NOTHING"
);
const idCategoria = db.prepare("SELECT id FROM categorias WHERE slug = ?");
const insertarProducto = db.prepare(`
  INSERT INTO productos (slug, nombre, categoria_id, precio, stock, descripcion)
  VALUES (?, ?, ?, ?, ?, ?)
  ON CONFLICT(slug) DO NOTHING
`);

db.transaction(() => {
  categorias.forEach((c) => insertarCategoria.run(c));
  productos.forEach(([slug, nombre, cat, precio, stock, desc]) => {
    insertarProducto.run(slug, nombre, idCategoria.get(cat).id, precio, stock, desc);
  });
})();

console.log(`Seed listo: ${categorias.length} categorías, ${productos.length} productos.`);
