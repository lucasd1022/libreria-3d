# Librería 3D

Tienda online de artículos de librería: catálogo, carrito, checkout y panel de administración.

**Stack:** Node.js + Express · vistas EJS renderizadas en el servidor · SQLite (`better-sqlite3`) · arquitectura **MVC**.

## Puesta en marcha

Requisitos: Node.js 20 o superior.

```bash
npm install
cp .env.example .env      # editá las credenciales de admin y SESSION_SECRET
npm run db:seed           # crea la base y carga productos de ejemplo
npm run dev               # http://localhost:3000 (se reinicia al guardar cambios)
```

En producción: `npm start` con `NODE_ENV=production`.

## Estructura (MVC)

```
├── public/                    Archivos estáticos (servidos tal cual)
│   ├── css/styles.css
│   ├── js/main.js
│   └── img/productos/         Fotos de productos
└── src/
    ├── server.js              Punto de entrada: levanta el servidor
    ├── app.js                 Configura Express, middlewares y rutas
    ├── config/                Lee variables de entorno (.env)
    ├── db/
    │   ├── connection.js      Conexión SQLite (crea las tablas si faltan)
    │   ├── schema.sql         Tablas: categorias, productos, pedidos, pedido_items
    │   ├── init.js            npm run db:init
    │   └── seed.js            npm run db:seed
    ├── models/          (M)   Acceso a datos y reglas de negocio
    │   ├── Producto.js
    │   ├── Categoria.js
    │   ├── Pedido.js          Crea pedidos y descuenta stock en una transacción
    │   └── Carrito.js         Carrito guardado en la sesión
    ├── views/           (V)   Plantillas EJS
    │   ├── partials/          head, header, footer, tarjeta de producto…
    │   ├── productos/  carrito/  pedidos/
    │   ├── admin/             login, ABM de productos, gestión de pedidos
    │   └── errors/            404 y 500
    ├── controllers/     (C)   Reciben el request, usan modelos y renderizan vistas
    │   └── admin/
    ├── routes/                Mapean URLs → controladores
    │   ├── tienda.js
    │   └── admin.js
    └── middlewares/           Sesión de admin, mensajes flash, variables globales, errores
```

El flujo de un pedido es: **ruta** → **controlador** → **modelo** (consulta/escribe en SQLite) → **controlador** → **vista**.

## Rutas

| Tienda | |
|---|---|
| `GET /` | Inicio con categorías y novedades |
| `GET /productos?categoria=&q=` | Catálogo con filtro y búsqueda |
| `GET /productos/:slug` | Detalle de producto |
| `GET /carrito` · `POST /carrito` · `PUT/DELETE /carrito/:id` | Carrito |
| `GET /checkout` · `POST /pedidos` | Finalizar compra |
| `GET /contacto` | Contacto |

| Admin (requiere login en `/admin/login`) | |
|---|---|
| `/admin/productos` | Alta, edición, baja y stock de productos |
| `/admin/pedidos` | Listado de pedidos y cambio de estado |

## Próximos pasos sugeridos

- Integrar un medio de pago (por ejemplo Mercado Pago).
- Subida de imágenes desde el panel (hoy se carga una URL).
- Envío de email de confirmación al cliente.
- Guardar las sesiones en la base (hoy están en memoria y se pierden al reiniciar).
- Tests automáticos de modelos y rutas.
