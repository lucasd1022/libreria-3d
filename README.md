# Librería 3D

![Logo de Librería 3D](wwwroot/img/favicon.svg)

## 📌 Descripción y objetivos del proyecto

**Librería 3D** es una tienda online de artículos de librería hecha con **ASP.NET Core MVC**. Muestra el catálogo de productos, permite armar un carrito y hacer el pedido, y tiene un panel para que la librería cargue productos y siga los pedidos.

### 🎯 Objetivos

- Que una persona pueda buscar productos, armar su carrito y hacer un pedido sin crear una cuenta.
- Que la librería pueda cargar y editar productos, controlar el stock y seguir cada pedido hasta que se entrega.
- Guardar y consultar toda la información en **SQL Server** usando **stored procedures** llamados con **Dapper**.

---

## 🧩 Funcionalidades implementadas

- **Catálogo** con buscador por texto y filtro por categoría, y una página para cada producto.
- **Inicio** con las categorías y los últimos productos cargados.
- **Carrito** guardado en la sesión: agregar, cambiar la cantidad y quitar productos. No deja agregar más unidades que el stock disponible.
- **Finalizar compra**: la persona deja su nombre, email y, si quiere, teléfono, dirección y notas. El pedido se guarda y el stock se descuenta en una sola transacción: si mientras tanto se terminó el stock de algún producto, no se guarda nada y se le avisa.
- **Confirmación del pedido**, que solo puede ver quien lo hizo.
- **Panel de administración** con inicio de sesión:
  - **Pedidos**: lista con filtro por estado, detalle de cada pedido y cambio de estado (Pendiente, Pagado, Enviado, Entregado, Cancelado). El menú muestra cuántos pedidos están pendientes.
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
| Frontend | HTML, CSS y Bootstrap 6 (sin JavaScript propio) |

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
| `HomeController` | Inicio, contacto y páginas de error |
| `ProductosController` | Catálogo y detalle de cada producto |
| `CarritoController` | Agregar, actualizar y quitar productos del carrito |
| `PedidosController` | Finalizar la compra y confirmación del pedido |
| `CuentaController` | Iniciar y cerrar la sesión del administrador |
| `PanelController` | Panel de administración: pedidos, productos y contraseña |

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

4. Revisar la cadena de conexión en `appsettings.json`. Por defecto usa `Server=localhost;Database=Libreria3D;Trusted_Connection=True;`. Ahí también están el nombre, el email y el WhatsApp de la tienda que se muestran en la página de contacto.
5. Ejecutar el programa:
```bash
dotnet watch run
```
6. Para entrar al panel, ir a **Ingresar** (al pie de la página) con el administrador de prueba:
   - Email: `admin@libreria3d.com`
   - Contraseña: `Libreria2026`

> Cambiá esa contraseña desde **Panel → Contraseña** antes de publicar la tienda.

---

## 🔜 Próximos pasos

- Cobrar online (por ejemplo, con Mercado Pago).
- Subir las fotos de los productos desde el panel (hoy se escribe la ruta o el link de la imagen).
- Mandar un mail al cliente cuando hace el pedido y cuando cambia de estado.
- Cuentas de cliente para ver el historial de pedidos.
