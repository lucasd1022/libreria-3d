USE Libreria3D
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- ---------------------------------------------- usuarios ----------------------------------------------

CREATE OR ALTER PROCEDURE ObtenerUsuarioPorEmail
    @Email NVARCHAR(100)
AS
BEGIN
    SELECT ID, Nombre, Apellido, Email, [Contraseña], Fecha_Registro, Es_Administrador
    FROM Usuario
    WHERE Email = @Email
END
GO

CREATE OR ALTER PROCEDURE ObtenerUsuarioPorId
    @ID INT
AS
BEGIN
    SELECT ID, Nombre, Apellido, Email, [Contraseña], Fecha_Registro, Es_Administrador
    FROM Usuario
    WHERE ID = @ID
END
GO

CREATE OR ALTER PROCEDURE CambiarContrasena
    @IDUsuario INT,
    @Hash NVARCHAR(200)
AS
BEGIN
    UPDATE Usuario
    SET [Contraseña] = @Hash
    WHERE ID = @IDUsuario
END
GO

-- ---------------------------------------------- catálogo ----------------------------------------------

CREATE OR ALTER PROCEDURE ObtenerCategorias
AS
BEGIN
    SELECT c.ID, c.Nombre,
           (SELECT COUNT(*) FROM Producto p WHERE p.IDCategoria = c.ID AND p.Activo = 1) AS CantidadProductos
    FROM Categoria c
    ORDER BY c.Nombre
END
GO

CREATE OR ALTER PROCEDURE BuscarProductos
    @Texto NVARCHAR(100) = NULL,
    @IDCategoria INT = NULL,
    @SoloActivos BIT = 1
AS
BEGIN
    SELECT p.ID, p.IDCategoria, c.Nombre AS Categoria, p.Nombre, p.Descripcion, p.Precio, p.Stock, p.Imagen, p.Activo, p.Fecha_Alta
    FROM Producto p
    LEFT JOIN Categoria c ON c.ID = p.IDCategoria
    WHERE (@SoloActivos = 0 OR p.Activo = 1)
      AND (@IDCategoria IS NULL OR p.IDCategoria = @IDCategoria)
      AND (@Texto IS NULL OR p.Nombre LIKE N'%' + @Texto + N'%' OR p.Descripcion LIKE N'%' + @Texto + N'%')
    ORDER BY p.Nombre
END
GO

CREATE OR ALTER PROCEDURE ObtenerProductosNuevos
    @Cantidad INT
AS
BEGIN
    SELECT TOP (@Cantidad) p.ID, p.IDCategoria, c.Nombre AS Categoria, p.Nombre, p.Descripcion, p.Precio, p.Stock, p.Imagen, p.Activo, p.Fecha_Alta
    FROM Producto p
    LEFT JOIN Categoria c ON c.ID = p.IDCategoria
    WHERE p.Activo = 1 AND p.Stock > 0
    ORDER BY p.Fecha_Alta DESC, p.ID DESC
END
GO

CREATE OR ALTER PROCEDURE ObtenerProductoPorId
    @ID INT
AS
BEGIN
    SELECT p.ID, p.IDCategoria, c.Nombre AS Categoria, p.Nombre, p.Descripcion, p.Precio, p.Stock, p.Imagen, p.Activo, p.Fecha_Alta
    FROM Producto p
    LEFT JOIN Categoria c ON c.ID = p.IDCategoria
    WHERE p.ID = @ID
END
GO

-- @IDs es una lista separada por comas, por ejemplo N'1,5,8'
CREATE OR ALTER PROCEDURE ObtenerProductosPorIds
    @IDs NVARCHAR(MAX)
AS
BEGIN
    SELECT p.ID, p.IDCategoria, c.Nombre AS Categoria, p.Nombre, p.Descripcion, p.Precio, p.Stock, p.Imagen, p.Activo, p.Fecha_Alta
    FROM Producto p
    LEFT JOIN Categoria c ON c.ID = p.IDCategoria
    WHERE p.ID IN (SELECT TRY_CAST(value AS INT) FROM STRING_SPLIT(@IDs, N','))
END
GO

CREATE OR ALTER PROCEDURE CrearProducto
    @Nombre NVARCHAR(120),
    @Descripcion NVARCHAR(1000) = NULL,
    @Precio DECIMAL(12, 2),
    @Stock INT,
    @Imagen NVARCHAR(260) = NULL,
    @Activo BIT,
    @IDCategoria INT = NULL
AS
BEGIN
    SET NOCOUNT ON
    INSERT INTO Producto (Nombre, Descripcion, Precio, Stock, Imagen, Activo, IDCategoria)
    VALUES (@Nombre, @Descripcion, @Precio, @Stock, @Imagen, @Activo, @IDCategoria)

    SELECT CAST(SCOPE_IDENTITY() AS INT) AS ID
END
GO

CREATE OR ALTER PROCEDURE ActualizarProducto
    @ID INT,
    @Nombre NVARCHAR(120),
    @Descripcion NVARCHAR(1000) = NULL,
    @Precio DECIMAL(12, 2),
    @Stock INT,
    @Imagen NVARCHAR(260) = NULL,
    @Activo BIT,
    @IDCategoria INT = NULL
AS
BEGIN
    UPDATE Producto
    SET Nombre = @Nombre,
        Descripcion = @Descripcion,
        Precio = @Precio,
        Stock = @Stock,
        Imagen = @Imagen,
        Activo = @Activo,
        IDCategoria = @IDCategoria
    WHERE ID = @ID
END
GO

