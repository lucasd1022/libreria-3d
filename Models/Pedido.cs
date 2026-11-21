namespace Libreria3D.Models;

public class Pedido
{
    public const string Pendiente = "Pendiente";
    public const string Pagado = "Pagado";
    public const string ListoParaRetirar = "Listo para retirar";
    public const string Enviado = "Enviado";
    public const string Entregado = "Entregado";
    public const string Cancelado = "Cancelado";

    public const string EntregaEnvio = "Envio";
    public const string EntregaRetiro = "Retiro";

    public static readonly string[] Estados = { Pendiente, Pagado, ListoParaRetirar, Enviado, Entregado, Cancelado };

    /// <summary>Los estados que tienen sentido según cómo se entrega el pedido.</summary>
    public static string[] EstadosPara(string formaEntrega) =>
        formaEntrega == EntregaRetiro
            ? new[] { Pendiente, Pagado, ListoParaRetirar, Entregado, Cancelado }
            : new[] { Pendiente, Pagado, Enviado, Entregado, Cancelado };

    public int ID { get; set; }
    public string Nombre { get; set; } = "";
    public string Email { get; set; } = "";
    public string? Telefono { get; set; }
    public string Forma_Entrega { get; set; } = EntregaRetiro;
    public string? Direccion { get; set; }
    public string? Localidad { get; set; }
    public string? Codigo_Postal { get; set; }
    public string? Notas { get; set; }
    public decimal Costo_Envio { get; set; }
    public decimal Total { get; set; }
    public string Estado { get; set; } = Pendiente;
    public DateTime Fecha { get; set; }
    public int CantidadProductos { get; set; }

    public List<PedidoItem> Items { get; set; } = new();

    public bool EsEnvio => Forma_Entrega == EntregaEnvio;
    public string TextoEntrega => EsEnvio ? "Envío a domicilio" : "Retiro en el local";
    public decimal Subtotal => Total - Costo_Envio;
    public string DireccionEnvio => string.Join(", ", new[] { Direccion, Localidad, Codigo_Postal is null ? null : $"CP {Codigo_Postal}" }.Where(t => !string.IsNullOrEmpty(t)));
    public string ClaseEstado => "estado-" + Estado.ToLowerInvariant().Replace(' ', '-');
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
