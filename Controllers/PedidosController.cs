using Microsoft.AspNetCore.Mvc;
using Libreria3D.Models;

namespace Libreria3D.Controllers;

public class PedidosController : Controller
{
    [HttpGet]
    public IActionResult Checkout()
    {
        var carrito = Carrito.Detalle(HttpContext.Session);
        if (carrito.EstaVacio)
        {
            return RedirectToAction("Index", "Carrito");
        }
        return Formulario(new CheckoutViewModel(), carrito);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Checkout(CheckoutViewModel modelo)
    {
        var carrito = Carrito.Detalle(HttpContext.Session);
        if (carrito.EstaVacio)
        {
            return RedirectToAction("Index", "Carrito");
        }
        Pedidos.ValidarEntrega(modelo, ModelState);
        if (!ModelState.IsValid)
        {
            return Formulario(modelo, carrito);
        }

        var (resultado, idPedido) = Pedidos.Confirmar(HttpContext.Session, modelo);
        if (idPedido == null)
        {
            TempData["Mensaje"] = resultado.Mensaje;
            TempData["MensajeTipo"] = resultado.Tipo;
            return RedirectToAction("Index", "Carrito");
        }
        return RedirectToAction(nameof(Confirmacion), new { id = idPedido });
    }

    public IActionResult Confirmacion(int id)
    {
        var pedido = Carrito.EsUltimoPedido(HttpContext.Session, id) ? BD.ObtenerPedidoPorId(id) : null;
        if (pedido == null)
        {
            return NotFound();
        }
        ViewBag.Pedido = pedido;
        return View();
    }

    private IActionResult Formulario(CheckoutViewModel modelo, DetalleCarrito carrito)
    {
        ViewBag.Formulario = modelo;
        ViewBag.Carrito = carrito;
        return View("Checkout");
    }
}
