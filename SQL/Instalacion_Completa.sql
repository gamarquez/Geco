-- =============================================
-- SCRIPT DE INSTALACIÓN COMPLETA - GECO
-- =============================================
-- Este script crea la tabla DisponibilidadesAgenda
-- y todos los stored procedures necesarios
-- =============================================
-- IMPORTANTE: Ejecutar este script en orden
-- =============================================

USE GECO;
GO

PRINT '========================================';
PRINT 'INICIANDO INSTALACIÓN - GECO';
PRINT '========================================';
GO

-- =============================================
-- PASO 1: CREAR TABLA DisponibilidadesAgenda
-- =============================================

PRINT 'Paso 1: Creando tabla DisponibilidadesAgenda...';
GO

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DisponibilidadesAgenda]') AND type in (N'U'))
BEGIN
    PRINT 'La tabla DisponibilidadesAgenda ya existe. Omitiendo creación.';
END
ELSE
BEGIN
    CREATE TABLE [dbo].[DisponibilidadesAgenda] (
        [DisponibilidadAgendaId] INT IDENTITY(1,1) NOT NULL,
        [ProfesionalId] INT NOT NULL,
        [DiaSemana] INT NOT NULL, -- 1=Lunes, 2=Martes, ..., 7=Domingo
        [HoraInicio] TIME NOT NULL,
        [HoraFin] TIME NOT NULL,
        [IntervaloMinutos] INT NOT NULL,
        [FechaVigenciaDesde] DATE NOT NULL,
        [FechaVigenciaHasta] DATE NULL,
        [Activo] BIT NOT NULL DEFAULT 1,
        [FechaAlta] DATETIME NOT NULL DEFAULT GETDATE(),
        [FechaModificacion] DATETIME NULL,

        CONSTRAINT [PK_DisponibilidadesAgenda] PRIMARY KEY CLUSTERED ([DisponibilidadAgendaId] ASC),
        CONSTRAINT [FK_DisponibilidadesAgenda_Profesionales] FOREIGN KEY ([ProfesionalId])
            REFERENCES [dbo].[Profesionales] ([ProfesionalId]),
        CONSTRAINT [CK_DiaSemana] CHECK ([DiaSemana] >= 1 AND [DiaSemana] <= 7),
        CONSTRAINT [CK_HoraInicioFin] CHECK ([HoraInicio] < [HoraFin]),
        CONSTRAINT [CK_IntervaloMinutos] CHECK ([IntervaloMinutos] > 0)
    );

    CREATE NONCLUSTERED INDEX [IX_DisponibilidadesAgenda_ProfesionalId]
        ON [dbo].[DisponibilidadesAgenda] ([ProfesionalId]);

    CREATE NONCLUSTERED INDEX [IX_DisponibilidadesAgenda_DiaSemana]
        ON [dbo].[DisponibilidadesAgenda] ([DiaSemana]);

    CREATE NONCLUSTERED INDEX [IX_DisponibilidadesAgenda_Vigencia]
        ON [dbo].[DisponibilidadesAgenda] ([FechaVigenciaDesde], [FechaVigenciaHasta]);

    PRINT 'Tabla DisponibilidadesAgenda creada exitosamente.';
END
GO

-- =============================================
-- PASO 2: STORED PROCEDURES PARA PACIENTES
-- =============================================

PRINT 'Paso 2: Creando stored procedures para Pacientes...';
GO

-- SP_ObtenerPacientePorDocumento
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'SP_ObtenerPacientePorDocumento')
    DROP PROCEDURE SP_ObtenerPacientePorDocumento;
GO

CREATE PROCEDURE SP_ObtenerPacientePorDocumento
    @TipoDocumento NVARCHAR(20),
    @NumeroDocumento NVARCHAR(20)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        p.PacienteId,
        p.Nombre,
        p.Apellido,
        p.TipoDocumento,
        p.NumeroDocumento,
        p.FechaNacimiento,
        p.Sexo,
        p.Telefono,
        p.TelefonoAlternativo,
        p.Email,
        p.Direccion,
        p.Localidad,
        p.Provincia,
        p.CodigoPostal,
        p.ObraSocialId,
        p.PlanId,
        p.NumeroAfiliado,
        p.Observaciones,
        p.Activo,
        p.FechaAlta,
        p.FechaModificacion,
        os.Nombre AS ObraSocialNombre,
        os.CUIT AS ObraSocialCUIT,
        pl.Nombre AS PlanNombre
    FROM Pacientes p
    LEFT JOIN ObrasSociales os ON p.ObraSocialId = os.ObraSocialId
    LEFT JOIN Planes pl ON p.PlanId = pl.PlanId
    WHERE p.TipoDocumento = @TipoDocumento
      AND p.NumeroDocumento = @NumeroDocumento;
