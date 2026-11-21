USE [master]
GO
CREATE DATABASE [Libreria3D]
GO
ALTER DATABASE [Libreria3D] SET COMPATIBILITY_LEVEL = 140
GO
ALTER DATABASE [Libreria3D] SET RECOVERY SIMPLE
GO
ALTER DATABASE [Libreria3D] SET MULTI_USER
GO
USE [Libreria3D]
GO
CREATE USER [alumno] FOR LOGIN [alumno] WITH DEFAULT_SCHEMA=[dbo]
GO
ALTER ROLE [db_owner] ADD MEMBER [alumno]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Usuario](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Nombre] [nvarchar](50) NOT NULL,
	[Apellido] [nvarchar](50) NOT NULL,
	[Email] [nvarchar](100) NOT NULL,
	[Contraseña] [nvarchar](200) NOT NULL,
	[Fecha_Registro] [date] NOT NULL,
	[Es_Administrador] [bit] NOT NULL,
 CONSTRAINT [PK_Usuario] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY],
 CONSTRAINT [UQ_Usuario_Email] UNIQUE NONCLUSTERED 
(
	[Email] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Categoria](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Nombre] [nvarchar](60) NOT NULL,
 CONSTRAINT [PK_Categoria] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY],
 CONSTRAINT [UQ_Categoria_Nombre] UNIQUE NONCLUSTERED 
