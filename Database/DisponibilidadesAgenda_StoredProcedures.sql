-- ===============================================
-- SCRIPT DE BASE DE DATOS - DISPONIBILIDADES DE AGENDA
-- Sistema GECO - Gestión de Consultorios Médicos
-- ===============================================

USE GECO;
GO

-- ===============================================
-- TABLA: DisponibilidadesAgenda
-- ===============================================

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'DisponibilidadesAgenda')
BEGIN
    CREATE TABLE DisponibilidadesAgenda (
        DisponibilidadAgendaId INT PRIMARY KEY IDENTITY(1,1),
        ProfesionalId INT NOT NULL,

        -- Día de la semana (1=Lunes, 2=Martes, ..., 7=Domingo)
        DiaSemana INT NOT NULL CHECK (DiaSemana BETWEEN 1 AND 7),

        -- Horario de atención
        HoraInicio TIME NOT NULL,
        HoraFin TIME NOT NULL,

        -- Intervalo entre turnos (en minutos)
        IntervaloMinutos INT NOT NULL CHECK (IntervaloMinutos > 0),

        -- Vigencia
        FechaVigenciaDesde DATE NOT NULL,
        FechaVigenciaHasta DATE NULL,

        -- Auditoría
        Activo BIT NOT NULL DEFAULT 1,
        FechaAlta DATETIME NOT NULL DEFAULT GETDATE(),
        FechaModificacion DATETIME NULL,

        -- Clave foránea
        CONSTRAINT FK_DisponibilidadesAgenda_Profesionales FOREIGN KEY (ProfesionalId)
            REFERENCES Profesionales(ProfesionalId),

        -- Constraint para asegurar que HoraFin > HoraInicio
        CONSTRAINT CK_DisponibilidadesAgenda_Horario CHECK (HoraFin > HoraInicio),

        -- Constraint para asegurar que FechaVigenciaHasta > FechaVigenciaDesde
        CONSTRAINT CK_DisponibilidadesAgenda_Vigencia CHECK (
            FechaVigenciaHasta IS NULL OR FechaVigenciaHasta >= FechaVigenciaDesde
        )
    );

    -- Índices
    CREATE INDEX IX_DisponibilidadesAgenda_ProfesionalId ON DisponibilidadesAgenda(ProfesionalId);
    CREATE INDEX IX_DisponibilidadesAgenda_DiaSemana ON DisponibilidadesAgenda(DiaSemana);
    CREATE INDEX IX_DisponibilidadesAgenda_Vigencia ON DisponibilidadesAgenda(FechaVigenciaDesde, FechaVigenciaHasta);
    CREATE INDEX IX_DisponibilidadesAgenda_Activo ON DisponibilidadesAgenda(Activo);

    PRINT 'Tabla DisponibilidadesAgenda creada exitosamente.';
END
ELSE
BEGIN
    PRINT 'La tabla DisponibilidadesAgenda ya existe.';
END
GO

-- ===============================================
-- SP: SP_ListarDisponibilidadesAgenda
-- ===============================================

