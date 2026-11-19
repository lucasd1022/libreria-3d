using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;

namespace Libreria3D.Models;

public static class BD
{
    private static string _connectionString = "";

    private const CommandType SP = CommandType.StoredProcedure;

    public static void Configurar(string connectionString)
    {
        _connectionString = connectionString;
    }

    private static SqlConnection Conectar() => new(_connectionString);

    private static List<T> Lista<T>(string procedimiento, object? parametros = null)
    {
        using var conexion = Conectar();
        return conexion.Query<T>(procedimiento, parametros, commandType: SP).ToList();
    }

    private static T? Uno<T>(string procedimiento, object? parametros = null)
    {
        using var conexion = Conectar();
        return conexion.QueryFirstOrDefault<T>(procedimiento, parametros, commandType: SP);
    }

    private static T Valor<T>(string procedimiento, object? parametros = null)
    {
        using var conexion = Conectar();
        return conexion.ExecuteScalar<T>(procedimiento, parametros, commandType: SP)!;
    }

    private static void Ejecutar(string procedimiento, object? parametros = null)
    {
        using var conexion = Conectar();
        conexion.Execute(procedimiento, parametros, commandType: SP);
    }

    // ---------------------------------------------- usuarios ----------------------------------------------

    public static Usuario? ObtenerUsuarioPorEmail(string email) =>
        Uno<Usuario>("ObtenerUsuarioPorEmail", new { Email = email });

    public static Usuario? ObtenerUsuarioPorId(int id) =>
        Uno<Usuario>("ObtenerUsuarioPorId", new { ID = id });

    public static void CambiarContrasena(int idUsuario, string hash) =>
        Ejecutar("CambiarContrasena", new { IDUsuario = idUsuario, Hash = hash });

    // ---------------------------------------------- catálogo ----------------------------------------------

    public static List<Categoria> ObtenerCategorias() =>
        Lista<Categoria>("ObtenerCategorias");

    public static List<Producto> BuscarProductos(string? texto, int? idCategoria, bool soloActivos = true) =>
        Lista<Producto>("BuscarProductos", new { Texto = texto, IDCategoria = idCategoria, SoloActivos = soloActivos });

    public static List<Producto> ObtenerProductosNuevos(int cantidad) =>
        Lista<Producto>("ObtenerProductosNuevos", new { Cantidad = cantidad });

    public static Producto? ObtenerProductoPorId(int id) =>
        Uno<Producto>("ObtenerProductoPorId", new { ID = id });

    public static List<Producto> ObtenerProductosPorIds(IEnumerable<int> ids)
    {
        var lista = string.Join(",", ids);
        return lista.Length == 0 ? new() : Lista<Producto>("ObtenerProductosPorIds", new { IDs = lista });
    }

    public static int CrearProducto(ProductoViewModel p) =>
        Valor<int>("CrearProducto", new { p.Nombre, p.Descripcion, p.Precio, p.Stock, p.Imagen, p.Activo, p.IDCategoria });

    public static void ActualizarProducto(int id, ProductoViewModel p) =>
        Ejecutar("ActualizarProducto", new { ID = id, p.Nombre, p.Descripcion, p.Precio, p.Stock, p.Imagen, p.Activo, p.IDCategoria });

    public static void EliminarProducto(int id) =>
        Ejecutar("EliminarProducto", new { ID = id });

    // ---------------------------------------------- pedidos ----------------------------------------------

    /// <summary>
    /// Crea el pedido con todos sus productos y descuenta el stock en una sola transacción.
    /// Si de algún producto no alcanza el stock no se guarda nada y se devuelve su nombre.
    /// </summary>
    public static (int? IDPedido, string? SinStock) CrearPedido(CheckoutViewModel d, IEnumerable<LineaCarrito> lineas)
    {
        using var conexion = Conectar();
        conexion.Open();
        using var tx = conexion.BeginTransaction();

        var idPedido = conexion.ExecuteScalar<int>("CrearPedido", new
        {
            d.Nombre,
            d.Email,
            d.Telefono,
            d.Direccion,
            d.Notas
        }, tx, commandType: SP);

        foreach (var linea in lineas)
        {
            var agregado = conexion.ExecuteScalar<bool>("AgregarProductoAlPedido", new
            {
                IDPedido = idPedido,
                IDProducto = linea.Producto.ID,
                linea.Cantidad
            }, tx, commandType: SP);
            if (!agregado)
            {
                tx.Rollback();
                return (null, linea.Producto.Nombre);
            }
        }

        conexion.Execute("CalcularTotalPedido", new { IDPedido = idPedido }, tx, commandType: SP);
        tx.Commit();
        return (idPedido, null);
    }

    public static List<Pedido> ObtenerPedidos(string? estado) =>
        Lista<Pedido>("ObtenerPedidos", new { Estado = estado });

    public static Pedido? ObtenerPedidoPorId(int id)
    {
        var pedido = Uno<Pedido>("ObtenerPedidoPorId", new { ID = id });
        if (pedido != null)
        {
            pedido.Items = Lista<PedidoItem>("ObtenerItemsDePedido", new { IDPedido = id });
        }
        return pedido;
    }

    public static void CambiarEstadoPedido(int id, string estado) =>
        Ejecutar("CambiarEstadoPedido", new { ID = id, Estado = estado });

    public static int ContarPedidosPendientes() =>
        Valor<int>("ContarPedidosPendientes");
}
