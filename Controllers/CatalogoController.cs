using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Libreria3D.Models;

namespace Libreria3D.Controllers;

// Carga masiva del catálogo: planillas de Excel (importar y exportar) y fotos en lote.
[Authorize(Policy = UsuarioActual.PoliticaAdministrador)]
public class CatalogoController : Controller
{
    private const string ClaveRevision = "PlanillaEnRevision";
    private const long LimiteFotos = 200 * 1024 * 1024;

    private readonly IWebHostEnvironment _entorno;

    public CatalogoController(IWebHostEnvironment entorno)
    {
        _entorno = entorno;
    }

    public IActionResult Index() => View();

    public IActionResult Plantilla() =>
        File(Planilla.Plantilla(), Planilla.TipoExcel, "plantilla-productos.xlsx");

    public IActionResult Exportar() =>
        File(Planilla.Exportar(BD.BuscarProductos(null, null, soloActivos: false)), Planilla.TipoExcel,
             $"productos-{DateTime.Now:yyyy-MM-dd}.xlsx");

    // ---------------------------------------------- importar ----------------------------------------------

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Importar(IFormFile? planilla)
    {
        if (planilla == null || planilla.Length == 0)
        {
            return Volver(Resultado.Mal("Elegí la planilla que querés subir."));
        }

        var lectura = Planilla.Leer(planilla, BD.ObtenerCodigosDeProductos());
        if (lectura.Error != null)
        {
            return Volver(Resultado.Mal(lectura.Error));
        }

        // Las filas válidas quedan guardadas en la sesión hasta que se confirma o se cancela.
        HttpContext.Session.SetString(ClaveRevision, JsonSerializer.Serialize(lectura.Validas));
        ViewBag.Lectura = lectura;
        ViewBag.Archivo = planilla.FileName;
        return View("Revisar");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Confirmar()
    {
        var json = HttpContext.Session.GetString(ClaveRevision);
        HttpContext.Session.Remove(ClaveRevision);
        var filas = json == null ? null : JsonSerializer.Deserialize<List<FilaPlanilla>>(json);
        if (filas == null || filas.Count == 0)
        {
            return Volver(Resultado.Mal("No hay ninguna planilla para aplicar. Subila de nuevo."));
        }

        var (creados, actualizados) = BD.ImportarProductos(filas);
        TempData["Mensaje"] = $"Listo: {Cantidad(creados, "producto nuevo", "productos nuevos")} y {Cantidad(actualizados, "producto actualizado", "productos actualizados")}.";
        TempData["MensajeTipo"] = "exito";
        return RedirectToAction("Productos", "Panel");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Cancelar()
    {
        HttpContext.Session.Remove(ClaveRevision);
        return Volver(Resultado.Bien("No se aplicó ningún cambio."));
    }

    // ---------------------------------------------- fotos ----------------------------------------------

    /// <summary>
    /// Cada foto se asigna al producto cuyo código es el nombre del archivo (CUA-A4-80.jpg → CUA-A4-80).
    /// Si el producto ya tenía una foto subida, se reemplaza.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(LimiteFotos)]
    [RequestFormLimits(MultipartBodyLengthLimit = LimiteFotos)]
    public async Task<IActionResult> Fotos(List<IFormFile> fotos)
    {
        if (fotos.Count == 0)
        {
            return Volver(Resultado.Mal("Elegí las fotos que querés subir."));
        }

        var asignadas = new List<string>();
        var sinProducto = new List<string>();
        var invalidas = new List<string>();
        foreach (var foto in fotos)
        {
            var nombre = Path.GetFileName(foto.FileName);
            var error = Archivos.ValidarFoto(foto);
            if (error != null)
            {
                invalidas.Add($"{nombre}: {error}");
                continue;
            }

            var producto = BD.ObtenerProductoPorCodigo(Path.GetFileNameWithoutExtension(nombre).Trim());
            if (producto == null)
            {
                sinProducto.Add(nombre);
                continue;
            }

            var ruta = await Archivos.GuardarFoto(foto, _entorno.WebRootPath);
            BD.CambiarImagenProducto(producto.ID, ruta);
            Archivos.BorrarFoto(producto.Imagen, _entorno.WebRootPath);
            asignadas.Add($"{nombre} → {producto.Nombre}");
        }

        ViewBag.Asignadas = asignadas;
        ViewBag.SinProducto = sinProducto;
        ViewBag.Invalidas = invalidas;
        return View("Index");
    }

    private IActionResult Volver(Resultado resultado)
    {
        TempData["Mensaje"] = resultado.Mensaje;
        TempData["MensajeTipo"] = resultado.Tipo;
        return RedirectToAction(nameof(Index));
    }

    private static string Cantidad(int n, string singular, string plural) => $"{n} {(n == 1 ? singular : plural)}";
}
