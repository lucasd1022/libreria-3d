using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using ClosedXML.Excel;

namespace Libreria3D.Models;

/// <summary>Una fila de la planilla de productos, ya leída y revisada.</summary>
public class FilaPlanilla
{
    public int Fila { get; set; }
    public string Codigo { get; set; } = "";
    public string? Nombre { get; set; }
    public string? Categoria { get; set; }
    public decimal? Precio { get; set; }
    public int? Stock { get; set; }
    public bool? Visible { get; set; }
    public string? Descripcion { get; set; }

    public bool EsNueva { get; set; }
    public List<string> Errores { get; set; } = new();

    public bool TieneErrores => Errores.Count > 0;
}

public class LecturaPlanilla
{
    public string? Error { get; set; }
    public List<FilaPlanilla> Filas { get; set; } = new();
    public List<string> ColumnasIgnoradas { get; set; } = new();

    public List<FilaPlanilla> Validas => Filas.Where(f => !f.TieneErrores).ToList();
    public List<FilaPlanilla> ConErrores => Filas.Where(f => f.TieneErrores).ToList();
    public int Nuevas => Filas.Count(f => !f.TieneErrores && f.EsNueva);
    public int Actualizaciones => Filas.Count(f => !f.TieneErrores && !f.EsNueva);
}

/// <summary>
/// Lee y escribe la planilla de productos (Excel .xlsx o CSV).
/// Cada fila se identifica por el código: si ya existe se actualiza, si no se crea.
/// Al actualizar, las celdas vacías dejan el valor que el producto ya tenía.
/// </summary>
public static class Planilla
{
    public const string FormatoCodigo = @"^[A-Za-z0-9._-]+$";
    public const int MaximoFilas = 10000;
    public const string TipoExcel = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

    private static readonly CultureInfo Invariante = CultureInfo.InvariantCulture;

    // Columnas en el orden de la plantilla, con otros nombres que se aceptan al leer.
    private static readonly (string Campo, string Titulo, string[] Alias)[] Columnas =
    {
        ("Codigo", "Código", new[] { "codigo", "cod", "sku", "ean", "codigo de barras", "codigo interno" }),
        ("Nombre", "Nombre", new[] { "nombre", "producto", "articulo" }),
        ("Categoria", "Categoría", new[] { "categoria", "rubro", "familia" }),
        ("Precio", "Precio", new[] { "precio", "precio de venta", "precio venta", "pvp", "precio final" }),
        ("Stock", "Stock", new[] { "stock", "cantidad", "existencia" }),
        ("Visible", "Visible", new[] { "visible", "activo", "publicado" }),
        ("Descripcion", "Descripción", new[] { "descripcion", "detalle" }),
    };

    // ---------------------------------------------- escribir ----------------------------------------------

    public static byte[] Plantilla() => Escribir(new List<Producto>());

    public static byte[] Exportar(List<Producto> productos) => Escribir(productos);

