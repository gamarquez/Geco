-- =============================================
-- STORED PROCEDURES PARA PLANES
-- =============================================
-- Este script crea los stored procedures necesarios
-- para el manejo de planes de obras sociales
-- =============================================

USE GECO;
GO

PRINT '========================================'
PRINT 'CREANDO STORED PROCEDURES PARA PLANES'
PRINT '========================================'
GO

-- =============================================
-- SP_ListarPlanesPorObraSocial
-- =============================================
PRINT 'Creando SP_ListarPlanesPorObraSocial...'
GO

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'SP_ListarPlanesPorObraSocial')
    DROP PROCEDURE SP_ListarPlanesPorObraSocial;
GO

CREATE PROCEDURE SP_ListarPlanesPorObraSocial
    @ObraSocialId INT,
    @SoloActivos BIT = 1
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        p.PlanId,
        p.ObraSocialId,
        p.Nombre,
        p.Codigo,
        p.Descripcion,
        p.PorcentajeCobertura,
        p.Copago,
        p.Activo,
        p.FechaAlta,
        p.Observaciones,
        os.Nombre AS ObraSocialNombre
    FROM Planes p
    INNER JOIN ObrasSociales os ON p.ObraSocialId = os.ObraSocialId
    WHERE p.ObraSocialId = @ObraSocialId
      AND (@SoloActivos = 0 OR p.Activo = 1)
    ORDER BY p.Nombre;
END
GO

PRINT 'SP_ListarPlanesPorObraSocial creado exitosamente.'
GO

-- =============================================
-- SP_ListarPlanes
-- =============================================
PRINT 'Creando SP_ListarPlanes...'
GO

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'SP_ListarPlanes')
    DROP PROCEDURE SP_ListarPlanes;
GO

CREATE PROCEDURE SP_ListarPlanes
    @SoloActivos BIT = 1
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        p.PlanId,
        p.ObraSocialId,
        p.Nombre,
        p.Codigo,
        p.Descripcion,
        p.PorcentajeCobertura,
        p.Copago,
        p.Activo,
        p.FechaAlta,
        p.Observaciones,
        os.Nombre AS ObraSocialNombre
    FROM Planes p
    INNER JOIN ObrasSociales os ON p.ObraSocialId = os.ObraSocialId
    WHERE (@SoloActivos = 0 OR p.Activo = 1)
    ORDER BY os.Nombre, p.Nombre;
END
GO

PRINT 'SP_ListarPlanes creado exitosamente.'
GO

-- =============================================
-- SP_ObtenerPlanPorId
-- =============================================
PRINT 'Creando SP_ObtenerPlanPorId...'
GO

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'SP_ObtenerPlanPorId')
    DROP PROCEDURE SP_ObtenerPlanPorId;
GO

CREATE PROCEDURE SP_ObtenerPlanPorId
    @PlanId INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        p.PlanId,
        p.ObraSocialId,
        p.Nombre,
        p.Codigo,
        p.Descripcion,
        p.PorcentajeCobertura,
        p.Copago,
        p.Activo,
        p.FechaAlta,
        p.Observaciones,
        os.Nombre AS ObraSocialNombre
    FROM Planes p
    INNER JOIN ObrasSociales os ON p.ObraSocialId = os.ObraSocialId
    WHERE p.PlanId = @PlanId;
END
GO

PRINT 'SP_ObtenerPlanPorId creado exitosamente.'
GO

-- =============================================
-- SP_CrearPlan
-- =============================================
PRINT 'Creando SP_CrearPlan...'
GO

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'SP_CrearPlan')
    DROP PROCEDURE SP_CrearPlan;
GO

CREATE PROCEDURE SP_CrearPlan
    @ObraSocialId INT,
    @Nombre NVARCHAR(100),
    @Codigo NVARCHAR(50) = NULL,
    @Descripcion NVARCHAR(500) = NULL,
    @PorcentajeCobertura DECIMAL(5,2) = NULL,
    @Copago DECIMAL(10,2) = NULL,
    @Observaciones NVARCHAR(MAX) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO Planes (
        ObraSocialId, Nombre, Codigo, Descripcion,
        PorcentajeCobertura, Copago, Observaciones,
        Activo, FechaAlta
    )
    VALUES (
        @ObraSocialId, @Nombre, @Codigo, @Descripcion,
        @PorcentajeCobertura, @Copago, @Observaciones,
        1, GETDATE()
    );

    SELECT SCOPE_IDENTITY() AS PlanId;
END
GO

PRINT 'SP_CrearPlan creado exitosamente.'
GO

-- =============================================
-- SP_ActualizarPlan
-- =============================================
PRINT 'Creando SP_ActualizarPlan...'
GO

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'SP_ActualizarPlan')
    DROP PROCEDURE SP_ActualizarPlan;
GO

CREATE PROCEDURE SP_ActualizarPlan
    @PlanId INT,
    @ObraSocialId INT,
    @Nombre NVARCHAR(100),
    @Codigo NVARCHAR(50) = NULL,
    @Descripcion NVARCHAR(500) = NULL,
    @PorcentajeCobertura DECIMAL(5,2) = NULL,
    @Copago DECIMAL(10,2) = NULL,
    @Observaciones NVARCHAR(MAX) = NULL,
    @Activo BIT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE Planes
    SET ObraSocialId = @ObraSocialId,
        Nombre = @Nombre,
        Codigo = @Codigo,
        Descripcion = @Descripcion,
        PorcentajeCobertura = @PorcentajeCobertura,
        Copago = @Copago,
        Observaciones = @Observaciones,
        Activo = @Activo
    WHERE PlanId = @PlanId;

    SELECT @@ROWCOUNT AS FilasAfectadas;
END
GO

PRINT 'SP_ActualizarPlan creado exitosamente.'
GO

-- =============================================
-- SP_EliminarPlan
-- =============================================
PRINT 'Creando SP_EliminarPlan...'
GO

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'SP_EliminarPlan')
    DROP PROCEDURE SP_EliminarPlan;
GO

CREATE PROCEDURE SP_EliminarPlan
    @PlanId INT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE Planes
    SET Activo = 0
    WHERE PlanId = @PlanId;

    SELECT @@ROWCOUNT AS FilasAfectadas;
END
GO

PRINT 'SP_EliminarPlan creado exitosamente.'
GO

PRINT '========================================'
PRINT 'STORED PROCEDURES CREADOS EXITOSAMENTE'
PRINT '========================================'
PRINT 'Total: 6 stored procedures para Planes'
GO
