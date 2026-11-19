const fs = require("fs");
const path = require("path");
const Database = require("better-sqlite3");
const config = require("../config");

fs.mkdirSync(path.dirname(config.dbPath), { recursive: true });

const db = new Database(config.dbPath);
db.pragma("journal_mode = WAL");
db.pragma("foreign_keys = ON");

// Crea las tablas si no existen, así la app arranca aunque no se haya corrido db:init.
db.exec(fs.readFileSync(path.join(__dirname, "schema.sql"), "utf8"));

module.exports = db;
