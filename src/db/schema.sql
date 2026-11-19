CREATE TABLE IF NOT EXISTS categorias (
  id      INTEGER PRIMARY KEY AUTOINCREMENT,
  slug    TEXT NOT NULL UNIQUE,
  nombre  TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS productos (
  id           INTEGER PRIMARY KEY AUTOINCREMENT,
  slug         TEXT NOT NULL UNIQUE,
  nombre       TEXT NOT NULL,
  descripcion  TEXT NOT NULL DEFAULT '',
  precio       INTEGER NOT NULL CHECK (precio >= 0),        -- en pesos, sin decimales
  stock        INTEGER NOT NULL DEFAULT 0 CHECK (stock >= 0),
  imagen       TEXT,
  activo       INTEGER NOT NULL DEFAULT 1,
  categoria_id INTEGER REFERENCES categorias(id) ON DELETE SET NULL,
  creado_en    TEXT NOT NULL DEFAULT (datetime('now'))
);

CREATE TABLE IF NOT EXISTS pedidos (
  id         INTEGER PRIMARY KEY AUTOINCREMENT,
  nombre     TEXT NOT NULL,
  email      TEXT NOT NULL,
  telefono   TEXT,
  direccion  TEXT,
  notas      TEXT,
  total      INTEGER NOT NULL,
  estado     TEXT NOT NULL DEFAULT 'pendiente'
             CHECK (estado IN ('pendiente','pagado','enviado','entregado','cancelado')),
  creado_en  TEXT NOT NULL DEFAULT (datetime('now'))
);

CREATE TABLE IF NOT EXISTS pedido_items (
  id              INTEGER PRIMARY KEY AUTOINCREMENT,
  pedido_id       INTEGER NOT NULL REFERENCES pedidos(id) ON DELETE CASCADE,
  producto_id     INTEGER REFERENCES productos(id) ON DELETE SET NULL,
  nombre          TEXT NOT NULL,     -- copia del nombre al momento de la compra
  precio_unitario INTEGER NOT NULL,
  cantidad        INTEGER NOT NULL CHECK (cantidad > 0)
);

CREATE INDEX IF NOT EXISTS idx_productos_categoria ON productos(categoria_id);
CREATE INDEX IF NOT EXISTS idx_items_pedido ON pedido_items(pedido_id);
