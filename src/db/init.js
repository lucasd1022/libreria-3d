// Crea la base de datos y sus tablas (lo hace connection.js al importarse).
const db = require("./connection");
const config = require("../config");

const tablas = db
  .prepare("SELECT name FROM sqlite_master WHERE type = 'table' AND name NOT LIKE 'sqlite_%'")
  .all()
  .map((t) => t.name);

console.log(`Base de datos lista en ${config.dbPath}`);
console.log(`Tablas: ${tablas.join(", ")}`);
