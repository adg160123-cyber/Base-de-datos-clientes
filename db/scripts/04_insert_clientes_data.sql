
-- ==========================================================================================
-- Autor: Gemini CLI
-- Fecha Creación: 2026-03-06
-- Descripción: Script DML para insertar 20 clientes en la tabla [dbo].[Cliente]
--              en la base de datos 'dcmo_utm_dacb', garantizando idempotencia
--              y manejo de errores.
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

    PRINT 'Iniciando inserción de datos para la tabla [dbo].[Cliente]...';

    -- Cliente 1
    IF NOT EXISTS (SELECT 1 FROM dbo.Cliente WHERE LOWER(Email) = LOWER('maria.gonzalez@example.com'))
       AND 'maria.gonzalez@example.com' LIKE '%[_]%@%[_]%.%[_]%'
    BEGIN
        INSERT INTO [dbo].[Cliente] (Nombre, ApellidoPaterno, ApellidoMaterno, Telefono, Email, Direccion)
        VALUES (N'María', N'González', N'Pérez', '5512345678', 'maria.gonzalez@example.com', N'Calle Falsa 123, Col. Centro, CDMX');
        PRINT 'Cliente María González Pérez insertado.';
    END;

    -- Cliente 2
    IF NOT EXISTS (SELECT 1 FROM dbo.Cliente WHERE LOWER(Email) = LOWER('juan.rodriguez@example.com'))
       AND 'juan.rodriguez@example.com' LIKE '%[_]%@%[_]%.%[_]%'
    BEGIN
        INSERT INTO [dbo].[Cliente] (Nombre, ApellidoPaterno, ApellidoMaterno, Telefono, Email, Direccion)
        VALUES (N'Juan', N'Rodríguez', N'Sánchez', '5587654321', 'juan.rodriguez@example.com', N'Av. Siempre Viva 742, Col. Del Valle, Monterrey');
        PRINT 'Cliente Juan Rodríguez Sánchez insertado.';
    END;

    -- Cliente 3
    IF NOT EXISTS (SELECT 1 FROM dbo.Cliente WHERE LOWER(Email) = LOWER('ana.lopez@example.com'))
       AND 'ana.lopez@example.com' LIKE '%[_]%@%[_]%.%[_]%'
    BEGIN
        INSERT INTO [dbo].[Cliente] (Nombre, ApellidoPaterno, ApellidoMaterno, Telefono, Email, Direccion)
        VALUES (N'Ana', N'López', N'Hernández', '3322110099', 'ana.lopez@example.com', N'Blvd. de los Sueños 45, Col. Americana, Guadalajara');
        PRINT 'Cliente Ana López Hernández insertado.';
    END;

    -- Cliente 4
    IF NOT EXISTS (SELECT 1 FROM dbo.Cliente WHERE LOWER(Email) = LOWER('pedro.martinez@example.com'))
       AND 'pedro.martinez@example.com' LIKE '%[_]%@%[_]%.%[_]%'
    BEGIN
        INSERT INTO [dbo].[Cliente] (Nombre, ApellidoPaterno, ApellidoMaterno, Telefono, Email, Direccion)
        VALUES (N'Pedro', N'Martínez', N'García', '8111223344', 'pedro.martinez@example.com', N'Paseo de la Reforma 100, Col. Juárez, CDMX');
        PRINT 'Cliente Pedro Martínez García insertado.';
    END;

    -- Cliente 5
    IF NOT EXISTS (SELECT 1 FROM dbo.Cliente WHERE LOWER(Email) = LOWER('sofia.fernandez@example.com'))
       AND 'sofia.fernandez@example.com' LIKE '%[_]%@%[_]%.%[_]%'
    BEGIN
        INSERT INTO [dbo].[Cliente] (Nombre, ApellidoPaterno, ApellidoMaterno, Telefono, Email, Direccion)
        VALUES (N'Sofía', N'Fernández', N'Díaz', '2225556677', 'sofia.fernandez@example.com', N'Av. Insurgentes Sur 200, Col. Roma, Puebla');
        PRINT 'Cliente Sofía Fernández Díaz insertado.';
    END;

    -- Cliente 6
    IF NOT EXISTS (SELECT 1 FROM dbo.Cliente WHERE LOWER(Email) = LOWER('carlos.gomez@example.com'))
       AND 'carlos.gomez@example.com' LIKE '%[_]%@%[_]%.%[_]%'
    BEGIN
        INSERT INTO [dbo].[Cliente] (Nombre, ApellidoPaterno, ApellidoMaterno, Telefono, Email, Direccion)
        VALUES (N'Carlos', N'Gómez', N'Torres', '6641234567', 'carlos.gomez@example.com', N'Via Rápida Ote. 30, Zona Urbana Rio, Tijuana');
        PRINT 'Cliente Carlos Gómez Torres insertado.';
    END;

    -- Cliente 7
    IF NOT EXISTS (SELECT 1 FROM dbo.Cliente WHERE LOWER(Email) = LOWER('laura.hernandez@example.com'))
       AND 'laura.hernandez@example.com' LIKE '%[_]%@%[_]%.%[_]%'
    BEGIN
        INSERT INTO [dbo].[Cliente] (Nombre, ApellidoPaterno, ApellidoMaterno, Telefono, Email, Direccion)
        VALUES (N'Laura', N'Hernández', N'Flores', '9987654321', 'laura.hernandez@example.com', N'Av. Tulum 15, Supermanzana 1, Cancún');
        PRINT 'Cliente Laura Hernández Flores insertado.';
    END;

    -- Cliente 8
    IF NOT EXISTS (SELECT 1 FROM dbo.Cliente WHERE LOWER(Email) = LOWER('ricardo.perez@example.com'))
       AND 'ricardo.perez@example.com' LIKE '%[_]%@%[_]%.%[_]%'
    BEGIN
        INSERT INTO [dbo].[Cliente] (Nombre, ApellidoPaterno, ApellidoMaterno, Telefono, Email, Direccion)
        VALUES (N'Ricardo', N'Pérez', N'Ramírez', '4423456789', 'ricardo.perez@example.com', N'Bernardo Quintana 50, Col. Centro, Querétaro');
        PRINT 'Cliente Ricardo Pérez Ramírez insertado.';
    END;

    -- Cliente 9
    IF NOT EXISTS (SELECT 1 FROM dbo.Cliente WHERE LOWER(Email) = LOWER('valeria.sanchez@example.com'))
       AND 'valeria.sanchez@example.com' LIKE '%[_]%@%[_]%.%[_]%'
    BEGIN
        INSERT INTO [dbo].[Cliente] (Nombre, ApellidoPaterno, ApellidoMaterno, Telefono, Email, Direccion)
        VALUES (N'Valeria', N'Sánchez', N'Vázquez', '7771122334', 'valeria.sanchez@example.com', N'Río Mayo 10, Col. Vista Hermosa, Cuernavaca');
        PRINT 'Cliente Valeria Sánchez Vázquez insertado.';
    END;

    -- Cliente 10
    IF NOT EXISTS (SELECT 1 FROM dbo.Cliente WHERE LOWER(Email) = LOWER('arturo.ramirez@example.com'))
       AND 'arturo.ramirez@example.com' LIKE '%[_]%@%[_]%.%[_]%'
    BEGIN
        INSERT INTO [dbo].[Cliente] (Nombre, ApellidoPaterno, ApellidoMaterno, Telefono, Email, Direccion)
        VALUES (N'Arturo', N'Ramírez', N'Morales', '9618877665', 'arturo.ramirez@example.com', N'1a. Ote. Nte. 9, Centro, Tuxtla Gutiérrez');
        PRINT 'Cliente Arturo Ramírez Morales insertado.';
    END;

    -- Cliente 11
    IF NOT EXISTS (SELECT 1 FROM dbo.Cliente WHERE LOWER(Email) = LOWER('gabriela.castro@example.com'))
       AND 'gabriela.castro@example.com' LIKE '%[_]%@%[_]%.%[_]%'
    BEGIN
        INSERT INTO [dbo].[Cliente] (Nombre, ApellidoPaterno, ApellidoMaterno, Telefono, Email, Direccion)
        VALUES (N'Gabriela', N'Castro', N'Ruiz', '6143322110', 'gabriela.castro@example.com', N'Av. Universidad 500, Col. San Felipe, Chihuahua');
        PRINT 'Cliente Gabriela Castro Ruiz insertado.';
    END;

    -- Cliente 12
    IF NOT EXISTS (SELECT 1 FROM dbo.Cliente WHERE LOWER(Email) = LOWER('daniel.jimenez@example.com'))
       AND 'daniel.jimenez@example.com' LIKE '%[_]%@%[_]%.%[_]%'
    BEGIN
        INSERT INTO [dbo].[Cliente] (Nombre, ApellidoPaterno, ApellidoMaterno, Telefono, Email, Direccion)
        VALUES (N'Daniel', N'Jiménez', N'Herrera', '6564433221', 'daniel.jimenez@example.com', N'Av. Juárez 100, Zona Centro, Ciudad Juárez');
        PRINT 'Cliente Daniel Jiménez Herrera insertado.';
    END;

    -- Cliente 13
    IF NOT EXISTS (SELECT 1 FROM dbo.Cliente WHERE LOWER(Email) = LOWER('paola.garcia@example.com'))
       AND 'paola.garcia@example.com' LIKE '%[_]%@%[_]%.%[_]%'
    BEGIN
        INSERT INTO [dbo].[Cliente] (Nombre, ApellidoPaterno, ApellidoMaterno, Telefono, Email, Direccion)
        VALUES (N'Paola', N'García', N'Castillo', '8712233445', 'paola.garcia@example.com', N'Paseo de la Rosita 20, Col. Campestre, Torreón');
        PRINT 'Cliente Paola García Castillo insertado.';
    END;

    -- Cliente 14
    IF NOT EXISTS (SELECT 1 FROM dbo.Cliente WHERE LOWER(Email) = LOWER('sergio.vargas@example.com'))
       AND 'sergio.vargas@example.com' LIKE '%[_]%@%[_]%.%[_]%'
    BEGIN
        INSERT INTO [dbo].[Cliente] (Nombre, ApellidoPaterno, ApellidoMaterno, Telefono, Email, Direccion)
        VALUES (N'Sergio', N'Vargas', N'Reyes', '6861122334', 'sergio.vargas@example.com', N'Av. Reforma 500, Col. Nueva, Mexicali');
        PRINT 'Cliente Sergio Vargas Reyes insertado.';
    END;

    -- Cliente 15
    IF NOT EXISTS (SELECT 1 FROM dbo.Cliente WHERE LOWER(Email) = LOWER('andre.mendoza@example.com'))
       AND 'andre.mendoza@example.com' LIKE '%[_]%@%[_]%.%[_]%'
    BEGIN
        INSERT INTO [dbo].[Cliente] (Nombre, ApellidoPaterno, ApellidoMaterno, Telefono, Email, Direccion)
        VALUES (N'Andrea', N'Mendoza', N'Guerrero', '3310099887', 'andre.mendoza@example.com', N'Pablo Neruda 200, Col. Providencia, Guadalajara');
        PRINT 'Cliente Andrea Mendoza Guerrero insertado.';
    END;

    -- Cliente 16
    IF NOT EXISTS (SELECT 1 FROM dbo.Cliente WHERE LOWER(Email) = LOWER('felipe.cruz@example.com'))
       AND 'felipe.cruz@example.com' LIKE '%[_]%@%[_]%.%[_]%'
    BEGIN
        INSERT INTO [dbo].[Cliente] (Nombre, ApellidoPaterno, ApellidoMaterno, Telefono, Email, Direccion)
        VALUES (N'Felipe', N'Cruz', N'Ortiz', '5598761234', 'felipe.cruz@example.com', N'Poniente 146 #10, Col. Industrial Vallejo, CDMX');
        PRINT 'Cliente Felipe Cruz Ortiz insertado.';
    END;

    -- Cliente 17
    IF NOT EXISTS (SELECT 1 FROM dbo.Cliente WHERE LOWER(Email) = LOWER('natalia.diaz@example.com'))
       AND 'natalia.diaz@example.com' LIKE '%[_]%@%[_]%.%[_]%'
    BEGIN
        INSERT INTO [dbo].[Cliente] (Nombre, ApellidoPaterno, ApellidoMaterno, Telefono, Email, Direccion)
        VALUES (N'Natalia', N'Díaz', N'Castillo', '8123456789', 'natalia.diaz@example.com', N'Gómez Morín 333, Col. Del Valle, San Pedro Garza García');
        PRINT 'Cliente Natalia Díaz Castillo insertado.';
    END;

    -- Cliente 18
    IF NOT EXISTS (SELECT 1 FROM dbo.Cliente WHERE LOWER(Email) = LOWER('javier.hernandez@example.com'))
       AND 'javier.hernandez@example.com' LIKE '%[_]%@%[_]%.%[_]%'
    BEGIN
        INSERT INTO [dbo].[Cliente] (Nombre, ApellidoPaterno, ApellidoMaterno, Telefono, Email, Direccion)
        VALUES (N'Javier', N'Hernández', N'Romero', '4445566778', 'javier.hernandez@example.com', N'Av. Chapultepec 1000, Col. Centro, San Luis Potosí');
        PRINT 'Cliente Javier Hernández Romero insertado.';
    END;

    -- Cliente 19
    IF NOT EXISTS (SELECT 1 FROM dbo.Cliente WHERE LOWER(Email) = LOWER('renata.lopez@example.com'))
       AND 'renata.lopez@example.com' LIKE '%[_]%@%[_]%.%[_]%'
    BEGIN
        INSERT INTO [dbo].[Cliente] (Nombre, ApellidoPaterno, ApellidoMaterno, Telefono, Email, Direccion)
        VALUES (N'Renata', N'López', N'Jiménez', '7223344556', 'renata.lopez@example.com', N'Independencia 1, Col. Centro, Toluca');
        PRINT 'Cliente Renata López Jiménez insertado.';
    END;

    -- Cliente 20
    IF NOT EXISTS (SELECT 1 FROM dbo.Cliente WHERE LOWER(Email) = LOWER('memo.sanchez@example.com'))
       AND 'memo.sanchez@example.com' LIKE '%[_]%@%[_]%.%[_]%'
    BEGIN
        INSERT INTO [dbo].[Cliente] (Nombre, ApellidoPaterno, ApellidoMaterno, Telefono, Email, Direccion)
        VALUES (N'Guillermo', N'Sánchez', N'Estrada', '6621122334', 'memo.sanchez@example.com', N'Blvd. Kino 123, Col. Pitic, Hermosillo');
        PRINT 'Cliente Guillermo Sánchez Estrada insertado.';
    END;

    COMMIT TRANSACTION;
    PRINT 'Inserción de 20 clientes completada exitosamente.';

END TRY
BEGIN CATCH
    ROLLBACK TRANSACTION;
    DECLARE @ErrorMessage NVARCHAR(MAX) = ERROR_MESSAGE();
    PRINT 'Error en la transacción. Se realizó ROLLBACK.';
    PRINT @ErrorMessage;
    THROW; -- Vuelve a lanzar el error después del ROLLBACK
END CATCH;
GO
