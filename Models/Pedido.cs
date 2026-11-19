namespace Libreria3D.Models;

public class Pedido
{
    public const string Pendiente = "Pendiente";
    public static readonly string[] Estados = { Pendiente, "Pagado", "Enviado", "Entregado", "Cancelado" };

    public int ID { get; set; }
    public string Nombre { get; set; } = "";
    public string Email { get; set; } = "";
    public string? Telefono { get; set; }
    public string? Direccion { get; set; }
    public string? Notas { get; set; }
    public decimal Total { get; set; }
    public string Estado { get; set; } = Pendiente;
    public DateTime Fecha { get; set; }
    public int CantidadProductos { get; set; }

    public List<PedidoItem> Items { get; set; } = new();

    public string ClaseEstado => $"estado-{Estado.ToLowerInvariant()}";
}

public class PedidoItem
{
    public int ID { get; set; }
    public int IDPedido { get; set; }
    public int? IDProducto { get; set; }
    public string Nombre { get; set; } = "";
    public decimal Precio_Unitario { get; set; }
    public int Cantidad { get; set; }

    public decimal Subtotal => Precio_Unitario * Cantidad;
}
