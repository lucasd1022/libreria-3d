using System.Net;

namespace Libreria3D.Models;

// Datos del negocio que se leen de appsettings.json (sección "Tienda").
// Se muestran en el inicio, en "El local", en el pie de página y al confirmar un pedido.
public static class Tienda
{
    public static string Nombre { get; private set; } = "Librería 3D";
    public static string Descripcion { get; private set; } = "";
    public static string Direccion { get; private set; } = "";
    public static string Localidad { get; private set; } = "";
    public static string[] Horarios { get; private set; } = Array.Empty<string>();
    public static string Telefono { get; private set; } = "";
    public static string Email { get; private set; } = "";
    public static string WhatsApp { get; private set; } = "";
    public static string Instagram { get; private set; } = "";

    public static bool HaceEnvios { get; private set; } = true;
    public static string ZonaEnvio { get; private set; } = "";
    public static decimal CostoEnvio { get; private set; }
    public static decimal? EnvioGratisDesde { get; private set; }

    /// <summary>
    /// Link de WhatsApp armado a partir del número como lo escribe la gente (por ejemplo "11 5661 2418").
    /// A los celulares argentinos sin código de país les agrega 549.
    /// </summary>
    public static string LinkWhatsApp
    {
        get
        {
            var numero = new string(WhatsApp.Where(char.IsDigit).ToArray());
            if (numero.StartsWith("0")) numero = numero[1..];
            return "https://wa.me/" + (numero.StartsWith("54") ? numero : "549" + numero);
        }
    }

    public static string DireccionCompleta => string.Join(", ", new[] { Direccion, Localidad }.Where(t => t.Length > 0));

    public static string LinkMapa => "https://www.google.com/maps/search/?api=1&query=" + WebUtility.UrlEncode($"{Nombre}, {DireccionCompleta}");

    public static string MapaIncrustado => "https://www.google.com/maps?output=embed&q=" + WebUtility.UrlEncode(DireccionCompleta);

    /// <summary>Lo que cuesta el envío para un pedido con ese subtotal.</summary>
    public static decimal CostoEnvioPara(decimal subtotal) =>
        EnvioGratisDesde.HasValue && subtotal >= EnvioGratisDesde.Value ? 0 : CostoEnvio;

    public static string TextoCostoEnvio =>
        CostoEnvio == 0 ? "Gratis"
        : EnvioGratisDesde.HasValue ? $"{Vistas.Precio(CostoEnvio)} · gratis desde {Vistas.Precio(EnvioGratisDesde.Value)}"
        : Vistas.Precio(CostoEnvio);

    public static void Configurar(IConfigurationSection seccion)
    {
        Nombre = Texto(seccion, "Nombre", Nombre);
        Descripcion = Texto(seccion, "Descripcion", "");
        Direccion = Texto(seccion, "Direccion", "");
        Localidad = Texto(seccion, "Localidad", "");
        Horarios = seccion.GetSection("Horarios").Get<string[]>() ?? Array.Empty<string>();
        Telefono = Texto(seccion, "Telefono", "");
        Email = Texto(seccion, "Email", "");
        WhatsApp = Texto(seccion, "WhatsApp", "");
        Instagram = Texto(seccion, "Instagram", "").TrimStart('@');

        var envios = seccion.GetSection("Envios");
        HaceEnvios = envios.GetValue("Activo", true);
        ZonaEnvio = Texto(envios, "Zona", "");
        CostoEnvio = envios.GetValue("Costo", 0m);
        EnvioGratisDesde = envios.GetValue<decimal?>("GratisDesde");
    }

    private static string Texto(IConfigurationSection seccion, string clave, string porDefecto) =>
        seccion[clave] is { Length: > 0 } valor ? valor.Trim() : porDefecto;
}
