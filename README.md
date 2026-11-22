# Librería 3D

![Logo de Librería 3D](wwwroot/img/favicon.svg)

## 📌 Descripción y objetivos del proyecto

**Librería 3D** es la página de compra online de una librería de barrio, hecha con **ASP.NET Core MVC**. No es un marketplace: es el sitio propio del negocio. Muestra sus productos, deja armar un carrito y hacer el pedido con **retiro sin cargo en el local** o **envío a domicilio**, y tiene un panel para que la librería cargue productos y prepare los pedidos.

### 🎯 Objetivos

- Que una persona pueda buscar productos, armar su carrito y hacer un pedido sin crear una cuenta, eligiendo si lo retira en el local o se lo envían.
- Que la librería pueda cargar y editar productos, controlar el stock y seguir cada pedido hasta que se retira o se entrega.
- Guardar y consultar toda la información en **SQL Server** usando **stored procedures** llamados con **Dapper**.

---

## 🧩 Funcionalidades implementadas

- **Catálogo** con buscador por texto y filtro por categoría, y una página para cada producto.
- **Inicio** con los datos del local (dirección, horarios, envíos), las categorías y los últimos productos cargados.
- **El local**: dirección, horarios, contacto y mapa de Google Maps.
- **Carrito** guardado en la sesión: agregar, cambiar la cantidad y quitar productos. No deja agregar más unidades que el stock disponible.
- **Finalizar compra** con dos formas de entrega:
  - **Retiro en el local**, sin cargo.
  - **Envío a domicilio**, con costo fijo y envío gratis desde un monto (se configuran en `appsettings.json`). Pide dirección y localidad.

  La persona deja su nombre, email y teléfono. El pedido se guarda, se descuenta el stock y se calcula el envío en una sola transacción: si mientras tanto se terminó el stock de algún producto, no se guarda nada y se le avisa. La dirección y el total con o sin envío se muestran según la opción elegida, solo con CSS.
- **Confirmación del pedido**, que solo puede ver quien lo hizo: si es retiro muestra la dirección, los horarios y cómo llegar; si es envío, la dirección de entrega.
- **Panel de administración** con inicio de sesión:
  - **Pedidos**: lista con filtros por forma de entrega y por estado, y detalle de cada pedido. Los estados dependen de la entrega: los de retiro pasan por "Listo para retirar" y los de envío por "Enviado" (Pendiente → Pagado → Listo para retirar / Enviado → Entregado, o Cancelado). El menú muestra cuántos pedidos están pendientes.
  - **Productos**: crear, editar, ocultar y eliminar productos, con precio, stock, categoría e imagen. Los pedidos viejos conservan el nombre y el precio con el que se vendió cada producto.
  - **Contraseña**: cambiar la contraseña del administrador.
- **Seguridad**: contraseñas guardadas con PBKDF2, todos los formularios protegidos con token antifalsificación y validación de todos los datos en el servidor.

---

## 🛠 Tecnologías

| Parte | Tecnología |
|-------|------------|
| Backend | ASP.NET Core MVC (.NET 9) |
| Base de datos | SQL Server |
| Acceso a datos | Dapper 2.1.89 + Microsoft.Data.SqlClient 7.1.1 |
| Frontend | HTML y CSS propio, sin frameworks ni JavaScript |

## 📂 Estructura del proyecto

| Carpeta | Qué tiene |
|---------|-----------|
| `Controllers/` | Reciben el pedido, llaman al Model y devuelven la vista |
| `Models/` | Clases del proyecto, validaciones y la clase `BD.cs` |
| `Views/` | Vistas Razor que leen los datos desde `ViewBag` |
| `wwwroot/` | La hoja de estilos `css/style.css` e imágenes |
| `SQL/` | Scripts de la base: estructura, datos de prueba y stored procedures |
| `script.sql` | Script completo para crear la base con datos y procedures |