IF OBJECT_ID('SP_ListarDisponibilidadesAgenda', 'P') IS NOT NULL
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
    DECLARE @FechaHoy DATE = CAST(GETDATE() AS DATE);

    SELECT
        d.DisponibilidadAgendaId,
        d.ProfesionalId,
        d.DiaSemana,
        d.HoraInicio,
        d.HoraFin,
        d.IntervaloMinutos,
        d.FechaVigenciaDesde,
        d.FechaVigenciaHasta,
        d.Activo,
        d.FechaAlta,
        d.FechaModificacion,

        -- Datos del profesional
        p.Nombre AS ProfesionalNombre,
        p.Apellido AS ProfesionalApellido,
        p.Matricula AS ProfesionalMatricula,
        p.Especialidad AS ProfesionalEspecialidad

    FROM DisponibilidadesAgenda d
    INNER JOIN Profesionales p ON d.ProfesionalId = p.ProfesionalId
    WHERE
        (@ProfesionalId IS NULL OR d.ProfesionalId = @ProfesionalId)
        AND (@DiaSemana IS NULL OR d.DiaSemana = @DiaSemana)
        AND (@SoloActivas = 0 OR d.Activo = 1)
        AND (
            @SoloVigentes IS NULL
            OR (
                @SoloVigentes = 1
                AND d.Activo = 1
                AND @FechaHoy >= d.FechaVigenciaDesde
                AND (@FechaHoy <= d.FechaVigenciaHasta OR d.FechaVigenciaHasta IS NULL)
            )
        )
    ORDER BY p.Apellido, p.Nombre, d.DiaSemana, d.HoraInicio
    OFFSET @Offset ROWS
    FETCH NEXT @PageSize ROWS ONLY;

    -- Total de registros
    SELECT COUNT(*)
    FROM DisponibilidadesAgenda d
    WHERE
        (@ProfesionalId IS NULL OR d.ProfesionalId = @ProfesionalId)
        AND (@DiaSemana IS NULL OR d.DiaSemana = @DiaSemana)
        AND (@SoloActivas = 0 OR d.Activo = 1)
        AND (
            @SoloVigentes IS NULL
            OR (
                @SoloVigentes = 1
                AND d.Activo = 1
                AND @FechaHoy >= d.FechaVigenciaDesde
                AND (@FechaHoy <= d.FechaVigenciaHasta OR d.FechaVigenciaHasta IS NULL)
            )
        );
END
GO

PRINT 'Stored Procedure SP_ListarDisponibilidadesAgenda creado.';
GO

-- ===============================================
-- SP: SP_ObtenerDisponibilidadPorId
-- ===============================================

IF OBJECT_ID('SP_ObtenerDisponibilidadPorId', 'P') IS NOT NULL
    DROP PROCEDURE SP_ObtenerDisponibilidadPorId;
GO

CREATE PROCEDURE SP_ObtenerDisponibilidadPorId
    @DisponibilidadAgendaId INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        d.DisponibilidadAgendaId,
        d.ProfesionalId,
        d.DiaSemana,
        d.HoraInicio,
        d.HoraFin,
        d.IntervaloMinutos,
        d.FechaVigenciaDesde,
        d.FechaVigenciaHasta,
        d.Activo,
        d.FechaAlta,
        d.FechaModificacion,

        -- Datos del profesional
        p.Nombre AS ProfesionalNombre,
        p.Apellido AS ProfesionalApellido,
        p.Matricula AS ProfesionalMatricula,
        p.Especialidad AS ProfesionalEspecialidad

    FROM DisponibilidadesAgenda d
    INNER JOIN Profesionales p ON d.ProfesionalId = p.ProfesionalId
    WHERE d.DisponibilidadAgendaId = @DisponibilidadAgendaId;
END
GO

PRINT 'Stored Procedure SP_ObtenerDisponibilidadPorId creado.';
GO

-- ===============================================
-- SP: SP_ObtenerDisponibilidadesPorProfesional
-- ===============================================

IF OBJECT_ID('SP_ObtenerDisponibilidadesPorProfesional', 'P') IS NOT NULL
    DROP PROCEDURE SP_ObtenerDisponibilidadesPorProfesional;
GO

