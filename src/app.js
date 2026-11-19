const path = require("path");
const express = require("express");
const session = require("express-session");
const methodOverride = require("method-override");
const config = require("./config");
const locals = require("./middlewares/locals");
const flash = require("./middlewares/flash");
const { noEncontrado, errorServidor } = require("./middlewares/errorHandler");

const app = express();

// Vistas (la "V" de MVC)
app.set("view engine", "ejs");
app.set("views", path.join(__dirname, "views"));

// Middlewares generales
if (config.entorno === "production") app.set("trust proxy", 1); // cookies seguras detrás de un proxy HTTPS
app.use(express.static(path.join(config.raiz, "public")));
app.use(express.urlencoded({ extended: false }));
app.use(methodOverride("_method")); // permite PUT/DELETE desde formularios HTML
app.use(
  session({
    secret: config.sessionSecret,
    resave: false,
    saveUninitialized: false,
    cookie: { httpOnly: true, sameSite: "lax", secure: config.entorno === "production" },
  })
);
app.use(flash);
app.use(locals);

// Rutas → controladores
app.use("/", require("./routes/tienda"));
app.use("/admin", require("./routes/admin"));

// Errores
app.use(noEncontrado);
app.use(errorServidor);

module.exports = app;
