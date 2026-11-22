namespace Libreria3D.Models;

// Fotos de productos subidas desde el panel. Se guardan en wwwroot/uploads/productos con un nombre al azar.
public static class Archivos
{
    public const long TamanoMaximo = 5 * 1024 * 1024;

    public static readonly string[] ExtensionesFoto = { ".jpg", ".jpeg", ".png", ".webp" };

    private const string CarpetaPublica = "/uploads/productos/";

    /// <summary>Devuelve el error si el archivo no sirve, o null si está bien (o si no se subió nada).</summary>
    public static string? ValidarFoto(IFormFile? archivo)
    {
        if (archivo == null || archivo.Length == 0)
        {
            return null;
        }
        if (archivo.Length > TamanoMaximo)
        {
            return "La foto pesa más de 5 MB";
        }
        var extension = Path.GetExtension(archivo.FileName).ToLowerInvariant();
        if (!ExtensionesFoto.Contains(extension) || !FirmaValida(archivo, extension))
        {
            return "Subí una foto JPG, PNG o WEBP";
        }
        return null;
    }

    public static async Task<string> GuardarFoto(IFormFile archivo, string carpetaWwwroot)
    {
        var carpeta = Path.Combine(carpetaWwwroot, "uploads", "productos");
        Directory.CreateDirectory(carpeta);
        var nombre = $"{Guid.NewGuid():N}{Path.GetExtension(archivo.FileName).ToLowerInvariant()}";
        await using (var destino = File.Create(Path.Combine(carpeta, nombre)))
        {
            await archivo.CopyToAsync(destino);
        }
        return CarpetaPublica + nombre;
    }

    /// <summary>Borra una foto que se subió desde el panel. Las rutas que no son de /uploads/productos no se tocan.</summary>
    public static void BorrarFoto(string? rutaPublica, string carpetaWwwroot)
    {
        if (string.IsNullOrEmpty(rutaPublica) || !rutaPublica.StartsWith(CarpetaPublica) || rutaPublica.Contains(".."))
        {
            return;
        }
        var ruta = Path.Combine(carpetaWwwroot, rutaPublica.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
        if (File.Exists(ruta))
        {
            File.Delete(ruta);
        }
    }

    // Revisa los primeros bytes para que no se pueda subir otra cosa con extensión de imagen.
    private static bool FirmaValida(IFormFile archivo, string extension)
    {
        Span<byte> cabecera = stackalloc byte[12];
        using var flujo = archivo.OpenReadStream();
        var leidos = flujo.Read(cabecera);
        var b = cabecera[..leidos];

        return extension switch
        {
            ".jpg" or ".jpeg" => b.Length >= 3 && b[0] == 0xFF && b[1] == 0xD8 && b[2] == 0xFF,
            ".png" => b.Length >= 8 && b[..8].SequenceEqual(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }),
            ".webp" => b.Length >= 12 && b[..4].SequenceEqual("RIFF"u8) && b[8..12].SequenceEqual("WEBP"u8),
            _ => false
        };
    }
}
