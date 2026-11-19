using Microsoft.AspNetCore.Mvc;
using Libreria3D.Models;

namespace Libreria3D.Controllers;

public class ProductosController : Controller
{
    public IActionResult Index(string? texto, int? categoria)
    {
        texto = string.IsNullOrWhiteSpace(texto) ? null : texto.Trim();
        ViewBag.Texto = texto;
        ViewBag.Categoria = categoria;
        ViewBag.Categorias = BD.ObtenerCategorias();
        ViewBag.Productos = BD.BuscarProductos(texto, categoria);
        return View();
    }

    public IActionResult Detalle(int id)
    {
        var producto = BD.ObtenerProductoPorId(id);
        if (producto == null || !producto.Activo)
        {
            return NotFound();
        }
        ViewBag.Producto = producto;
        return View();
    }
}
