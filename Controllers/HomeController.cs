using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Libreria3D.Models;

namespace Libreria3D.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        ViewBag.Categorias = BD.ObtenerCategorias();
        ViewBag.Nuevos = BD.ObtenerProductosNuevos(8);
        return View();
    }

    public IActionResult Contacto()
    {
        return View();
    }

    // Página para los errores 404, 400, etc. (ver UseStatusCodePagesWithReExecute en Program.cs).
    public IActionResult Estado(int codigo)
    {
        Response.StatusCode = codigo >= 400 && codigo < 600 ? codigo : 404;
        return Response.StatusCode == 404 ? View("NoEncontrado") : View("~/Views/Shared/Error.cshtml");
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        ViewBag.CodigoSolicitud = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        return View();
    }
}
