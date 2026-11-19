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
	[Direccion] [nvarchar](200) NULL,
	[Notas] [nvarchar](500) NULL,
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
ALTER TABLE [dbo].[Pedido]  WITH CHECK ADD  CONSTRAINT [CK_Pedido_Estado] CHECK  (([Estado]=N'Pendiente' OR [Estado]=N'Pagado' OR [Estado]=N'Enviado' OR [Estado]=N'Entregado' OR [Estado]=N'Cancelado'))
GO
ALTER TABLE [dbo].[Pedido] CHECK CONSTRAINT [CK_Pedido_Estado]
GO
ALTER TABLE [dbo].[PedidoItem]  WITH CHECK ADD  CONSTRAINT [CK_PedidoItem_Cantidad] CHECK  (([Cantidad]>(0)))
GO
ALTER TABLE [dbo].[PedidoItem] CHECK CONSTRAINT [CK_PedidoItem_Cantidad]
GO