END
GO

PRINT 'SP_ObtenerPacientePorDocumento creado.';
GO

-- =============================================
-- PASO 3: STORED PROCEDURES PARA DISPONIBILIDADES
-- =============================================

PRINT 'Paso 3: Creando stored procedures para Disponibilidades...';
GO

-- SP_CrearDisponibilidadAgenda
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'SP_CrearDisponibilidadAgenda')
    DROP PROCEDURE SP_CrearDisponibilidadAgenda;
GO

CREATE PROCEDURE SP_CrearDisponibilidadAgenda
    @ProfesionalId INT,
    @DiaSemana INT,
    @HoraInicio TIME,
    @HoraFin TIME,
    @IntervaloMinutos INT,
    @FechaVigenciaDesde DATE,
    @FechaVigenciaHasta DATE = NULL
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO DisponibilidadesAgenda (
        ProfesionalId, DiaSemana, HoraInicio, HoraFin, IntervaloMinutos,
        FechaVigenciaDesde, FechaVigenciaHasta, Activo, FechaAlta
    )
    VALUES (
        @ProfesionalId, @DiaSemana, @HoraInicio, @HoraFin, @IntervaloMinutos,
        @FechaVigenciaDesde, @FechaVigenciaHasta, 1, GETDATE()
    );

    SELECT SCOPE_IDENTITY() AS DisponibilidadAgendaId;
END
GO

-- SP_ActualizarDisponibilidadAgenda
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'SP_ActualizarDisponibilidadAgenda')
    DROP PROCEDURE SP_ActualizarDisponibilidadAgenda;
GO

CREATE PROCEDURE SP_ActualizarDisponibilidadAgenda
    @DisponibilidadAgendaId INT,
    @ProfesionalId INT,
    @DiaSemana INT,
    @HoraInicio TIME,
    @HoraFin TIME,
    @IntervaloMinutos INT,
    @FechaVigenciaDesde DATE,
    @FechaVigenciaHasta DATE = NULL
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE DisponibilidadesAgenda
    SET ProfesionalId = @ProfesionalId,
        DiaSemana = @DiaSemana,
        HoraInicio = @HoraInicio,
        HoraFin = @HoraFin,
        IntervaloMinutos = @IntervaloMinutos,
        FechaVigenciaDesde = @FechaVigenciaDesde,
        FechaVigenciaHasta = @FechaVigenciaHasta,
        FechaModificacion = GETDATE()
    WHERE DisponibilidadAgendaId = @DisponibilidadAgendaId;

    SELECT @@ROWCOUNT AS FilasAfectadas;
END
GO

-- SP_EliminarDisponibilidadAgenda
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'SP_EliminarDisponibilidadAgenda')
    DROP PROCEDURE SP_EliminarDisponibilidadAgenda;
GO

CREATE PROCEDURE SP_EliminarDisponibilidadAgenda
    @DisponibilidadAgendaId INT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE DisponibilidadesAgenda
    SET Activo = 0, FechaModificacion = GETDATE()
    WHERE DisponibilidadAgendaId = @DisponibilidadAgendaId;

    SELECT @@ROWCOUNT AS FilasAfectadas;
END
GO

-- SP_ListarDisponibilidadesAgenda
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'SP_ListarDisponibilidadesAgenda')
    DROP PROCEDURE SP_ListarDisponibilidadesAgenda;
GO

