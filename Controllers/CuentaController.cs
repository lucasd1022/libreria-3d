using Microsoft.AspNetCore.Mvc;
using Libreria3D.Models;

namespace Libreria3D.Controllers;

public class CuentaController : Controller
{
    [HttpGet]
    public IActionResult IniciarSesion(string? returnUrl = null)
    {
        if (User.EsAdministrador())
        {
            return RedirigirDespuesDeIngresar(returnUrl);
        }
        return Formulario(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> IniciarSesion(LoginViewModel modelo)
    {
        var usuario = Cuentas.IniciarSesion(modelo, ModelState);
        if (usuario == null)
        {
            modelo.Contrasena = "";
            return Formulario(modelo);
        }

        await Sesion.Iniciar(HttpContext, usuario);
        TempData["Mensaje"] = $"¡Hola, {usuario.Nombre}!";
        return RedirigirDespuesDeIngresar(modelo.ReturnUrl);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CerrarSesion()
    {
        await Sesion.Cerrar(HttpContext);
        return RedirectToAction("Index", "Home");
    }

    private IActionResult Formulario(LoginViewModel modelo)
    {
        ViewBag.Formulario = modelo;
        return View();
    }

    private IActionResult RedirigirDespuesDeIngresar(string? returnUrl) =>
        !string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl) ? LocalRedirect(returnUrl) : RedirectToAction("Index", "Panel");
}
