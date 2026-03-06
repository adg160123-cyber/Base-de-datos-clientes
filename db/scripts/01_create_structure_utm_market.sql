/*
================================================================================
Arquitectura: Estructura UTM Market (Producto, Venta, Detalle)
Motor: Microsoft SQL Server 2022 Express
Base de Datos: dcmo_utm_dacb
Objetivo: Definir estructura de datos con alta integridad y eficiencia.
================================================================================
*/

USE [dcmo_utm_dacb];
GO

SET NOCOUNT ON;
SET XACT_ABORT ON;

-- =============================================
-- 1. TABLA: Producto
-- Almacena el catálogo de productos con control de SKU único y stock.
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Producto]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Producto] (
        [ProductoID] INT IDENTITY(1,1) NOT NULL,
        [Nombre]     NVARCHAR(100) NOT NULL,
        [SKU]        VARCHAR(20)   NOT NULL,
        [Marca]      NVARCHAR(50)  NULL,
        [Precio]     DECIMAL(19,4) NOT NULL DEFAULT 0.0000,
        [Stock]      INT           NOT NULL DEFAULT 0,
        
        CONSTRAINT [PK_Producto] PRIMARY KEY CLUSTERED ([ProductoID] ASC),
        CONSTRAINT [UQ_Producto_SKU] UNIQUE ([SKU]),
        CONSTRAINT [CK_Producto_Precio] CHECK ([Precio] >= 0),
        CONSTRAINT [CK_Producto_Stock]  CHECK ([Stock] >= 0)
    );
    PRINT 'Tabla [Producto] creada exitosamente.';
END
ELSE
    PRINT 'La tabla [Producto] ya existe.';
GO

-- =============================================
-- 2. TABLA: Venta
-- Cabecera de la transacción comercial.
-- Estatus: 1=Pendiente, 2=Completada, 3=Cancelada
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Venta]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Venta] (
        [VentaID]        INT IDENTITY(1,1) NOT NULL,
        [Folio]          VARCHAR(20)   NOT NULL,
        [FechaVenta]     DATETIME      NOT NULL DEFAULT GETDATE(),
        [TotalArticulos] INT           NOT NULL DEFAULT 0,
        [TotalVenta]     DECIMAL(19,4) NOT NULL DEFAULT 0.0000,
        [Estatus]        TINYINT       NOT NULL DEFAULT 1, -- 1: Pendiente, 2: Completada, 3: Cancelada
        
        CONSTRAINT [PK_Venta] PRIMARY KEY CLUSTERED ([VentaID] ASC),
        CONSTRAINT [UQ_Venta_Folio] UNIQUE ([Folio]),
        CONSTRAINT [CK_Venta_Estatus] CHECK ([Estatus] IN (1, 2, 3)),
        CONSTRAINT [CK_Venta_Totales] CHECK ([TotalArticulos] >= 0 AND [TotalVenta] >= 0)
    );
    PRINT 'Tabla [Venta] creada exitosamente.';
END
ELSE
    PRINT 'La tabla [Venta] ya existe.';
GO

-- =============================================
-- 3. TABLA: DetalleVenta
-- Relación 1:N entre Venta y Producto.
-- Mantiene la trazabilidad de precios históricos al momento de la venta.
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DetalleVenta]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[DetalleVenta] (
        [DetalleID]      INT IDENTITY(1,1) NOT NULL,
        [VentaID]        INT NOT NULL,
        [ProductoID]     INT NOT NULL,
        [PrecioUnitario] DECIMAL(19,4) NOT NULL,
        [Cantidad]       INT NOT NULL,
        [TotalDetalle]   DECIMAL(19,4) NOT NULL,
        
        CONSTRAINT [PK_DetalleVenta] PRIMARY KEY CLUSTERED ([DetalleID] ASC),
        CONSTRAINT [FK_DetalleVenta_Venta] FOREIGN KEY ([VentaID]) 
            REFERENCES [dbo].[Venta] ([VentaID]),
        CONSTRAINT [FK_DetalleVenta_Producto] FOREIGN KEY ([ProductoID]) 
            REFERENCES [dbo].[Producto] ([ProductoID]),
        CONSTRAINT [CK_DetalleVenta_Valores] CHECK ([PrecioUnitario] >= 0 AND [Cantidad] > 0 AND [TotalDetalle] >= 0)
    );
    PRINT 'Tabla [DetalleVenta] creada exitosamente.';
END
ELSE
    PRINT 'La tabla [DetalleVenta] ya existe.';
GO
