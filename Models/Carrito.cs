using System.Text.Json;

namespace Libreria3D.Models;

// El carrito vive en la sesión como { IDProducto: cantidad }.
public static class Carrito
{
    private const string Clave = "Carrito";
    private const string ClaveUltimoPedido = "UltimoPedido";

    private static Dictionary<int, int> Leer(ISession sesion) =>
        sesion.GetString(Clave) is string json
            ? JsonSerializer.Deserialize<Dictionary<int, int>>(json) ?? new()
            : new();

    private static void Guardar(ISession sesion, Dictionary<int, int> items) =>
        sesion.SetString(Clave, JsonSerializer.Serialize(items));

    public static int CantidadTotal(ISession sesion) => Leer(sesion).Values.Sum();

    public static Resultado Agregar(ISession sesion, int idProducto, int cantidad)
    {
        var producto = BD.ObtenerProductoPorId(idProducto);
        if (producto == null || !producto.Activo)
        {
            return Resultado.Mal("Ese producto ya no está disponible.");
        }

        cantidad = Math.Max(1, cantidad);
        var items = Leer(sesion);
        var enCarrito = items.GetValueOrDefault(idProducto);
        if (enCarrito + cantidad > producto.Stock)
        {
            return Resultado.Mal(producto.Stock == 0
                ? $"No queda stock de \"{producto.Nombre}\"."
                : $"Solo quedan {producto.Stock} unidades de \"{producto.Nombre}\".");
        }

        items[idProducto] = enCarrito + cantidad;
        Guardar(sesion, items);
        return Resultado.Bien($"Agregaste \"{producto.Nombre}\" al carrito.");
    }

    public static Resultado Actualizar(ISession sesion, int idProducto, int cantidad)
    {
        var items = Leer(sesion);
        if (cantidad <= 0)
        {
            items.Remove(idProducto);
            Guardar(sesion, items);
            return Resultado.Bien("Quitaste el producto del carrito.");
        }

        var producto = BD.ObtenerProductoPorId(idProducto);
        if (producto == null || !producto.Activo)
        {
            items.Remove(idProducto);
            Guardar(sesion, items);
            return Resultado.Mal("Ese producto ya no está disponible y lo sacamos del carrito.");
        }
        if (cantidad > producto.Stock)
        {
            return Resultado.Mal($"Solo quedan {producto.Stock} unidades de \"{producto.Nombre}\".");
        }

        items[idProducto] = cantidad;
        Guardar(sesion, items);
        return Resultado.Bien("Actualizamos la cantidad.");
    }

    public static void Quitar(ISession sesion, int idProducto)
    {
        var items = Leer(sesion);
        items.Remove(idProducto);
        Guardar(sesion, items);
    }

    public static void Vaciar(ISession sesion) => sesion.Remove(Clave);

    /// <summary>Los productos del carrito con sus datos actuales de la base.</summary>
    public static DetalleCarrito Detalle(ISession sesion)
    {
        var items = Leer(sesion);
        var lineas = BD.ObtenerProductosPorIds(items.Keys)
            .Where(p => p.Activo)
            .Select(p => new LineaCarrito { Producto = p, Cantidad = items[p.ID] })
            .OrderBy(l => l.Producto.Nombre)
            .ToList();
        return new DetalleCarrito { Lineas = lineas };
    }

    // Solo quien hizo el pedido puede ver su página de confirmación.
    public static void RecordarPedido(ISession sesion, int idPedido) => sesion.SetInt32(ClaveUltimoPedido, idPedido);

    public static bool EsUltimoPedido(ISession sesion, int idPedido) => sesion.GetInt32(ClaveUltimoPedido) == idPedido;
}

public class LineaCarrito
{
    public Producto Producto { get; set; } = new();
    public int Cantidad { get; set; }

    public decimal Subtotal => Producto.Precio * Cantidad;
}

public class DetalleCarrito
{
    public List<LineaCarrito> Lineas { get; set; } = new();

    public decimal Total => Lineas.Sum(l => l.Subtotal);
    public bool EstaVacio => Lineas.Count == 0;
}
