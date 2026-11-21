using System.ComponentModel.DataAnnotations;

namespace Libreria3D.Models;

public class LoginViewModel
{
    [Required(ErrorMessage = "Ingresá tu email")]
    [EmailAddress(ErrorMessage = "Ingresá un email válido")]
    [StringLength(100)]
    public string Email { get; set; } = "";

    [Required(ErrorMessage = "Ingresá tu contraseña")]
    [DataType(DataType.Password)]
    public string Contrasena { get; set; } = "";

    public string? ReturnUrl { get; set; }
}

public class CambiarContrasenaViewModel
{
    [Required(ErrorMessage = "Ingresá tu contraseña actual")]
    public string Actual { get; set; } = "";

    [Required(ErrorMessage = "Elegí una contraseña nueva")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "La contraseña debe tener entre 8 y 100 caracteres")]
    [RegularExpression(@"^(?=.*[A-Za-zÁÉÍÓÚÜÑáéíóúüñ])(?=.*\d).+$", ErrorMessage = "Usá al menos una letra y un número")]
    public string Nueva { get; set; } = "";

    [Required(ErrorMessage = "Repetí la contraseña nueva")]
    [Compare(nameof(Nueva), ErrorMessage = "Las contraseñas no coinciden")]
    public string Repetir { get; set; } = "";
}

public class CheckoutViewModel
{
    [Required(ErrorMessage = "Completá tu nombre y apellido")]
    [StringLength(100, ErrorMessage = "Usá hasta 100 caracteres")]
    public string Nombre { get; set; } = "";

    [Required(ErrorMessage = "Ingresá tu email")]
    [EmailAddress(ErrorMessage = "Ingresá un email válido")]
    [StringLength(100, ErrorMessage = "Usá hasta 100 caracteres")]
    public string Email { get; set; } = "";

    [Required(ErrorMessage = "Ingresá un teléfono para avisarte")]
    [StringLength(30, ErrorMessage = "Usá hasta 30 caracteres")]
    [RegularExpression(@"^[\d\s()+-]{6,30}$", ErrorMessage = "Usá solo números, espacios, paréntesis o guiones")]
    public string Telefono { get; set; } = "";

    public string FormaEntrega { get; set; } = Pedido.EntregaRetiro;

    [StringLength(200, ErrorMessage = "Usá hasta 200 caracteres")]
    public string? Direccion { get; set; }

    [StringLength(100, ErrorMessage = "Usá hasta 100 caracteres")]
    public string? Localidad { get; set; }

    [StringLength(10, ErrorMessage = "Usá hasta 10 caracteres")]
    public string? CodigoPostal { get; set; }

    [StringLength(500, ErrorMessage = "Usá hasta 500 caracteres")]
    public string? Notas { get; set; }

    public bool EsEnvio => FormaEntrega == Pedido.EntregaEnvio;
}

public class ProductoViewModel
{
    [Required(ErrorMessage = "Completá el nombre")]
    [StringLength(120, ErrorMessage = "Usá hasta 120 caracteres")]
    public string Nombre { get; set; } = "";

    [StringLength(1000, ErrorMessage = "Usá hasta 1000 caracteres")]
    public string? Descripcion { get; set; }

    [Required(ErrorMessage = "Completá el precio")]
    [Range(typeof(decimal), "0", "99999999", ErrorMessage = "El precio tiene que ser 0 o más")]
    public decimal Precio { get; set; }

    [Required(ErrorMessage = "Completá el stock")]
    [Range(0, 1000000, ErrorMessage = "El stock tiene que ser 0 o más")]
    public int Stock { get; set; }

    public int? IDCategoria { get; set; }

    [StringLength(260, ErrorMessage = "Usá hasta 260 caracteres")]
    [RegularExpression(@"^\s*(/|https?://).*$", ErrorMessage = "Usá una ruta que empiece con / o una dirección https://")]
    public string? Imagen { get; set; }

    // Sin marcar el checkbox no llega nada, por eso arranca en false.
    public bool Activo { get; set; }

    public static ProductoViewModel Desde(Producto p) => new()
    {
        Nombre = p.Nombre,
        Descripcion = p.Descripcion,
        Precio = p.Precio,
        Stock = p.Stock,
        IDCategoria = p.IDCategoria,
        Imagen = p.Imagen,
        Activo = p.Activo
    };
}
