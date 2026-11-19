using System.Security.Claims;

namespace Libreria3D.Models;

public static class UsuarioActual
{
    public const string ClaimAdministrador = "libreria3d:administrador";
    public const string PoliticaAdministrador = "Administrador";

    public static int IdUsuario(this ClaimsPrincipal usuario) =>
        int.TryParse(usuario.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : 0;

    public static string Nombre(this ClaimsPrincipal usuario) =>
        usuario.FindFirstValue(ClaimTypes.GivenName) ?? "";

    public static bool EstaLogueado(this ClaimsPrincipal usuario) =>
        usuario.Identity?.IsAuthenticated == true;

    public static bool EsAdministrador(this ClaimsPrincipal usuario) =>
        usuario.FindFirstValue(ClaimAdministrador) == "true";
}
