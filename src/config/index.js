const path = require("path");
require("dotenv").config();

const raiz = path.resolve(__dirname, "..", "..");

module.exports = {
  raiz,
  port: Number(process.env.PORT) || 3000,
  entorno: process.env.NODE_ENV || "development",
  dbPath: path.resolve(raiz, process.env.DB_PATH || "data/libreria.sqlite"),
  sessionSecret: process.env.SESSION_SECRET || "dev-secret-cambiar",
  admin: {
    usuario: process.env.ADMIN_USER || "admin",
    password: process.env.ADMIN_PASSWORD || "admin",
  },
  tienda: {
    nombre: process.env.TIENDA_NOMBRE || "Librería 3D",
    email: process.env.TIENDA_EMAIL || "contacto@ejemplo.com",
    whatsapp: process.env.TIENDA_WHATSAPP || "",
    moneda: "ARS",
    locale: "es-AR",
  },
};
