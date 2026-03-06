
-- ==========================================================================================
-- Autor: Gemini CLI
-- Fecha Creación: 2026-03-06
-- Descripción: Script DDL para crear la tabla [dbo].[Cliente] con optimizaciones y
--              restricciones de integridad para SQL Server 2022 Express.
-- Base de Datos: dcmo_utm_dacb
-- Esquema: dbo
-- ==========================================================================================

USE dcmo_utm_dacb;
GO

SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

BEGIN TRY
    BEGIN TRANSACTION;

    -- ======================================================================================
    -- 1. Verificar la existencia de la tabla [dbo].[Cliente] y crearla si no existe.
    --    Esto garantiza la idempotencia del script.
    -- ======================================================================================
    IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Cliente' AND schema_id = SCHEMA_ID('dbo'))
    BEGIN
        PRINT 'Creando tabla [dbo].[Cliente]...';

        CREATE TABLE [dbo].[Cliente]
        (
            -- ClienteID: Clave primaria, tipo INT con auto-incremento (IDENTITY) para eficiencia.
            -- PRIMARY KEY CLUSTERED optimiza el almacenamiento y recuperación de filas.
            ClienteID         INT IDENTITY(1,1)     NOT NULL,
            
            -- Nombre: NVARCHAR(100) para soporte Unicode y longitud adecuada.
            Nombre            NVARCHAR(100)         NOT NULL,
            
            -- ApellidoPaterno: NVARCHAR(100) para soporte Unicode.
            ApellidoPaterno   NVARCHAR(100)         NOT NULL,
            
            -- ApellidoMaterno: NVARCHAR(100) NULL, ya que puede no existir.
            ApellidoMaterno   NVARCHAR(100)         NULL,
            
            -- Telefono: VARCHAR(15) para números telefónicos, puede ser NULL.
            Telefono          VARCHAR(15)           NULL,
            
            -- Email: NVARCHAR(150) para direcciones de correo electrónico, debe ser único.
            Email             NVARCHAR(150)         NOT NULL,
            
            -- Direccion: NVARCHAR(200) para direcciones postales, puede ser NULL.
            Direccion         NVARCHAR(200)         NULL,
            
            -- FechaRegistro: DATETIME con DEFAULT GETDATE() para registrar la fecha de creación.
            FechaRegistro     DATETIME              NOT NULL DEFAULT GETDATE(),
            
            -- Activo: BIT con DEFAULT 1 para indicar estado activo/inactivo.
            Activo            BIT                   NOT NULL DEFAULT 1,

            -- Restricción PRIMARY KEY CLUSTERED
            CONSTRAINT PK_Cliente_ClienteID PRIMARY KEY CLUSTERED (ClienteID)
        );

        PRINT 'Tabla [dbo].[Cliente] creada exitosamente.';

        -- ==================================================================================
        -- 2. Añadir Restricciones de Integridad (UNIQUE y CHECK)
        -- ==================================================================================

        -- Restricción UNIQUE para Email: Asegura que cada dirección de correo sea única.
        IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UQ_Cliente_Email' AND object_id = OBJECT_ID('[dbo].[Cliente]'))
        BEGIN
            ALTER TABLE [dbo].[Cliente]
            ADD CONSTRAINT UQ_Cliente_Email UNIQUE (Email);
            PRINT 'Restricción UQ_Cliente_Email añadida.';
        END;

        -- Restricción CHECK para Activo: Asegura que 'Activo' sea 0 o 1.
        IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CK_Cliente_Activo' AND parent_object_id = OBJECT_ID('[dbo].[Cliente]'))
        BEGIN
            ALTER TABLE [dbo].[Cliente]
            ADD CONSTRAINT CK_Cliente_Activo CHECK (Activo IN (0,1));
            PRINT 'Restricción CK_Cliente_Activo añadida.';
        END;

        -- Restricción CHECK para Formato de Email: Valida el formato básico del correo electrónico.
        IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CK_Cliente_Email_Formato' AND parent_object_id = OBJECT_ID('[dbo].[Cliente]'))
        BEGIN
            ALTER TABLE [dbo].[Cliente]
            ADD CONSTRAINT CK_Cliente_Email_Formato CHECK (Email LIKE '%[_]%@%[_]%.%[_]%');
            PRINT 'Restricción CK_Cliente_Email_Formato añadida.';
        END;
    END
    ELSE
    BEGIN
        PRINT 'La tabla [dbo].[Cliente] ya existe. No se realizaron cambios.';
    END;

    COMMIT TRANSACTION;
    PRINT 'Transacción completada y cambios aplicados.';

END TRY
BEGIN CATCH
    ROLLBACK TRANSACTION;
    PRINT ERROR_MESSAGE();
    PRINT 'Error en la transacción. Se realizó ROLLBACK.';
    THROW; -- Vuelve a lanzar el error después del ROLLBACK
END CATCH;
GO