CREATE PROCEDURE SP_ObtenerDisponibilidadesPorProfesional
    @ProfesionalId INT,
    @SoloVigentes BIT = 1
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @FechaHoy DATE = CAST(GETDATE() AS DATE);

    SELECT
        d.DisponibilidadAgendaId,
        d.ProfesionalId,
        d.DiaSemana,
        d.HoraInicio,
        d.HoraFin,
        d.IntervaloMinutos,
        d.FechaVigenciaDesde,
        d.FechaVigenciaHasta,
        d.Activo,
        d.FechaAlta,
        d.FechaModificacion,

        -- Datos del profesional
        p.Nombre AS ProfesionalNombre,
        p.Apellido AS ProfesionalApellido,
        p.Matricula AS ProfesionalMatricula,
        p.Especialidad AS ProfesionalEspecialidad

    FROM DisponibilidadesAgenda d
    INNER JOIN Profesionales p ON d.ProfesionalId = p.ProfesionalId
    WHERE
        d.ProfesionalId = @ProfesionalId
        AND d.Activo = 1
        AND (
            @SoloVigentes = 0
            OR (
                @FechaHoy >= d.FechaVigenciaDesde
                AND (@FechaHoy <= d.FechaVigenciaHasta OR d.FechaVigenciaHasta IS NULL)
            )
        )
    ORDER BY d.DiaSemana, d.HoraInicio;
END
GO

PRINT 'Stored Procedure SP_ObtenerDisponibilidadesPorProfesional creado.';
GO

-- ===============================================
-- SP: SP_CrearDisponibilidadAgenda
-- ===============================================

IF OBJECT_ID('SP_CrearDisponibilidadAgenda', 'P') IS NOT NULL
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

    BEGIN TRY
        INSERT INTO DisponibilidadesAgenda (
            ProfesionalId, DiaSemana, HoraInicio, HoraFin,
            IntervaloMinutos, FechaVigenciaDesde, FechaVigenciaHasta,
            Activo, FechaAlta
        )
        VALUES (
            @ProfesionalId, @DiaSemana, @HoraInicio, @HoraFin,
            @IntervaloMinutos, @FechaVigenciaDesde, @FechaVigenciaHasta,
            1, GETDATE()
        );

        SELECT SCOPE_IDENTITY() AS DisponibilidadAgendaId;
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO

PRINT 'Stored Procedure SP_CrearDisponibilidadAgenda creado.';
GO

-- ===============================================
-- SP: SP_ActualizarDisponibilidadAgenda
-- ===============================================

IF OBJECT_ID('SP_ActualizarDisponibilidadAgenda', 'P') IS NOT NULL
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
    SET
        ProfesionalId = @ProfesionalId,
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

PRINT 'Stored Procedure SP_ActualizarDisponibilidadAgenda creado.';
GO

-- ===============================================
-- SP: SP_EliminarDisponibilidadAgenda
-- ===============================================

IF OBJECT_ID('SP_EliminarDisponibilidadAgenda', 'P') IS NOT NULL
    DROP PROCEDURE SP_EliminarDisponibilidadAgenda;
GO

CREATE PROCEDURE SP_EliminarDisponibilidadAgenda
    @DisponibilidadAgendaId INT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE DisponibilidadesAgenda
    SET
        Activo = 0,
        FechaModificacion = GETDATE()
    WHERE DisponibilidadAgendaId = @DisponibilidadAgendaId;

    SELECT @@ROWCOUNT AS FilasAfectadas;
END
GO

PRINT 'Stored Procedure SP_EliminarDisponibilidadAgenda creado.';
GO

-- ===============================================
-- SP: SP_VerificarDisponibilidadParaTurno
-- ===============================================

