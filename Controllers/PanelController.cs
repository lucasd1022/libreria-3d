using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Libreria3D.Models;

namespace Libreria3D.Controllers;

[Authorize(Policy = UsuarioActual.PoliticaAdministrador)]
public class PanelController : Controller
{
    private readonly IWebHostEnvironment _entorno;

    public PanelController(IWebHostEnvironment entorno)
    {
        _entorno = entorno;
    }

    public IActionResult Index() => RedirectToAction(nameof(Pedidos));

    // ---------------------------------------------- pedidos ----------------------------------------------

    public IActionResult Pedidos(string? estado, string? entrega)
    {
        estado = Models.Pedido.Estados.Contains(estado) ? estado : null;
        entrega = entrega is Models.Pedido.EntregaEnvio or Models.Pedido.EntregaRetiro ? entrega : null;
        ViewBag.Estado = estado;
        ViewBag.Entrega = entrega;
        ViewBag.Pedidos = BD.ObtenerPedidos(estado, entrega);
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
    public async Task<IActionResult> NuevoProducto(ProductoViewModel modelo, IFormFile? foto)
    {
        Limpiar(modelo);
        ValidarProducto(modelo, foto, null);
        if (!ModelState.IsValid)
        {
            return FormularioProducto(modelo, null);
        }
        if (foto is { Length: > 0 })
        {
            modelo.Imagen = await Archivos.GuardarFoto(foto, _entorno.WebRootPath);
        }
        BD.CrearProducto(modelo);
        Avisar(Resultado.Bien($"Creaste \"{modelo.Nombre}\"."));
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
    public async Task<IActionResult> EditarProducto(int id, ProductoViewModel modelo, IFormFile? foto, bool quitarFoto)
    {
        var actual = BD.ObtenerProductoPorId(id);
        if (actual == null)
        {
            return NotFound();
        }
        Limpiar(modelo);
        modelo.Imagen = actual.Imagen;
        ValidarProducto(modelo, foto, id);
        if (!ModelState.IsValid)
        {
            return FormularioProducto(modelo, id);
        }

        if (foto is { Length: > 0 })
        {
            modelo.Imagen = await Archivos.GuardarFoto(foto, _entorno.WebRootPath);
        }
        else if (quitarFoto)
        {
            modelo.Imagen = null;
        }
        BD.ActualizarProducto(id, modelo);
        if (modelo.Imagen != actual.Imagen)
        {
            Archivos.BorrarFoto(actual.Imagen, _entorno.WebRootPath);
        }
        Avisar(Resultado.Bien($"Guardaste los cambios de \"{modelo.Nombre}\"."));
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
            Archivos.BorrarFoto(producto.Imagen, _entorno.WebRootPath);
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

    private static void Limpiar(ProductoViewModel p)
    {
        p.Codigo = string.IsNullOrWhiteSpace(p.Codigo) ? null : p.Codigo.Trim();
        p.Nombre = p.Nombre.Trim();
        p.Descripcion = string.IsNullOrWhiteSpace(p.Descripcion) ? null : p.Descripcion.Trim();
    }

    private void ValidarProducto(ProductoViewModel p, IFormFile? foto, int? id)
    {
        if (p.Codigo != null && BD.ExisteCodigoProducto(p.Codigo, id))
        {
            ModelState.AddModelError(nameof(p.Codigo), "Ya hay otro producto con ese código");
        }
        if (Archivos.ValidarFoto(foto) is string error)
        {
            ModelState.AddModelError("Foto", error);
        }
    }

    private void Avisar(Resultado resultado)
    {
        TempData["Mensaje"] = resultado.Mensaje;
        TempData["MensajeTipo"] = resultado.Tipo;
    }
}