CREATE PROCEDURE SP_ListarDisponibilidadesAgenda
    @ProfesionalId INT = NULL,
    @DiaSemana INT = NULL,
    @SoloVigentes BIT = NULL,
    @PageNumber INT = 1,
    @PageSize INT = 20,
    @SoloActivas BIT = 1
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;
    DECLARE @TotalRegistros INT;

    ;WITH DisponibilidadesFiltradas AS (
        SELECT
            da.DisponibilidadAgendaId, da.ProfesionalId, da.DiaSemana,
            da.HoraInicio, da.HoraFin, da.IntervaloMinutos,
            da.FechaVigenciaDesde, da.FechaVigenciaHasta,
            da.Activo, da.FechaAlta, da.FechaModificacion,
            p.Nombre AS ProfesionalNombre,
            p.Apellido AS ProfesionalApellido,
            p.Matricula AS ProfesionalMatricula,
            p.Especialidad AS ProfesionalEspecialidad
        FROM DisponibilidadesAgenda da
        INNER JOIN Profesionales p ON da.ProfesionalId = p.ProfesionalId
        WHERE (@SoloActivas = 0 OR da.Activo = 1)
          AND (@ProfesionalId IS NULL OR da.ProfesionalId = @ProfesionalId)
          AND (@DiaSemana IS NULL OR da.DiaSemana = @DiaSemana)
          AND (@SoloVigentes IS NULL OR
               (@SoloVigentes = 1 AND GETDATE() >= da.FechaVigenciaDesde AND
                (da.FechaVigenciaHasta IS NULL OR GETDATE() <= da.FechaVigenciaHasta)))
    )
    SELECT * FROM DisponibilidadesFiltradas
    ORDER BY DiaSemana, HoraInicio
    OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

    SELECT @TotalRegistros = COUNT(*)
    FROM DisponibilidadesAgenda da
    WHERE (@SoloActivas = 0 OR da.Activo = 1)
      AND (@ProfesionalId IS NULL OR da.ProfesionalId = @ProfesionalId)
      AND (@DiaSemana IS NULL OR da.DiaSemana = @DiaSemana)
      AND (@SoloVigentes IS NULL OR
           (@SoloVigentes = 1 AND GETDATE() >= da.FechaVigenciaDesde AND
            (da.FechaVigenciaHasta IS NULL OR GETDATE() <= da.FechaVigenciaHasta)));

    SELECT @TotalRegistros AS TotalRegistros;
END
GO

-- SP_ObtenerDisponibilidadPorId
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'SP_ObtenerDisponibilidadPorId')
    DROP PROCEDURE SP_ObtenerDisponibilidadPorId;
GO

CREATE PROCEDURE SP_ObtenerDisponibilidadPorId
    @DisponibilidadAgendaId INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        da.DisponibilidadAgendaId, da.ProfesionalId, da.DiaSemana,
        da.HoraInicio, da.HoraFin, da.IntervaloMinutos,
        da.FechaVigenciaDesde, da.FechaVigenciaHasta,
        da.Activo, da.FechaAlta, da.FechaModificacion,
        p.Nombre AS ProfesionalNombre,
        p.Apellido AS ProfesionalApellido,
        p.Matricula AS ProfesionalMatricula,
        p.Especialidad AS ProfesionalEspecialidad
    FROM DisponibilidadesAgenda da
    INNER JOIN Profesionales p ON da.ProfesionalId = p.ProfesionalId
    WHERE da.DisponibilidadAgendaId = @DisponibilidadAgendaId;
END
GO

-- SP_ObtenerDisponibilidadesPorProfesional
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'SP_ObtenerDisponibilidadesPorProfesional')
    DROP PROCEDURE SP_ObtenerDisponibilidadesPorProfesional;
GO

CREATE PROCEDURE SP_ObtenerDisponibilidadesPorProfesional
    @ProfesionalId INT,
    @SoloVigentes BIT = 1
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        da.DisponibilidadAgendaId, da.ProfesionalId, da.DiaSemana,
        da.HoraInicio, da.HoraFin, da.IntervaloMinutos,
        da.FechaVigenciaDesde, da.FechaVigenciaHasta,
        da.Activo, da.FechaAlta, da.FechaModificacion,
        p.Nombre AS ProfesionalNombre,
        p.Apellido AS ProfesionalApellido,
        p.Matricula AS ProfesionalMatricula,
        p.Especialidad AS ProfesionalEspecialidad
    FROM DisponibilidadesAgenda da
    INNER JOIN Profesionales p ON da.ProfesionalId = p.ProfesionalId
    WHERE da.ProfesionalId = @ProfesionalId
      AND da.Activo = 1
      AND (@SoloVigentes = 0 OR
           (GETDATE() >= da.FechaVigenciaDesde AND
            (da.FechaVigenciaHasta IS NULL OR GETDATE() <= da.FechaVigenciaHasta)))
    ORDER BY da.DiaSemana, da.HoraInicio;
