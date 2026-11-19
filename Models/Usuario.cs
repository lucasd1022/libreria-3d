namespace Libreria3D.Models;

public class Usuario
{
    public int ID { get; set; }
    public string Nombre { get; set; } = "";
    public string Apellido { get; set; } = "";
    public string Email { get; set; } = "";

    public string? Contraseña { get; set; }

    public DateTime Fecha_Registro { get; set; }
    public bool Es_Administrador { get; set; }

    public string NombreCompleto => $"{Nombre} {Apellido}".Trim();
}