-- Los pedidos que tenían este producto lo conservan: PedidoItem guarda una copia del nombre y del precio.
CREATE OR ALTER PROCEDURE EliminarProducto
    @ID INT
AS
BEGIN
    DELETE FROM Producto
    WHERE ID = @ID
END
GO

-- ---------------------------------------------- pedidos ----------------------------------------------

CREATE OR ALTER PROCEDURE CrearPedido
    @Nombre NVARCHAR(100),
    @Email NVARCHAR(100),
    @Telefono NVARCHAR(30) = NULL,
    @Forma_Entrega NVARCHAR(10),
    @Direccion NVARCHAR(200) = NULL,
    @Localidad NVARCHAR(100) = NULL,
    @Codigo_Postal NVARCHAR(10) = NULL,
    @Notas NVARCHAR(500) = NULL
AS
BEGIN
    SET NOCOUNT ON
    INSERT INTO Pedido (Nombre, Email, Telefono, Forma_Entrega, Direccion, Localidad, Codigo_Postal, Notas)
    VALUES (@Nombre, @Email, @Telefono, @Forma_Entrega, @Direccion, @Localidad, @Codigo_Postal, @Notas)

    SELECT CAST(SCOPE_IDENTITY() AS INT) AS ID
END
GO

-- Descuenta el stock y agrega el producto al pedido con su precio actual.
-- Devuelve 0 si el producto no existe, está oculto o no alcanza el stock.
CREATE OR ALTER PROCEDURE AgregarProductoAlPedido
    @IDPedido INT,
    @IDProducto INT,
    @Cantidad INT
AS
BEGIN
    SET NOCOUNT ON

    UPDATE Producto
    SET Stock = Stock - @Cantidad
    WHERE ID = @IDProducto AND Activo = 1 AND Stock >= @Cantidad

    IF @@ROWCOUNT = 0
    BEGIN
        SELECT CAST(0 AS BIT) AS Agregado
        RETURN
    END

    INSERT INTO PedidoItem (IDPedido, IDProducto, Nombre, Precio_Unitario, Cantidad)
    SELECT @IDPedido, ID, Nombre, Precio, @Cantidad
    FROM Producto
    WHERE ID = @IDProducto

    SELECT CAST(1 AS BIT) AS Agregado
END
GO

-- Suma los productos y, si es con envío, el costo de envío (gratis si el subtotal llega a @EnvioGratisDesde).
CREATE OR ALTER PROCEDURE CalcularTotalPedido
    @IDPedido INT,
    @CostoEnvio DECIMAL(12, 2),
    @EnvioGratisDesde DECIMAL(12, 2) = NULL
AS
BEGIN
    DECLARE @Subtotal DECIMAL(12, 2) = ISNULL((SELECT SUM(Precio_Unitario * Cantidad) FROM PedidoItem WHERE IDPedido = @IDPedido), 0)

    UPDATE Pedido
    SET Costo_Envio = CASE
            WHEN Forma_Entrega = N'Envio' AND (@EnvioGratisDesde IS NULL OR @Subtotal < @EnvioGratisDesde) THEN @CostoEnvio
            ELSE 0
        END
    WHERE ID = @IDPedido

    UPDATE Pedido
    SET Total = @Subtotal + Costo_Envio
    WHERE ID = @IDPedido
END
GO

CREATE OR ALTER PROCEDURE ObtenerPedidos
    @Estado NVARCHAR(20) = NULL,
    @Forma_Entrega NVARCHAR(10) = NULL
AS
BEGIN
    SELECT p.ID, p.Nombre, p.Email, p.Telefono, p.Forma_Entrega, p.Direccion, p.Localidad, p.Codigo_Postal, p.Notas,
           p.Costo_Envio, p.Total, p.Estado, p.Fecha,
           ISNULL((SELECT SUM(i.Cantidad) FROM PedidoItem i WHERE i.IDPedido = p.ID), 0) AS CantidadProductos
    FROM Pedido p
    WHERE (@Estado IS NULL OR p.Estado = @Estado)
      AND (@Forma_Entrega IS NULL OR p.Forma_Entrega = @Forma_Entrega)
    ORDER BY p.Fecha DESC, p.ID DESC
END
GO

CREATE OR ALTER PROCEDURE ObtenerPedidoPorId
    @ID INT
AS
BEGIN
    SELECT p.ID, p.Nombre, p.Email, p.Telefono, p.Forma_Entrega, p.Direccion, p.Localidad, p.Codigo_Postal, p.Notas,
           p.Costo_Envio, p.Total, p.Estado, p.Fecha,
           ISNULL((SELECT SUM(i.Cantidad) FROM PedidoItem i WHERE i.IDPedido = p.ID), 0) AS CantidadProductos
    FROM Pedido p
    WHERE p.ID = @ID
END
GO

CREATE OR ALTER PROCEDURE ObtenerItemsDePedido
    @IDPedido INT
AS
BEGIN
    SELECT ID, IDPedido, IDProducto, Nombre, Precio_Unitario, Cantidad
    FROM PedidoItem
    WHERE IDPedido = @IDPedido
    ORDER BY Nombre
END
GO

CREATE OR ALTER PROCEDURE CambiarEstadoPedido
    @ID INT,
    @Estado NVARCHAR(20)
AS
BEGIN
    UPDATE Pedido
    SET Estado = @Estado
    WHERE ID = @ID
END
GO

CREATE OR ALTER PROCEDURE ContarPedidosPendientes
AS
BEGIN
    SELECT COUNT(*) AS Cantidad
    FROM Pedido
    WHERE Estado = N'Pendiente'
END
GO
