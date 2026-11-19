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

    public static Resultado CambiarEstado(int idPedido, string estado)
    {
        if (!Pedido.Estados.Contains(estado))
        {
            return Resultado.Mal("Elegí un estado válido.");
        }
        if (BD.ObtenerPedidoPorId(idPedido) == null)
        {
            return Resultado.Mal("Ese pedido no existe.");
        }
        BD.CambiarEstadoPedido(idPedido, estado);
        return Resultado.Bien($"El pedido #{idPedido} quedó como \"{estado}\".");
    }

    private static CheckoutViewModel Normalizar(CheckoutViewModel d) => new()
    {
        Nombre = d.Nombre.Trim(),
        Email = d.Email.Trim().ToLowerInvariant(),
        Telefono = Vacio(d.Telefono),
        Direccion = Vacio(d.Direccion),
        Notas = Vacio(d.Notas)
    };

    private static string? Vacio(string? texto) => string.IsNullOrWhiteSpace(texto) ? null : texto.Trim();
}
