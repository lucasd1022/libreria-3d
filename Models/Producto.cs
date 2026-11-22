namespace Libreria3D.Models;

public class Producto
{
    public int ID { get; set; }
    public int? IDCategoria { get; set; }
    public string? Categoria { get; set; }
    public string? Codigo { get; set; }
    public string Nombre { get; set; } = "";
    public string? Descripcion { get; set; }
    public decimal Precio { get; set; }
    public int Stock { get; set; }
    public string? Imagen { get; set; }
    public bool Activo { get; set; }
    public DateTime Fecha_Alta { get; set; }

    public bool HayStock => Stock > 0;
    public string Inicial => Nombre.Trim().Length > 0 ? Nombre.Trim()[..1].ToUpperInvariant() : "?";
}
