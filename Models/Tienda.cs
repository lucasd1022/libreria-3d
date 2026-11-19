namespace Libreria3D.Models;

// Datos de la tienda que se leen de appsettings.json (sección "Tienda").
public static class Tienda
{
    public static string Nombre { get; private set; } = "Librería 3D";
    public static string Email { get; private set; } = "";
    public static string WhatsApp { get; private set; } = "";

    public static void Configurar(IConfigurationSection seccion)
    {
        Nombre = seccion["Nombre"] is { Length: > 0 } nombre ? nombre : Nombre;
        Email = seccion["Email"] ?? "";
        WhatsApp = seccion["WhatsApp"] ?? "";
    }
}