END
GO

-- SP_VerificarDisponibilidadParaTurno
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'SP_VerificarDisponibilidadParaTurno')
    DROP PROCEDURE SP_VerificarDisponibilidadParaTurno;
GO

CREATE PROCEDURE SP_VerificarDisponibilidadParaTurno
    @ProfesionalId INT,
    @FechaTurno DATE,
    @HoraTurno TIME,
    @DuracionMinutos INT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @DiaSemana INT = DATEPART(WEEKDAY, @FechaTurno);
    DECLARE @HoraFin TIME = DATEADD(MINUTE, @DuracionMinutos, @HoraTurno);
    DECLARE @EstaDisponible BIT = 0;

    IF EXISTS (
        SELECT 1 FROM DisponibilidadesAgenda da
        WHERE da.ProfesionalId = @ProfesionalId
          AND da.DiaSemana = @DiaSemana
          AND da.HoraInicio <= @HoraTurno
          AND da.HoraFin >= @HoraFin
          AND da.Activo = 1
          AND @FechaTurno >= da.FechaVigenciaDesde
          AND (da.FechaVigenciaHasta IS NULL OR @FechaTurno <= da.FechaVigenciaHasta)
    )
    BEGIN
        SET @EstaDisponible = 1;
    END

    SELECT @EstaDisponible AS EstaDisponible;
END
GO

-- SP_ObtenerHorariosDisponibles
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'SP_ObtenerHorariosDisponibles')
    DROP PROCEDURE SP_ObtenerHorariosDisponibles;
GO

CREATE PROCEDURE SP_ObtenerHorariosDisponibles
    @ProfesionalId INT,
    @FechaTurno DATE
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @DiaSemana INT = DATEPART(WEEKDAY, @FechaTurno);

    -- Disponibilidades del día
    SELECT HoraInicio, HoraFin, IntervaloMinutos
    FROM DisponibilidadesAgenda da
    WHERE da.ProfesionalId = @ProfesionalId
      AND da.DiaSemana = @DiaSemana
      AND da.Activo = 1
      AND @FechaTurno >= da.FechaVigenciaDesde
      AND (da.FechaVigenciaHasta IS NULL OR @FechaTurno <= da.FechaVigenciaHasta)
    ORDER BY da.HoraInicio;

    -- Turnos ocupados del día
    SELECT
        t.HoraInicio,
        CAST(DATEADD(MINUTE, t.DuracionMinutos, CAST(t.HoraInicio AS DATETIME)) AS TIME) AS HoraFin
    FROM Turnos t
    WHERE t.ProfesionalId = @ProfesionalId
      AND t.FechaTurno = @FechaTurno
      AND t.Estado NOT IN ('Cancelado')
      AND t.Activo = 1
    ORDER BY t.HoraInicio;
END
GO

-- =============================================
-- PASO 4: STORED PROCEDURES PARA PLANES
-- =============================================

PRINT 'Paso 4: Creando stored procedures para Planes...';
GO

-- SP_ListarPlanesPorObraSocial
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

PRINT 'SP_ListarPlanesPorObraSocial creado.';
GO

-- SP_ListarPlanes
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

PRINT 'SP_ListarPlanes creado.';
GO

-- SP_ObtenerPlanPorId
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

PRINT 'SP_ObtenerPlanPorId creado.';
GO

-- SP_CrearPlan
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

PRINT 'SP_CrearPlan creado.';
GO

-- SP_ActualizarPlan
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

PRINT 'SP_ActualizarPlan creado.';
GO

-- SP_EliminarPlan
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

PRINT 'SP_EliminarPlan creado.';
GO

PRINT '========================================';
PRINT 'INSTALACIÓN COMPLETADA EXITOSAMENTE';
PRINT '========================================';
PRINT '';
PRINT 'Resumen:';
PRINT '- Tabla DisponibilidadesAgenda: CREADA';
PRINT '- Stored Procedures Pacientes: 1';
PRINT '- Stored Procedures Disponibilidades: 8';
PRINT '- Stored Procedures Planes: 6';
PRINT '';
PRINT 'Total de objetos creados: 15 stored procedures + 1 tabla';
PRINT '';
GO
