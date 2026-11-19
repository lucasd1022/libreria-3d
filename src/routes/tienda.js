const { Router } = require("express");
const home = require("../controllers/homeController");
const productos = require("../controllers/productoController");
const carrito = require("../controllers/carritoController");
const pedidos = require("../controllers/pedidoController");

const router = Router();

router.get("/", home.inicio);
router.get("/contacto", home.contacto);

router.get("/productos", productos.listar);
router.get("/productos/:slug", productos.detalle);

router.get("/carrito", carrito.ver);
router.post("/carrito", carrito.agregar);
router.put("/carrito/:id", carrito.actualizar);
router.delete("/carrito/:id", carrito.quitar);

router.get("/checkout", pedidos.checkout);
router.post("/pedidos", pedidos.crear);
router.get("/pedidos/:id/confirmacion", pedidos.confirmacion);

module.exports = router;
