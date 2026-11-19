using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Libreria3D.Models;

public static class Cuentas
{
    /// <summary>Devuelve el usuario si el email y la contraseña son correctos y es administrador.</summary>
    public static Usuario? IniciarSesion(LoginViewModel modelo, ModelStateDictionary estado)
    {
        if (!estado.IsValid)
        {
            return null;
        }

        var usuario = BD.ObtenerUsuarioPorEmail(modelo.Email.Trim().ToLowerInvariant());
        // Se verifica igual aunque el usuario no exista, para no revelar qué emails están registrados.
        if (!Contrasenas.Verificar(modelo.Contrasena, usuario?.Contraseña) || usuario == null || !usuario.Es_Administrador)
        {
            estado.AddModelError(string.Empty, "El email o la contraseña no son correctos.");
            return null;
        }
        return usuario;
    }

    public static Resultado CambiarContrasena(int idUsuario, CambiarContrasenaViewModel modelo, ModelStateDictionary estado)
    {
        var usuario = BD.ObtenerUsuarioPorId(idUsuario);
        if (usuario == null || !Contrasenas.Verificar(modelo.Actual, usuario.Contraseña))
        {
            estado.AddModelError(nameof(modelo.Actual), "La contraseña actual no es correcta");
        }
        if (!estado.IsValid)
        {
            return Resultado.Mal("Revisá los datos.");
        }
        BD.CambiarContrasena(idUsuario, Contrasenas.Hashear(modelo.Nueva));
        return Resultado.Bien("Listo, cambiaste tu contraseña.");
    }
}
