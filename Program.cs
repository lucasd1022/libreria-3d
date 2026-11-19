using System.Globalization;
using Microsoft.AspNetCore.Authentication.Cookies;
using Libreria3D.Models;

// Los números de los formularios (precios) llegan con punto decimal, sin importar el idioma del servidor.
CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddAntiforgery(opciones => opciones.Cookie.Name = "Libreria3D.Seguridad");
builder.Services.Configure<Microsoft.AspNetCore.Mvc.CookieTempDataProviderOptions>(opciones => opciones.Cookie.Name = "Libreria3D.Avisos");

// El carrito se guarda en la sesión del visitante.
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(opciones =>
{
    opciones.Cookie.Name = "Libreria3D.Carrito";
    opciones.Cookie.HttpOnly = true;
    opciones.Cookie.IsEssential = true;
    opciones.Cookie.SameSite = SameSiteMode.Lax;
    opciones.IdleTimeout = TimeSpan.FromDays(2);
});

builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(opciones =>
    {
        opciones.LoginPath = "/Cuenta/IniciarSesion";
        opciones.LogoutPath = "/Cuenta/CerrarSesion";
        opciones.AccessDeniedPath = "/Cuenta/IniciarSesion";
        opciones.Cookie.Name = "Libreria3D.Sesion";
        opciones.Cookie.HttpOnly = true;
        opciones.Cookie.SameSite = SameSiteMode.Lax;
        opciones.ExpireTimeSpan = TimeSpan.FromDays(7);
        opciones.SlidingExpiration = true;
    });
builder.Services.AddAuthorization(opciones =>
    opciones.AddPolicy(UsuarioActual.PoliticaAdministrador, politica => politica.RequireClaim(UsuarioActual.ClaimAdministrador, "true")));

BD.Configurar(builder.Configuration.GetConnectionString("Libreria3D")
    ?? throw new InvalidOperationException("Falta la cadena de conexión \"Libreria3D\" en appsettings.json"));
Tienda.Configurar(builder.Configuration.GetSection("Tienda"));

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/Home/Estado", "?codigo={0}");

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
