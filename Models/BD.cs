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
        Valor<int>("CrearProducto", new { p.Codigo, p.Nombre, p.Descripcion, p.Precio, p.Stock, p.Imagen, p.Activo, p.IDCategoria });

    public static void ActualizarProducto(int id, ProductoViewModel p) =>
        Ejecutar("ActualizarProducto", new { ID = id, p.Codigo, p.Nombre, p.Descripcion, p.Precio, p.Stock, p.Imagen, p.Activo, p.IDCategoria });

    public static bool ExisteCodigoProducto(string codigo, int? exceptoId = null) =>
        Valor<bool>("ExisteCodigoProducto", new { Codigo = codigo, Excepto = exceptoId });

    public static Producto? ObtenerProductoPorCodigo(string codigo) =>
        Uno<Producto>("ObtenerProductoPorCodigo", new { Codigo = codigo });

    public static HashSet<string> ObtenerCodigosDeProductos() =>
        new(Lista<string>("ObtenerCodigosDeProductos"), StringComparer.OrdinalIgnoreCase);

    public static void CambiarImagenProducto(int id, string? imagen) =>
        Ejecutar("CambiarImagenProducto", new { ID = id, Imagen = imagen });

    /// <summary>
    /// Crea o actualiza todas las filas de una planilla en una sola transacción:
    /// si una falla no queda nada a medias. Las categorías que no existen se crean.
    /// </summary>
    public static (int Creados, int Actualizados) ImportarProductos(IEnumerable<FilaPlanilla> filas)
    {
        using var conexion = Conectar();
        conexion.Open();
        using var tx = conexion.BeginTransaction();

        var categorias = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        int creados = 0, actualizados = 0;
        foreach (var fila in filas)
        {
            int? idCategoria = null;
            if (fila.Categoria != null)
            {
                if (!categorias.TryGetValue(fila.Categoria, out var id))
                {
                    id = conexion.ExecuteScalar<int>("ObtenerOCrearCategoria", new { Nombre = fila.Categoria }, tx, commandType: SP);
                    categorias[fila.Categoria] = id;
                }
                idCategoria = id;
            }

            var creado = conexion.ExecuteScalar<bool>("ImportarProducto", new
            {
                fila.Codigo,
                fila.Nombre,
                fila.Descripcion,
                fila.Precio,
                fila.Stock,
                Activo = fila.Visible,
                IDCategoria = idCategoria
            }, tx, commandType: SP);
            if (creado) creados++; else actualizados++;
        }

        tx.Commit();
        return (creados, actualizados);
    }

    public static void EliminarProducto(int id) =>
        Ejecutar("EliminarProducto", new { ID = id });

    // ---------------------------------------------- pedidos ----------------------------------------------

    /// <summary>
    /// Crea el pedido con todos sus productos y descuenta el stock en una sola transacción.
    /// Si de algún producto no alcanza el stock no se guarda nada y se devuelve su nombre.
    /// El costo de envío se calcula en la base con los precios guardados en el pedido.
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
            Forma_Entrega = d.FormaEntrega,
            d.Direccion,
            d.Localidad,
            Codigo_Postal = d.CodigoPostal,
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

        conexion.Execute("CalcularTotalPedido", new
        {
            IDPedido = idPedido,
            CostoEnvio = Tienda.CostoEnvio,
            EnvioGratisDesde = Tienda.EnvioGratisDesde
        }, tx, commandType: SP);
        tx.Commit();
        return (idPedido, null);
    }

    public static List<Pedido> ObtenerPedidos(string? estado, string? formaEntrega) =>
        Lista<Pedido>("ObtenerPedidos", new { Estado = estado, Forma_Entrega = formaEntrega });

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