| Controller | Qué hace |
|------------|----------|
| `HomeController` | Inicio, el local (dirección, horarios y mapa) y páginas de error |
| `ProductosController` | Catálogo y detalle de cada producto |
| `CarritoController` | Agregar, actualizar y quitar productos del carrito |
| `PedidosController` | Finalizar la compra (retiro o envío) y confirmación del pedido |
| `CuentaController` | Iniciar y cerrar la sesión del administrador |
| `PanelController` | Panel de administración: pedidos, productos y contraseña |

### 🎨 Estilos

Todo el diseño está en `wwwroot/css/style.css`, sin frameworks:

- **Mobile first**: las reglas base son para celular y las media queries solo usan `min-width` (768px tablet, 1024px escritorio) para sumar lo que cambia en pantallas más grandes.
- **Clases por diseño, no por contenido**: los nombres dicen cómo se ve o se acomoda algo (`tarjeta`, `grilla-2`, `banda-oscura`, `etiqueta-verde`, `fila-separada`), no qué muestra. Así la misma clase sirve para productos, pedidos o datos del local.
- Los colores, radios y espacios están en variables al principio del archivo (`--color-principal`, `--radio`, etc.).
- Lo que cambia según una opción (por ejemplo, la dirección y el total con envío) se muestra u oculta solo con CSS usando `.conmutador`, `.visible-con-a` y `.visible-con-b`.

Toda la base se usa desde `Models/BD.cs`. Cada método llama a un stored procedure con Dapper:

```c#
public static Producto? ObtenerProductoPorId(int id) =>
    Uno<Producto>("ObtenerProductoPorId", new { ID = id });
```

---

## ▶ Instrucciones para ejecutar

1. Clonar el repositorio:
```bash
git clone https://github.com/lucasd1022/libreria-3d.git
```
2. Entrar en la carpeta del proyecto:
```bash
cd libreria-3d
```
3. Crear la base de datos: abrir `script.sql` en SQL Server Management Studio y ejecutarlo. También se pueden ejecutar en orden `SQL/1_Estructura.sql`, `SQL/2_DatosDePrueba.sql` y `SQL/3_StoredProcedures.sql`.

> El script crea el usuario de base `alumno`, que usa el login `alumno` del servidor. Si tu SQL Server no tiene ese login, crealo antes de ejecutar el script.

4. Revisar la cadena de conexión en `appsettings.json`. Por defecto usa `Server=localhost;Database=Libreria3D;Trusted_Connection=True;`.
5. Revisar los datos del negocio en la sección `Tienda` de `appsettings.json`:

| Clave | Qué es |
|-------|--------|
| `Nombre`, `Descripcion` | Nombre del local y la frase que aparece en el inicio |
| `Direccion`, `Localidad` | Dónde se retiran los pedidos. También se usa para el mapa |
| `Horarios` | Lista de horarios, por ejemplo `["Lunes a viernes de 9 a 19 h", "Sábados de 9 a 13 h"]` |
| `Telefono`, `Email`, `WhatsApp`, `Instagram` | Contacto. Los que queden vacíos no se muestran. El WhatsApp se escribe como se marca en Argentina (por ejemplo `11 5661-2418`) y la página arma el link sola |
| `Envios:Activo` | `false` si el local solo hace retiro |
| `Envios:Zona` | Hasta dónde llegan los envíos, por ejemplo `CABA y GBA` |
| `Envios:Costo` | Costo del envío en pesos |
| `Envios:GratisDesde` | Desde qué monto el envío es gratis. Borrar la línea si nunca es gratis |

6. Ejecutar el programa:
```bash
dotnet watch run
```
7. Para entrar al panel, ir a **Ingresar** (al pie de la página) con el administrador de prueba:
   - Email: `admin@libreria3d.com`
   - Contraseña: `Libreria2026`

> Cambiá esa contraseña desde **Panel → Contraseña** antes de publicar la tienda.

---

## 🔜 Próximos pasos

- Cobrar online (por ejemplo, con Mercado Pago).
- Subir las fotos de los productos desde el panel (hoy se escribe la ruta o el link de la imagen).
- Mandar un mail al cliente cuando hace el pedido y cuando cambia de estado.
- Cuentas de cliente para ver el historial de pedidos.
- Calcular el costo de envío según la zona o el código postal.