    private static byte[] Escribir(List<Producto> productos)
    {
        using var libro = new XLWorkbook();
        var hoja = libro.Worksheets.Add("Productos");

        for (var i = 0; i < Columnas.Length; i++)
        {
            hoja.Cell(1, i + 1).Value = Columnas[i].Titulo;
        }
        var encabezado = hoja.Range(1, 1, 1, Columnas.Length);
        encabezado.Style.Font.Bold = true;
        encabezado.Style.Font.FontColor = XLColor.White;
        encabezado.Style.Fill.BackgroundColor = XLColor.FromHtml("#9b2a8e");

        // El código va como texto para que Excel no convierta los códigos de barras a notación científica.
        hoja.Column(1).Style.NumberFormat.Format = "@";
        hoja.Column(4).Style.NumberFormat.Format = "#,##0.00";

        var fila = 2;
        foreach (var p in productos)
        {
            hoja.Cell(fila, 1).Value = p.Codigo ?? "";
            hoja.Cell(fila, 2).Value = p.Nombre;
            hoja.Cell(fila, 3).Value = p.Categoria ?? "";
            hoja.Cell(fila, 4).Value = p.Precio;
            hoja.Cell(fila, 5).Value = p.Stock;
            hoja.Cell(fila, 6).Value = p.Activo ? "Sí" : "No";
            hoja.Cell(fila, 7).Value = p.Descripcion ?? "";
            fila++;
        }

        hoja.SheetView.FreezeRows(1);
        hoja.Columns(1, 6).AdjustToContents(8.0, 45.0);
        hoja.Column(7).Width = 60;

        var ayuda = libro.Worksheets.Add("Instrucciones");
        var lineas = new[]
        {
            "Cómo usar esta planilla",
            "",
            "• Cada fila es un producto. El Código es obligatorio y no se puede repetir: puede ser el código de barras o uno interno (letras, números, guiones o puntos).",
            "• Si el código ya existe en la tienda, la fila actualiza ese producto. Si no existe, se crea un producto nuevo.",
            "• Para un producto nuevo son obligatorios Nombre y Precio. Stock vacío es 0 y Visible vacío es Sí.",
            "• Al actualizar, las celdas vacías no cambian nada: podés dejar solo Código y Precio para actualizar precios.",
            "• Categoría: si no existe, se crea sola.",
            "• Precio: sin el signo $ o con él, con o sin separador de miles (por ejemplo 4500 o $ 4.500,50).",
            "• Visible: Sí o No. Los productos con No quedan ocultos en la tienda.",
            "• Fotos: se suben aparte, desde el panel, nombrando cada archivo con el código del producto (por ejemplo CUA-A4-80.jpg).",
            "• Antes de aplicar los cambios, la tienda te muestra qué se va a crear, qué se va a actualizar y qué filas tienen errores.",
        };
        for (var i = 0; i < lineas.Length; i++)
        {
            ayuda.Cell(i + 1, 1).Value = lineas[i];
        }
        ayuda.Cell(1, 1).Style.Font.Bold = true;
        ayuda.Cell(1, 1).Style.Font.FontSize = 14;
        ayuda.Column(1).Width = 130;

        using var salida = new MemoryStream();
        libro.SaveAs(salida);
        return salida.ToArray();
    }

    // ---------------------------------------------- leer ----------------------------------------------

    /// <summary>Lee la planilla y revisa cada fila. No toca la base: solo consulta qué códigos ya existen.</summary>
    public static LecturaPlanilla Leer(IFormFile archivo, HashSet<string> codigosExistentes)
    {
        var extension = Path.GetExtension(archivo.FileName).ToLowerInvariant();
        List<List<object?>> celdas;
        try
        {
            using var flujo = archivo.OpenReadStream();
            celdas = extension switch
            {
                ".xlsx" => LeerExcel(flujo),
                ".csv" or ".txt" => LeerCsv(flujo),
                _ => throw new FormatException()
            };
        }
        catch (Exception)
        {
            return new LecturaPlanilla { Error = "No pudimos leer el archivo. Subí una planilla de Excel (.xlsx) o un archivo CSV." };
        }

        celdas = celdas.Where(f => f.Take(f.Count - 1).Any(c => !EstaVacia(c))).ToList();
        if (celdas.Count == 0)
        {
            return new LecturaPlanilla { Error = "La planilla está vacía." };
        }

        var lectura = new LecturaPlanilla();
        var mapa = MapearColumnas(celdas[0], lectura.ColumnasIgnoradas);
        if (!mapa.ContainsKey("Codigo"))
        {
            return new LecturaPlanilla { Error = "No encontramos la columna Código en la primera fila. Descargá la plantilla para ver el formato." };
        }
        if (celdas.Count - 1 > MaximoFilas)
        {
            return new LecturaPlanilla { Error = $"La planilla tiene más de {MaximoFilas} productos. Dividila en varias partes." };
        }

        var vistos = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        for (var i = 1; i < celdas.Count; i++)
        {
            var fila = LeerFila(celdas[i], mapa, celdas[i][^1] is int numero ? numero : i + 1);
            if (fila.Codigo.Length > 0)
            {
                if (vistos.TryGetValue(fila.Codigo, out var anterior))
                {
                    fila.Errores.Add($"El código está repetido (también en la fila {anterior})");
                }
                else
                {
                    vistos[fila.Codigo] = fila.Fila;
                }
                fila.EsNueva = !codigosExistentes.Contains(fila.Codigo);
                if (fila.EsNueva && fila.Nombre == null)
                {
                    fila.Errores.Add("Es un producto nuevo: falta el nombre");
                }
                if (fila.EsNueva && fila.Precio == null && !fila.Errores.Any(e => e.StartsWith("Precio")))
                {
                    fila.Errores.Add("Es un producto nuevo: falta el precio");
                }
            }
            lectura.Filas.Add(fila);
        }
        return lectura;
    }

