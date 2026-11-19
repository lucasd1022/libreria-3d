using Microsoft.AspNetCore.Mvc;
using Libreria3D.Models;

namespace Libreria3D.Controllers;

public class CarritoController : Controller
{
    public IActionResult Index()
    {
        ViewBag.Carrito = Carrito.Detalle(HttpContext.Session);
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Agregar(int id, int cantidad = 1, string? volver = null) =>
        Volver(Carrito.Agregar(HttpContext.Session, id, cantidad), volver);

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Actualizar(int id, int cantidad) =>
        Volver(Carrito.Actualizar(HttpContext.Session, id, cantidad), null);

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Quitar(int id)
    {
        Carrito.Quitar(HttpContext.Session, id);
        return Volver(Resultado.Bien("Quitaste el producto del carrito."), null);
    }

    private IActionResult Volver(Resultado resultado, string? volver)
    {
        TempData["Mensaje"] = resultado.Mensaje;
        TempData["MensajeTipo"] = resultado.Tipo;
        return !string.IsNullOrEmpty(volver) && Url.IsLocalUrl(volver) ? LocalRedirect(volver) : RedirectToAction(nameof(Index));
    }
}
