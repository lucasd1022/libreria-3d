using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Libreria3D.Models;

namespace Libreria3D.Controllers;

[Authorize(Policy = UsuarioActual.PoliticaAdministrador)]
public class PanelController : Controller
{
    public IActionResult Index() => RedirectToAction(nameof(Pedidos));

    // ---------------------------------------------- pedidos ----------------------------------------------

    public IActionResult Pedidos(string? estado)
    {
        estado = Models.Pedido.Estados.Contains(estado) ? estado : null;
        ViewBag.Estado = estado;
        ViewBag.Pedidos = BD.ObtenerPedidos(estado);
        return View();
    }

    public IActionResult Pedido(int id)
    {
        var pedido = BD.ObtenerPedidoPorId(id);
        if (pedido == null)
        {
            return NotFound();
        }
        ViewBag.Pedido = pedido;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult CambiarEstado(int id, string estado)
    {
        Avisar(Models.Pedidos.CambiarEstado(id, estado));
        return RedirectToAction(nameof(Pedido), new { id });
    }

    // ---------------------------------------------- productos ----------------------------------------------

    public IActionResult Productos(string? texto)
    {
        texto = string.IsNullOrWhiteSpace(texto) ? null : texto.Trim();
        ViewBag.Texto = texto;
        ViewBag.Productos = BD.BuscarProductos(texto, null, soloActivos: false);
        return View();
    }

    [HttpGet]
    public IActionResult NuevoProducto() =>
        FormularioProducto(new ProductoViewModel { Activo = true }, null);

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult NuevoProducto(ProductoViewModel modelo)
    {
        if (!ModelState.IsValid)
        {
            return FormularioProducto(modelo, null);
        }
        BD.CrearProducto(Limpiar(modelo));
        Avisar(Resultado.Bien($"Creaste \"{modelo.Nombre.Trim()}\"."));
        return RedirectToAction(nameof(Productos));
    }

    [HttpGet]
    public IActionResult EditarProducto(int id)
    {
        var producto = BD.ObtenerProductoPorId(id);
        return producto == null ? NotFound() : FormularioProducto(ProductoViewModel.Desde(producto), id);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult EditarProducto(int id, ProductoViewModel modelo)
    {
        if (BD.ObtenerProductoPorId(id) == null)
        {
            return NotFound();
        }
        if (!ModelState.IsValid)
        {
            return FormularioProducto(modelo, id);
        }
        BD.ActualizarProducto(id, Limpiar(modelo));
        Avisar(Resultado.Bien($"Guardaste los cambios de \"{modelo.Nombre.Trim()}\"."));
        return RedirectToAction(nameof(Productos));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult EliminarProducto(int id)
    {
        var producto = BD.ObtenerProductoPorId(id);
        if (producto != null)
        {
            BD.EliminarProducto(id);
            Avisar(Resultado.Bien($"Eliminaste \"{producto.Nombre}\"."));
        }
        return RedirectToAction(nameof(Productos));
    }

    // ---------------------------------------------- cuenta ----------------------------------------------

    [HttpGet]
    public IActionResult Contrasena()
    {
        ViewBag.Formulario = new CambiarContrasenaViewModel();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Contrasena(CambiarContrasenaViewModel modelo)
    {
        var resultado = Cuentas.CambiarContrasena(User.IdUsuario(), modelo, ModelState);
        if (!resultado.Exito)
        {
            ViewBag.Formulario = new CambiarContrasenaViewModel();
            return View();
        }
        Avisar(resultado);
        return RedirectToAction(nameof(Pedidos));
    }

    private IActionResult FormularioProducto(ProductoViewModel modelo, int? id)
    {
        ViewBag.Formulario = modelo;
        ViewBag.IDProducto = id;
        ViewBag.Categorias = BD.ObtenerCategorias();
        return View("ProductoFormulario");
    }

    private static ProductoViewModel Limpiar(ProductoViewModel p)
    {
        p.Nombre = p.Nombre.Trim();
        p.Descripcion = string.IsNullOrWhiteSpace(p.Descripcion) ? null : p.Descripcion.Trim();
        p.Imagen = string.IsNullOrWhiteSpace(p.Imagen) ? null : p.Imagen.Trim();
        return p;
    }

    private void Avisar(Resultado resultado)
    {
        TempData["Mensaje"] = resultado.Mensaje;
        TempData["MensajeTipo"] = resultado.Tipo;
    }
}