    private static FilaPlanilla LeerFila(List<object?> valores, Dictionary<string, int> mapa, int numero)
    {
        object? Celda(string campo) => mapa.TryGetValue(campo, out var i) && i < valores.Count ? valores[i] : null;
        var fila = new FilaPlanilla { Fila = numero };

        fila.Codigo = Codigo(Celda("Codigo"));
        if (fila.Codigo.Length == 0)
        {
            fila.Errores.Add("Falta el código");
        }
        else if (fila.Codigo.Length > 40 || !Regex.IsMatch(fila.Codigo, FormatoCodigo))
        {
            fila.Errores.Add("Código inválido: usá hasta 40 letras, números, guiones o puntos, sin espacios");
        }

        fila.Nombre = Texto(Celda("Nombre"), 120, "Nombre", fila.Errores);
        fila.Categoria = Texto(Celda("Categoria"), 60, "Categoría", fila.Errores);
        fila.Descripcion = Texto(Celda("Descripcion"), 1000, "Descripción", fila.Errores);

        var precio = Celda("Precio");
        if (!EstaVacia(precio))
        {
            if (Numero(precio) is decimal p && p >= 0 && p <= 99_999_999)
                fila.Precio = Math.Round(p, 2);
            else
                fila.Errores.Add($"Precio inválido: \"{precio}\"");
        }

        var stock = Celda("Stock");
        if (!EstaVacia(stock))
        {
            if (Numero(stock) is decimal s && s >= 0 && s <= 1_000_000 && s == Math.Floor(s))
                fila.Stock = (int)s;
            else
                fila.Errores.Add($"Stock inválido: \"{stock}\" (tiene que ser un número entero, 0 o más)");
        }

        var visible = Celda("Visible");
        if (!EstaVacia(visible))
        {
            fila.Visible = Normalizar(visible!.ToString()!) switch
            {
                "si" or "s" or "x" or "1" or "true" or "verdadero" => true,
                "no" or "n" or "0" or "false" or "falso" => false,
                _ => null
            };
            if (fila.Visible == null)
            {
                fila.Errores.Add($"Visible inválido: \"{visible}\" (usá Sí o No)");
            }
        }
        return fila;
    }

    private static Dictionary<string, int> MapearColumnas(List<object?> encabezados, List<string> ignoradas)
    {
        var mapa = new Dictionary<string, int>();
        for (var i = 0; i < encabezados.Count; i++)
        {
            if (EstaVacia(encabezados[i]) || encabezados[i] is int) continue;
            var titulo = Normalizar(encabezados[i]!.ToString()!);
            var columna = Columnas.FirstOrDefault(c => c.Alias.Contains(titulo));
            if (columna.Campo != null && !mapa.ContainsKey(columna.Campo))
                mapa[columna.Campo] = i;
            else
                ignoradas.Add(encabezados[i]!.ToString()!.Trim());
        }
        return mapa;
    }

    // Cada fila leída termina con su número de fila real (int) para poder mostrarlo en los errores.
    private static List<List<object?>> LeerExcel(Stream flujo)
    {
        using var libro = new XLWorkbook(flujo);
        var hoja = libro.Worksheets.First();
        var filas = new List<List<object?>>();
        var ultimaColumna = hoja.LastColumnUsed()?.ColumnNumber() ?? 0;
        foreach (var fila in hoja.RowsUsed())
        {
            var valores = new List<object?>();
            for (var c = 1; c <= ultimaColumna; c++)
            {
                var celda = fila.Cell(c);
                valores.Add(celda.Value.Type switch
                {
                    XLDataType.Number => (object)celda.Value.GetNumber(),
                    XLDataType.Boolean => celda.Value.GetBoolean() ? "Sí" : "No",
                    XLDataType.Blank => null,
                    _ => celda.GetFormattedString()
                });
            }
            valores.Add(fila.RowNumber());
            filas.Add(valores);
        }
        return filas;
    }

