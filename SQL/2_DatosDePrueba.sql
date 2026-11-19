USE [Libreria3D]
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
