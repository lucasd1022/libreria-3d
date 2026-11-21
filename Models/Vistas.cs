using Microsoft.AspNetCore.Mvc.Rendering;

namespace Libreria3D.Models;

public static class Vistas
{
    public static readonly System.Globalization.CultureInfo Cultura = new("es-AR");

    public static string Precio(decimal valor) => valor.ToString("C0", Cultura);

    /// <summary>Color de la etiqueta de cada estado de pedido.</summary>
    public static string ClaseEstado(string estado) => estado switch
    {
        Pedido.Pendiente => "etiqueta-amarilla",
        Pedido.Pagado => "etiqueta-azul",
        Pedido.ListoParaRetirar => "etiqueta-turquesa",
        Pedido.Enviado => "etiqueta-violeta",
        Pedido.Entregado => "etiqueta-verde",
        Pedido.Cancelado => "etiqueta-roja",
        _ => ""
    };

    public static string Fecha(DateTime fecha) => fecha.ToString("dd/MM/yyyy HH:mm", Cultura);

    public static string? Actual(this ViewContext vista, string controlador) =>
        string.Equals(vista.RouteData.Values["controller"]?.ToString(), controlador, StringComparison.OrdinalIgnoreCase) ? "page" : null;

    public static string? ActualAccion(this ViewContext vista, string controlador, string accion) =>
        vista.Actual(controlador) != null && string.Equals(vista.RouteData.Values["action"]?.ToString(), accion, StringComparison.OrdinalIgnoreCase) ? "page" : null;

    public static string ClaseLink(this ViewContext vista, string controlador, string? accion = null) =>
        (accion == null ? vista.Actual(controlador) : vista.ActualAccion(controlador, accion)) != null ? "menu-link activo" : "menu-link";

    public static string AquiMismo(this ViewContext vista) =>
        vista.HttpContext.Request.Path + vista.HttpContext.Request.QueryString;

    public static int CantidadCarrito(this ViewContext vista) =>
        Carrito.CantidadTotal(vista.HttpContext.Session);

    public static int PedidosPendientes(this ViewContext vista)
    {
        var items = vista.HttpContext.Items;
        if (items["Pendientes"] is not int pendientes)
        {
            pendientes = vista.HttpContext.User.EsAdministrador() ? BD.ContarPedidosPendientes() : 0;
            items["Pendientes"] = pendientes;
        }
        return pendientes;
    }
}