IF OBJECT_ID('SP_VerificarDisponibilidadParaTurno', 'P') IS NOT NULL
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

    -- Obtener el día de la semana (1=Lunes, 7=Domingo)
    DECLARE @DiaSemana INT = DATEPART(WEEKDAY, @FechaTurno);
    -- Ajustar para que Lunes=1 (por defecto SQL Server tiene Domingo=1)
    IF @DiaSemana = 1
        SET @DiaSemana = 7;
    ELSE
        SET @DiaSemana = @DiaSemana - 1;

    DECLARE @HoraFinTurno TIME = DATEADD(MINUTE, @DuracionMinutos, CAST(@HoraTurno AS DATETIME));

    -- Buscar disponibilidades que cubran este horario
    SELECT
        d.DisponibilidadAgendaId,
        d.HoraInicio,
        d.HoraFin,
        d.IntervaloMinutos,
        CASE
            WHEN @HoraTurno >= d.HoraInicio
                AND @HoraFinTurno <= d.HoraFin
                AND @FechaTurno >= d.FechaVigenciaDesde
                AND (@FechaTurno <= d.FechaVigenciaHasta OR d.FechaVigenciaHasta IS NULL)
                AND d.Activo = 1
            THEN 1
            ELSE 0
        END AS EstaDisponible
    FROM DisponibilidadesAgenda d
    WHERE
        d.ProfesionalId = @ProfesionalId
        AND d.DiaSemana = @DiaSemana
        AND d.Activo = 1
        AND @FechaTurno >= d.FechaVigenciaDesde
        AND (@FechaTurno <= d.FechaVigenciaHasta OR d.FechaVigenciaHasta IS NULL);

    -- Si no devuelve ningún registro, el profesional no tiene disponibilidad ese día
END
GO

PRINT 'Stored Procedure SP_VerificarDisponibilidadParaTurno creado.';
GO

-- ===============================================
-- SP: SP_ObtenerHorariosDisponibles
-- ===============================================

IF OBJECT_ID('SP_ObtenerHorariosDisponibles', 'P') IS NOT NULL
    DROP PROCEDURE SP_ObtenerHorariosDisponibles;
GO

CREATE PROCEDURE SP_ObtenerHorariosDisponibles
    @ProfesionalId INT,
    @FechaTurno DATE
AS
BEGIN
    SET NOCOUNT ON;

    -- Obtener el día de la semana (1=Lunes, 7=Domingo)
    DECLARE @DiaSemana INT = DATEPART(WEEKDAY, @FechaTurno);
    -- Ajustar para que Lunes=1
    IF @DiaSemana = 1
        SET @DiaSemana = 7;
    ELSE
        SET @DiaSemana = @DiaSemana - 1;

    -- Obtener las disponibilidades para ese día
    SELECT
        d.DisponibilidadAgendaId,
        d.HoraInicio,
        d.HoraFin,
        d.IntervaloMinutos
    FROM DisponibilidadesAgenda d
    WHERE
        d.ProfesionalId = @ProfesionalId
        AND d.DiaSemana = @DiaSemana
        AND d.Activo = 1
        AND @FechaTurno >= d.FechaVigenciaDesde
        AND (@FechaTurno <= d.FechaVigenciaHasta OR d.FechaVigenciaHasta IS NULL)
    ORDER BY d.HoraInicio;

    -- Obtener los turnos ya ocupados para ese día
    SELECT
        HoraInicio,
        HoraFin
    FROM Turnos
    WHERE
        ProfesionalId = @ProfesionalId
        AND CAST(FechaTurno AS DATE) = @FechaTurno
        AND Estado NOT IN ('Cancelado', 'Ausente')
        AND Activo = 1
    ORDER BY HoraInicio;
END
GO

PRINT 'Stored Procedure SP_ObtenerHorariosDisponibles creado.';
GO

-- ===============================================
-- SCRIPT COMPLETADO
-- ===============================================

PRINT '';
PRINT '========================================';
PRINT 'SCRIPT COMPLETADO EXITOSAMENTE';
PRINT '========================================';
PRINT 'Tabla creada: DisponibilidadesAgenda';
PRINT 'Stored Procedures creados: 8';
PRINT '  - SP_ListarDisponibilidadesAgenda';
PRINT '  - SP_ObtenerDisponibilidadPorId';
PRINT '  - SP_ObtenerDisponibilidadesPorProfesional';
PRINT '  - SP_CrearDisponibilidadAgenda';
PRINT '  - SP_ActualizarDisponibilidadAgenda';
PRINT '  - SP_EliminarDisponibilidadAgenda';
PRINT '  - SP_VerificarDisponibilidadParaTurno';
PRINT '  - SP_ObtenerHorariosDisponibles';
PRINT '========================================';
GO