(
	[Nombre] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Producto](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[IDCategoria] [int] NULL,
	[Nombre] [nvarchar](120) NOT NULL,
	[Descripcion] [nvarchar](1000) NULL,
	[Precio] [decimal](12, 2) NOT NULL,
	[Stock] [int] NOT NULL,
	[Imagen] [nvarchar](260) NULL,
	[Activo] [bit] NOT NULL,
	[Fecha_Alta] [datetime2](0) NOT NULL,
 CONSTRAINT [PK_Producto] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Pedido](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Nombre] [nvarchar](100) NOT NULL,
	[Email] [nvarchar](100) NOT NULL,
	[Telefono] [nvarchar](30) NULL,
	[Forma_Entrega] [nvarchar](10) NOT NULL,
	[Direccion] [nvarchar](200) NULL,
	[Localidad] [nvarchar](100) NULL,
	[Codigo_Postal] [nvarchar](10) NULL,
	[Notas] [nvarchar](500) NULL,
	[Costo_Envio] [decimal](12, 2) NOT NULL,
	[Total] [decimal](12, 2) NOT NULL,
	[Estado] [nvarchar](20) NOT NULL,
	[Fecha] [datetime2](0) NOT NULL,
 CONSTRAINT [PK_Pedido] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PedidoItem](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[IDPedido] [int] NOT NULL,
	[IDProducto] [int] NULL,
	[Nombre] [nvarchar](120) NOT NULL,
	[Precio_Unitario] [decimal](12, 2) NOT NULL,
	[Cantidad] [int] NOT NULL,
 CONSTRAINT [PK_PedidoItem] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [IX_Producto_Categoria] ON [dbo].[Producto]
(
	[IDCategoria] ASC
)
GO
CREATE NONCLUSTERED INDEX [IX_PedidoItem_Pedido] ON [dbo].[PedidoItem]
(
	[IDPedido] ASC
)
GO
ALTER TABLE [dbo].[Usuario] ADD  CONSTRAINT [DF_Usuario_Fecha_Registro]  DEFAULT (CONVERT([date],getdate())) FOR [Fecha_Registro]
GO
ALTER TABLE [dbo].[Usuario] ADD  CONSTRAINT [DF_Usuario_Es_Administrador]  DEFAULT ((0)) FOR [Es_Administrador]
GO
ALTER TABLE [dbo].[Producto] ADD  CONSTRAINT [DF_Producto_Stock]  DEFAULT ((0)) FOR [Stock]
GO
ALTER TABLE [dbo].[Producto] ADD  CONSTRAINT [DF_Producto_Activo]  DEFAULT ((1)) FOR [Activo]
GO
ALTER TABLE [dbo].[Producto] ADD  CONSTRAINT [DF_Producto_Fecha_Alta]  DEFAULT (getdate()) FOR [Fecha_Alta]
GO
ALTER TABLE [dbo].[Pedido] ADD  CONSTRAINT [DF_Pedido_Forma_Entrega]  DEFAULT (N'Retiro') FOR [Forma_Entrega]
GO
ALTER TABLE [dbo].[Pedido] ADD  CONSTRAINT [DF_Pedido_Costo_Envio]  DEFAULT ((0)) FOR [Costo_Envio]
GO
ALTER TABLE [dbo].[Pedido] ADD  CONSTRAINT [DF_Pedido_Total]  DEFAULT ((0)) FOR [Total]
GO
ALTER TABLE [dbo].[Pedido] ADD  CONSTRAINT [DF_Pedido_Estado]  DEFAULT (N'Pendiente') FOR [Estado]
GO
ALTER TABLE [dbo].[Pedido] ADD  CONSTRAINT [DF_Pedido_Fecha]  DEFAULT (getdate()) FOR [Fecha]
GO
ALTER TABLE [dbo].[Producto]  WITH CHECK ADD  CONSTRAINT [FK_Producto_Categoria] FOREIGN KEY([IDCategoria])
REFERENCES [dbo].[Categoria] ([ID])
ON DELETE SET NULL
GO
ALTER TABLE [dbo].[Producto] CHECK CONSTRAINT [FK_Producto_Categoria]
GO
ALTER TABLE [dbo].[PedidoItem]  WITH CHECK ADD  CONSTRAINT [FK_PedidoItem_Pedido] FOREIGN KEY([IDPedido])
REFERENCES [dbo].[Pedido] ([ID])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[PedidoItem] CHECK CONSTRAINT [FK_PedidoItem_Pedido]
GO
ALTER TABLE [dbo].[PedidoItem]  WITH CHECK ADD  CONSTRAINT [FK_PedidoItem_Producto] FOREIGN KEY([IDProducto])
REFERENCES [dbo].[Producto] ([ID])
ON DELETE SET NULL
GO
ALTER TABLE [dbo].[PedidoItem] CHECK CONSTRAINT [FK_PedidoItem_Producto]
GO
ALTER TABLE [dbo].[Producto]  WITH CHECK ADD  CONSTRAINT [CK_Producto_Precio] CHECK  (([Precio]>=(0)))
GO
ALTER TABLE [dbo].[Producto] CHECK CONSTRAINT [CK_Producto_Precio]
GO
ALTER TABLE [dbo].[Producto]  WITH CHECK ADD  CONSTRAINT [CK_Producto_Stock] CHECK  (([Stock]>=(0)))
GO
ALTER TABLE [dbo].[Producto] CHECK CONSTRAINT [CK_Producto_Stock]
GO
ALTER TABLE [dbo].[Pedido]  WITH CHECK ADD  CONSTRAINT [CK_Pedido_Estado] CHECK  (([Estado]=N'Pendiente' OR [Estado]=N'Pagado' OR [Estado]=N'Listo para retirar' OR [Estado]=N'Enviado' OR [Estado]=N'Entregado' OR [Estado]=N'Cancelado'))
GO
ALTER TABLE [dbo].[Pedido] CHECK CONSTRAINT [CK_Pedido_Estado]
GO
ALTER TABLE [dbo].[Pedido]  WITH CHECK ADD  CONSTRAINT [CK_Pedido_Forma_Entrega] CHECK  (([Forma_Entrega]=N'Envio' OR [Forma_Entrega]=N'Retiro'))
GO
ALTER TABLE [dbo].[Pedido] CHECK CONSTRAINT [CK_Pedido_Forma_Entrega]
GO
ALTER TABLE [dbo].[Pedido]  WITH CHECK ADD  CONSTRAINT [CK_Pedido_Direccion_Envio] CHECK  (([Forma_Entrega]=N'Retiro' OR [Direccion] IS NOT NULL AND [Localidad] IS NOT NULL))
GO
ALTER TABLE [dbo].[Pedido] CHECK CONSTRAINT [CK_Pedido_Direccion_Envio]
GO
ALTER TABLE [dbo].[PedidoItem]  WITH CHECK ADD  CONSTRAINT [CK_PedidoItem_Cantidad] CHECK  (([Cantidad]>(0)))
GO
ALTER TABLE [dbo].[PedidoItem] CHECK CONSTRAINT [CK_PedidoItem_Cantidad]
GO

SET IDENTITY_INSERT [dbo].[Categoria] ON 

INSERT [dbo].[Categoria] ([ID], [Nombre]) VALUES (1, N'Cuadernos')
INSERT [dbo].[Categoria] ([ID], [Nombre]) VALUES (2, N'Escritura')
INSERT [dbo].[Categoria] ([ID], [Nombre]) VALUES (3, N'Arte y dibujo')
INSERT [dbo].[Categoria] ([ID], [Nombre]) VALUES (4, N'Organización')
INSERT [dbo].[Categoria] ([ID], [Nombre]) VALUES (5, N'Oficina')
SET IDENTITY_INSERT [dbo].[Categoria] OFF
GO
SET IDENTITY_INSERT [dbo].[Producto] ON 

INSERT [dbo].[Producto] ([ID], [IDCategoria], [Nombre], [Descripcion], [Precio], [Stock], [Imagen], [Activo]) VALUES (1, 1, N'Cuaderno A4 rayado 80 hojas', N'Tapa dura, espiral metálico, hojas de 80 g.', CAST(4500.00 AS Decimal(12, 2)), 25, NULL, 1)
INSERT [dbo].[Producto] ([ID], [IDCategoria], [Nombre], [Descripcion], [Precio], [Stock], [Imagen], [Activo]) VALUES (2, 1, N'Cuaderno A5 punteado', N'Ideal para bullet journal. 96 hojas de 100 g.', CAST(3900.00 AS Decimal(12, 2)), 18, NULL, 1)
INSERT [dbo].[Producto] ([ID], [IDCategoria], [Nombre], [Descripcion], [Precio], [Stock], [Imagen], [Activo]) VALUES (3, 2, N'Bolígrafos azules x10', N'Trazo medio 1.0 mm, tinta de secado rápido.', CAST(2800.00 AS Decimal(12, 2)), 40, NULL, 1)
INSERT [dbo].[Producto] ([ID], [IDCategoria], [Nombre], [Descripcion], [Precio], [Stock], [Imagen], [Activo]) VALUES (4, 2, N'Resaltadores pastel x6', N'Punta biselada, tonos pastel.', CAST(3300.00 AS Decimal(12, 2)), 30, NULL, 1)
INSERT [dbo].[Producto] ([ID], [IDCategoria], [Nombre], [Descripcion], [Precio], [Stock], [Imagen], [Activo]) VALUES (5, 3, N'Fibras de colores x24', N'Punta fina, lavables, colores intensos.', CAST(6200.00 AS Decimal(12, 2)), 12, NULL, 1)
INSERT [dbo].[Producto] ([ID], [IDCategoria], [Nombre], [Descripcion], [Precio], [Stock], [Imagen], [Activo]) VALUES (6, 3, N'Lápices de colores x36', N'Mina blanda de 3.3 mm, estuche metálico.', CAST(7400.00 AS Decimal(12, 2)), 15, NULL, 1)
INSERT [dbo].[Producto] ([ID], [IDCategoria], [Nombre], [Descripcion], [Precio], [Stock], [Imagen], [Activo]) VALUES (7, 4, N'Carpeta A4 con anillos', N'Tres anillos, tapa plástica resistente.', CAST(3600.00 AS Decimal(12, 2)), 20, NULL, 1)
INSERT [dbo].[Producto] ([ID], [IDCategoria], [Nombre], [Descripcion], [Precio], [Stock], [Imagen], [Activo]) VALUES (8, 5, N'Abrochadora metálica', N'Para broches 21/6, hasta 20 hojas.', CAST(5900.00 AS Decimal(12, 2)), 10, NULL, 1)
SET IDENTITY_INSERT [dbo].[Producto] OFF
GO
-- Administrador inicial: admin@libreria3d.com / Libreria2026 (cambiala desde Panel > Contraseña)
SET IDENTITY_INSERT [dbo].[Usuario] ON 

INSERT [dbo].[Usuario] ([ID], [Nombre], [Apellido], [Email], [Contraseña], [Es_Administrador]) VALUES (1, N'Admin', N'Librería', N'admin@libreria3d.com', N'PBKDF2$600000$SSskpAHoRxce/bu1EnSn4w==$zkndus/Wru/12EF5BVurbQ2yOkv7l2AzIQ8kcOQlsOw=', 1)
SET IDENTITY_INSERT [dbo].[Usuario] OFF
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
