using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Libreria3D.Models;

public static class Pedidos
{
    public static (Resultado Resultado, int? IDPedido) Confirmar(ISession sesion, CheckoutViewModel datos)
    {
        var carrito = Carrito.Detalle(sesion);
        if (carrito.EstaVacio)
        {
            return (Resultado.Mal("Tu carrito está vacío."), null);
        }

        var (idPedido, sinStock) = BD.CrearPedido(Normalizar(datos), carrito.Lineas);
        if (idPedido == null)
        {
            return (Resultado.Mal($"Se terminó el stock de \"{sinStock}\". Revisá tu carrito antes de confirmar."), null);
        }

        Carrito.Vaciar(sesion);
        Carrito.RecordarPedido(sesion, idPedido.Value);
        return (Resultado.Bien("¡Recibimos tu pedido!"), idPedido);
    }

    /// <summary>
    /// Revisa la forma de entrega y, si es con envío, que esté la dirección.
    /// Va aparte de los atributos del ViewModel para que se marquen todos los errores juntos.
    /// </summary>
    public static void ValidarEntrega(CheckoutViewModel d, ModelStateDictionary estado)
    {
        if (d.FormaEntrega != Pedido.EntregaEnvio && d.FormaEntrega != Pedido.EntregaRetiro)
        {
            estado.AddModelError(nameof(d.FormaEntrega), "Elegí si querés envío o retiro en el local");
            return;
        }
        if (!d.EsEnvio)
        {
            return;
        }
        if (!Tienda.HaceEnvios)
        {
            estado.AddModelError(nameof(d.FormaEntrega), "Por ahora no hacemos envíos, solo retiro en el local");
        }
        if (string.IsNullOrWhiteSpace(d.Direccion))
        {
            estado.AddModelError(nameof(d.Direccion), "Completá la dirección de envío");
        }
        if (string.IsNullOrWhiteSpace(d.Localidad))
        {
            estado.AddModelError(nameof(d.Localidad), "Completá la localidad");
        }
    }

    public static Resultado CambiarEstado(int idPedido, string estado)
    {
        var pedido = BD.ObtenerPedidoPorId(idPedido);
        if (pedido == null)
        {
            return Resultado.Mal("Ese pedido no existe.");
        }
        if (!Pedido.EstadosPara(pedido.Forma_Entrega).Contains(estado))
        {
            return Resultado.Mal($"Un pedido con {pedido.TextoEntrega.ToLowerInvariant()} no puede quedar como \"{estado}\".");
        }
        BD.CambiarEstadoPedido(idPedido, estado);
        return Resultado.Bien($"El pedido #{idPedido} quedó como \"{estado}\".");
    }

    // Si retira en el local no se guarda la dirección aunque la haya escrito.
    private static CheckoutViewModel Normalizar(CheckoutViewModel d) => new()
    {
        Nombre = d.Nombre.Trim(),
        Email = d.Email.Trim().ToLowerInvariant(),
        Telefono = d.Telefono.Trim(),
        FormaEntrega = d.FormaEntrega,
        Direccion = d.EsEnvio ? Vacio(d.Direccion) : null,
        Localidad = d.EsEnvio ? Vacio(d.Localidad) : null,
        CodigoPostal = d.EsEnvio ? Vacio(d.CodigoPostal) : null,
        Notas = Vacio(d.Notas)
    };

    private static string? Vacio(string? texto) => string.IsNullOrWhiteSpace(texto) ? null : texto.Trim();
}