    // CSV con ; (lo que exporta Excel en español) o con , ; con comillas para textos con separadores.
    private static List<List<object?>> LeerCsv(Stream flujo)
    {
        using var memoria = new MemoryStream();
        flujo.CopyTo(memoria);
        var bytes = memoria.ToArray();
        string texto;
        try
        {
            texto = new UTF8Encoding(false, true).GetString(bytes);
        }
        catch (DecoderFallbackException)
        {
            texto = Encoding.Latin1.GetString(bytes);
        }
        texto = texto.TrimStart('﻿');

        var primera = texto.Split('\n')[0];
        var separador = primera.Count(c => c == ';') >= primera.Count(c => c == ',') ? ';' : ',';

        var filas = new List<List<object?>>();
        var actual = new List<object?>();
        var campo = new StringBuilder();
        var entreComillas = false;
        var numeroFila = 1;
        for (var i = 0; i < texto.Length; i++)
        {
            var c = texto[i];
            if (entreComillas)
            {
                if (c == '"' && i + 1 < texto.Length && texto[i + 1] == '"') { campo.Append('"'); i++; }
                else if (c == '"') entreComillas = false;
                else campo.Append(c);
            }
            else if (c == '"') entreComillas = true;
            else if (c == separador) { actual.Add(campo.ToString()); campo.Clear(); }
            else if (c == '\n' || c == '\r')
            {
                if (c == '\r' && i + 1 < texto.Length && texto[i + 1] == '\n') i++;
                actual.Add(campo.ToString()); campo.Clear();
                actual.Add(numeroFila++);
                filas.Add(actual);
                actual = new List<object?>();
            }
            else campo.Append(c);
        }
        if (campo.Length > 0 || actual.Count > 0)
        {
            actual.Add(campo.ToString());
            actual.Add(numeroFila);
            filas.Add(actual);
        }
        return filas;
    }

    // ---------------------------------------------- conversiones ----------------------------------------------

    private static bool EstaVacia(object? valor) => valor is null || valor is string s && string.IsNullOrWhiteSpace(s);

    private static string Codigo(object? valor) => valor switch
    {
        null => "",
        // Los códigos de barras guardados como número vienen como double: se escriben sin decimales ni exponente.
        double d => ((decimal)d).ToString("0", Invariante),
        _ => valor.ToString()!.Trim()
    };

    private static string? Texto(object? valor, int largo, string nombre, List<string> errores)
    {
        if (EstaVacia(valor)) return null;
        var texto = valor is double d ? ((decimal)d).ToString(Invariante) : valor!.ToString()!.Trim();
        if (texto.Length > largo)
        {
            errores.Add($"{nombre} demasiado largo (máximo {largo} caracteres)");
            return null;
        }
        return texto;
    }

    /// <summary>Lee números como los escribe la gente: 4500, 4500.5, 4.500, $ 4.500,50.</summary>
    private static decimal? Numero(object? valor)
    {
        if (valor is double d) return (decimal)d;
        var t = valor?.ToString()?.Replace("$", "").Replace(" ", "").Replace(" ", "").Trim() ?? "";
        if (t.Length == 0) return null;
        if (t.Contains(','))
            t = t.Replace(".", "").Replace(",", ".");
        else if (Regex.IsMatch(t, @"^\d{1,3}(\.\d{3})+$"))
            t = t.Replace(".", "");
        return decimal.TryParse(t, NumberStyles.Number, Invariante, out var n) ? n : null;
    }

    private static string Normalizar(string texto)
    {
        var sinTildes = new string(texto.Normalize(NormalizationForm.FormD)
            .Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark).ToArray());
        return Regex.Replace(sinTildes.ToLowerInvariant().Trim(), @"\s+", " ");
    }
}
