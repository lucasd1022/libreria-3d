const { Router } = require("express");
const requireAdmin = require("../middlewares/requireAdmin");
const auth = require("../controllers/admin/authController");
const productos = require("../controllers/admin/productoAdminController");
const pedidos = require("../controllers/admin/pedidoAdminController");

const router = Router();

router.get("/login", auth.formLogin);
router.post("/login", auth.login);
router.post("/logout", auth.logout);

// Todo lo que sigue requiere estar logueado como administrador.
router.use(requireAdmin);

router.get("/", pedidos.panel);

router.get("/productos", productos.listar);
router.get("/productos/nuevo", productos.nuevo);
router.post("/productos", productos.crear);
router.get("/productos/:id/editar", productos.editar);
router.put("/productos/:id", productos.actualizar);
router.delete("/productos/:id", productos.eliminar);

router.get("/pedidos", pedidos.listar);
router.get("/pedidos/:id", pedidos.detalle);
router.put("/pedidos/:id/estado", pedidos.cambiarEstado);

module.exports = router;
