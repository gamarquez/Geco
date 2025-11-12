-- ===============================================
-- SCRIPT DE BASE DE DATOS - TURNOS/AGENDA
-- Sistema GECO - Gestión de Consultorios Médicos
-- ===============================================

USE GECO;
GO

-- ===============================================
-- TABLA: Turnos
-- ===============================================

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Turnos')
BEGIN
    CREATE TABLE Turnos (
        TurnoId INT PRIMARY KEY IDENTITY(1,1),
        PacienteId INT NOT NULL,
        ProfesionalId INT NOT NULL,
        FechaTurno DATE NOT NULL,
        HoraInicio TIME NOT NULL,
        HoraFin TIME NOT NULL,
        DuracionMinutos INT NOT NULL,
        MotivoConsulta NVARCHAR(500) NOT NULL,
        Estado NVARCHAR(20) NOT NULL DEFAULT 'Pendiente', -- Pendiente, Confirmado, EnCurso, Completado, Cancelado, Ausente
        MotivoCancelacion NVARCHAR(500) NULL,
        Observaciones NVARCHAR(MAX) NULL,

        -- Auditoría
        Activo BIT NOT NULL DEFAULT 1,
        FechaAlta DATETIME NOT NULL DEFAULT GETDATE(),
        FechaModificacion DATETIME NULL,

        -- Claves foráneas
        CONSTRAINT FK_Turnos_Pacientes FOREIGN KEY (PacienteId)
            REFERENCES Pacientes(PacienteId),
        CONSTRAINT FK_Turnos_Profesionales FOREIGN KEY (ProfesionalId)
            REFERENCES Profesionales(ProfesionalId),

        -- Constraints
        CONSTRAINT CK_Turnos_DuracionMinutos CHECK (DuracionMinutos > 0 AND DuracionMinutos <= 480),
        CONSTRAINT CK_Turnos_Estado CHECK (Estado IN ('Pendiente', 'Confirmado', 'EnCurso', 'Completado', 'Cancelado', 'Ausente'))
    );

    -- Índices para mejorar el rendimiento
    CREATE INDEX IX_Turnos_PacienteId ON Turnos(PacienteId);
    CREATE INDEX IX_Turnos_ProfesionalId ON Turnos(ProfesionalId);
    CREATE INDEX IX_Turnos_FechaTurno ON Turnos(FechaTurno);
    CREATE INDEX IX_Turnos_Estado ON Turnos(Estado);
    CREATE INDEX IX_Turnos_Activo ON Turnos(Activo);
    CREATE INDEX IX_Turnos_ProfesionalFecha ON Turnos(ProfesionalId, FechaTurno);

    PRINT 'Tabla Turnos creada exitosamente.';
END
ELSE
BEGIN
    PRINT 'La tabla Turnos ya existe.';
END
GO

-- ===============================================
-- SP: SP_ListarTurnos
-- Descripción: Lista turnos con filtros y paginación
-- ===============================================

IF OBJECT_ID('SP_ListarTurnos', 'P') IS NOT NULL
    DROP PROCEDURE SP_ListarTurnos;
GO

CREATE PROCEDURE SP_ListarTurnos
    @PacienteId INT = NULL,
    @ProfesionalId INT = NULL,
    @FechaDesde DATE = NULL,
    @FechaHasta DATE = NULL,
    @Estado NVARCHAR(20) = NULL,
    @PageNumber INT = 1,
    @PageSize INT = 20,
    @SoloActivos BIT = 1
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;

    -- Resultado paginado
    SELECT
        t.TurnoId,
        t.PacienteId,
        t.ProfesionalId,
        t.FechaTurno,
        t.HoraInicio,
        t.HoraFin,
        t.DuracionMinutos,
        t.MotivoConsulta,
        t.Estado,
        t.MotivoCancelacion,
        t.Observaciones,
        t.Activo,
        t.FechaAlta,
        t.FechaModificacion,

        -- Datos del paciente
        p.Nombre AS PacienteNombre,
        p.Apellido AS PacienteApellido,
        p.NumeroDocumento AS PacienteDocumento,
        p.Telefono AS PacienteTelefono,

        -- Datos del profesional
        pr.Nombre AS ProfesionalNombre,
        pr.Apellido AS ProfesionalApellido,
        pr.Matricula AS ProfesionalMatricula

    FROM Turnos t
    INNER JOIN Pacientes p ON t.PacienteId = p.PacienteId
    INNER JOIN Profesionales pr ON t.ProfesionalId = pr.ProfesionalId
    WHERE
        (@PacienteId IS NULL OR t.PacienteId = @PacienteId)
        AND (@ProfesionalId IS NULL OR t.ProfesionalId = @ProfesionalId)
        AND (@FechaDesde IS NULL OR t.FechaTurno >= @FechaDesde)
        AND (@FechaHasta IS NULL OR t.FechaTurno <= @FechaHasta)
        AND (@Estado IS NULL OR t.Estado = @Estado)
        AND (@SoloActivos = 0 OR t.Activo = 1)
    ORDER BY t.FechaTurno DESC, t.HoraInicio DESC
    OFFSET @Offset ROWS
    FETCH NEXT @PageSize ROWS ONLY;

    -- Total de registros
    SELECT COUNT(*)
    FROM Turnos t
    WHERE
        (@PacienteId IS NULL OR t.PacienteId = @PacienteId)
        AND (@ProfesionalId IS NULL OR t.ProfesionalId = @ProfesionalId)
        AND (@FechaDesde IS NULL OR t.FechaTurno >= @FechaDesde)
        AND (@FechaHasta IS NULL OR t.FechaTurno <= @FechaHasta)
        AND (@Estado IS NULL OR t.Estado = @Estado)
        AND (@SoloActivos = 0 OR t.Activo = 1);
END
GO

PRINT 'Stored Procedure SP_ListarTurnos creado.';
GO

-- ===============================================
-- SP: SP_ObtenerTurnoPorId
-- Descripción: Obtiene un turno por ID
-- ===============================================

IF OBJECT_ID('SP_ObtenerTurnoPorId', 'P') IS NOT NULL
    DROP PROCEDURE SP_ObtenerTurnoPorId;
GO

CREATE PROCEDURE SP_ObtenerTurnoPorId
    @TurnoId INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        t.TurnoId,
        t.PacienteId,
        t.ProfesionalId,
        t.FechaTurno,
        t.HoraInicio,
        t.HoraFin,
        t.DuracionMinutos,
        t.MotivoConsulta,
        t.Estado,
        t.MotivoCancelacion,
        t.Observaciones,
        t.Activo,
        t.FechaAlta,
        t.FechaModificacion,

        -- Datos del paciente
        p.Nombre AS PacienteNombre,
        p.Apellido AS PacienteApellido,
        p.NumeroDocumento AS PacienteDocumento,
        p.Telefono AS PacienteTelefono,

        -- Datos del profesional
        pr.Nombre AS ProfesionalNombre,
        pr.Apellido AS ProfesionalApellido,
        pr.Matricula AS ProfesionalMatricula

    FROM Turnos t
    INNER JOIN Pacientes p ON t.PacienteId = p.PacienteId
    INNER JOIN Profesionales pr ON t.ProfesionalId = pr.ProfesionalId
    WHERE t.TurnoId = @TurnoId;
END
GO

PRINT 'Stored Procedure SP_ObtenerTurnoPorId creado.';
GO

-- ===============================================
-- SP: SP_ObtenerTurnosPorPaciente
-- Descripción: Obtiene todos los turnos de un paciente
-- ===============================================

IF OBJECT_ID('SP_ObtenerTurnosPorPaciente', 'P') IS NOT NULL
    DROP PROCEDURE SP_ObtenerTurnosPorPaciente;
GO

CREATE PROCEDURE SP_ObtenerTurnosPorPaciente
    @PacienteId INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        t.TurnoId,
        t.PacienteId,
        t.ProfesionalId,
        t.FechaTurno,
        t.HoraInicio,
        t.HoraFin,
        t.DuracionMinutos,
        t.MotivoConsulta,
        t.Estado,
        t.MotivoCancelacion,
        t.Observaciones,
        t.Activo,
        t.FechaAlta,
        t.FechaModificacion,

        -- Datos del paciente
        p.Nombre AS PacienteNombre,
        p.Apellido AS PacienteApellido,
        p.NumeroDocumento AS PacienteDocumento,
        p.Telefono AS PacienteTelefono,

        -- Datos del profesional
        pr.Nombre AS ProfesionalNombre,
        pr.Apellido AS ProfesionalApellido,
        pr.Matricula AS ProfesionalMatricula

    FROM Turnos t
    INNER JOIN Pacientes p ON t.PacienteId = p.PacienteId
    INNER JOIN Profesionales pr ON t.ProfesionalId = pr.ProfesionalId
    WHERE t.PacienteId = @PacienteId
        AND t.Activo = 1
    ORDER BY t.FechaTurno DESC, t.HoraInicio DESC;
END
GO

PRINT 'Stored Procedure SP_ObtenerTurnosPorPaciente creado.';
GO

-- ===============================================
-- SP: SP_ObtenerAgendaProfesional
-- Descripción: Obtiene la agenda de un profesional para una fecha
-- ===============================================

IF OBJECT_ID('SP_ObtenerAgendaProfesional', 'P') IS NOT NULL
    DROP PROCEDURE SP_ObtenerAgendaProfesional;
GO

CREATE PROCEDURE SP_ObtenerAgendaProfesional
    @ProfesionalId INT,
    @Fecha DATE
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        t.TurnoId,
        t.PacienteId,
        t.ProfesionalId,
        t.FechaTurno,
        t.HoraInicio,
        t.HoraFin,
        t.DuracionMinutos,
        t.MotivoConsulta,
        t.Estado,
        t.MotivoCancelacion,
        t.Observaciones,
        t.Activo,
        t.FechaAlta,
        t.FechaModificacion,

        -- Datos del paciente
        p.Nombre AS PacienteNombre,
        p.Apellido AS PacienteApellido,
        p.NumeroDocumento AS PacienteDocumento,
        p.Telefono AS PacienteTelefono,

        -- Datos del profesional
        pr.Nombre AS ProfesionalNombre,
        pr.Apellido AS ProfesionalApellido,
        pr.Matricula AS ProfesionalMatricula

    FROM Turnos t
    INNER JOIN Pacientes p ON t.PacienteId = p.PacienteId
    INNER JOIN Profesionales pr ON t.ProfesionalId = pr.ProfesionalId
    WHERE t.ProfesionalId = @ProfesionalId
        AND t.FechaTurno = @Fecha
        AND t.Activo = 1
    ORDER BY t.HoraInicio ASC;
END
GO

PRINT 'Stored Procedure SP_ObtenerAgendaProfesional creado.';
GO

-- ===============================================
-- SP: SP_VerificarDisponibilidad
-- Descripción: Verifica si hay disponibilidad horaria
-- ===============================================

IF OBJECT_ID('SP_VerificarDisponibilidad', 'P') IS NOT NULL
    DROP PROCEDURE SP_VerificarDisponibilidad;
GO

CREATE PROCEDURE SP_VerificarDisponibilidad
    @ProfesionalId INT,
    @FechaTurno DATE,
    @HoraInicio TIME,
    @DuracionMinutos INT,
    @TurnoIdExcluir INT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @HoraFin TIME = DATEADD(MINUTE, @DuracionMinutos, @HoraInicio);
    DECLARE @HayConflicto INT;

    SELECT @HayConflicto = COUNT(*)
    FROM Turnos
    WHERE ProfesionalId = @ProfesionalId
        AND FechaTurno = @FechaTurno
        AND Activo = 1
        AND Estado NOT IN ('Cancelado')
        AND (@TurnoIdExcluir IS NULL OR TurnoId != @TurnoIdExcluir)
        AND (
            -- El nuevo turno comienza durante un turno existente
            (@HoraInicio >= HoraInicio AND @HoraInicio < HoraFin)
            -- El nuevo turno termina durante un turno existente
            OR (@HoraFin > HoraInicio AND @HoraFin <= HoraFin)
            -- El nuevo turno engloba completamente un turno existente
            OR (@HoraInicio <= HoraInicio AND @HoraFin >= HoraFin)
        );

    -- Retornar 1 si hay disponibilidad (no hay conflictos), 0 si no hay disponibilidad
    SELECT CASE WHEN @HayConflicto = 0 THEN 1 ELSE 0 END AS Disponible;
END
GO

PRINT 'Stored Procedure SP_VerificarDisponibilidad creado.';
GO

-- ===============================================
-- SP: SP_CrearTurno
-- Descripción: Crea un nuevo turno
-- ===============================================

IF OBJECT_ID('SP_CrearTurno', 'P') IS NOT NULL
    DROP PROCEDURE SP_CrearTurno;
GO

CREATE PROCEDURE SP_CrearTurno
    @PacienteId INT,
    @ProfesionalId INT,
    @FechaTurno DATE,
    @HoraInicio TIME,
    @DuracionMinutos INT,
    @MotivoConsulta NVARCHAR(500),
    @Observaciones NVARCHAR(MAX) = NULL,
    @Estado NVARCHAR(20) = 'Pendiente'
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        DECLARE @HoraFin TIME = DATEADD(MINUTE, @DuracionMinutos, @HoraInicio);

        -- Validar que el paciente existe y está activo
        IF NOT EXISTS (SELECT 1 FROM Pacientes WHERE PacienteId = @PacienteId AND Activo = 1)
        BEGIN
            RAISERROR('El paciente no existe o está inactivo.', 16, 1);
            RETURN;
        END

        -- Validar que el profesional existe y está activo
        IF NOT EXISTS (SELECT 1 FROM Profesionales WHERE ProfesionalId = @ProfesionalId AND Activo = 1)
        BEGIN
            RAISERROR('El profesional no existe o está inactivo.', 16, 1);
            RETURN;
        END

        -- Insertar el turno
        INSERT INTO Turnos (
            PacienteId, ProfesionalId, FechaTurno, HoraInicio, HoraFin,
            DuracionMinutos, MotivoConsulta, Observaciones, Estado,
            Activo, FechaAlta
        )
        VALUES (
            @PacienteId, @ProfesionalId, @FechaTurno, @HoraInicio, @HoraFin,
            @DuracionMinutos, @MotivoConsulta, @Observaciones, @Estado,
            1, GETDATE()
        );

        -- Retornar el ID generado
        SELECT SCOPE_IDENTITY() AS TurnoId;
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO

PRINT 'Stored Procedure SP_CrearTurno creado.';
GO

-- ===============================================
-- SP: SP_ActualizarTurno
-- Descripción: Actualiza un turno existente
-- ===============================================

IF OBJECT_ID('SP_ActualizarTurno', 'P') IS NOT NULL
    DROP PROCEDURE SP_ActualizarTurno;
GO

CREATE PROCEDURE SP_ActualizarTurno
    @TurnoId INT,
    @PacienteId INT,
    @ProfesionalId INT,
    @FechaTurno DATE,
    @HoraInicio TIME,
    @DuracionMinutos INT,
    @MotivoConsulta NVARCHAR(500),
    @Estado NVARCHAR(20),
    @MotivoCancelacion NVARCHAR(500) = NULL,
    @Observaciones NVARCHAR(MAX) = NULL,
    @Activo BIT
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        DECLARE @HoraFin TIME = DATEADD(MINUTE, @DuracionMinutos, @HoraInicio);

        -- Validar que el turno existe
        IF NOT EXISTS (SELECT 1 FROM Turnos WHERE TurnoId = @TurnoId)
        BEGIN
            RAISERROR('El turno no existe.', 16, 1);
            RETURN;
        END

        -- Actualizar el turno
        UPDATE Turnos
        SET
            PacienteId = @PacienteId,
            ProfesionalId = @ProfesionalId,
            FechaTurno = @FechaTurno,
            HoraInicio = @HoraInicio,
            HoraFin = @HoraFin,
            DuracionMinutos = @DuracionMinutos,
            MotivoConsulta = @MotivoConsulta,
            Estado = @Estado,
            MotivoCancelacion = @MotivoCancelacion,
            Observaciones = @Observaciones,
            Activo = @Activo,
            FechaModificacion = GETDATE()
        WHERE TurnoId = @TurnoId;

    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO

PRINT 'Stored Procedure SP_ActualizarTurno creado.';
GO

-- ===============================================
-- SP: SP_CancelarTurno
-- Descripción: Cancela un turno
-- ===============================================

IF OBJECT_ID('SP_CancelarTurno', 'P') IS NOT NULL
    DROP PROCEDURE SP_CancelarTurno;
GO

CREATE PROCEDURE SP_CancelarTurno
    @TurnoId INT,
    @MotivoCancelacion NVARCHAR(500)
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        -- Validar que el turno existe
        IF NOT EXISTS (SELECT 1 FROM Turnos WHERE TurnoId = @TurnoId)
        BEGIN
            RAISERROR('El turno no existe.', 16, 1);
            RETURN;
        END

        -- Cancelar el turno
        UPDATE Turnos
        SET
            Estado = 'Cancelado',
            MotivoCancelacion = @MotivoCancelacion,
            FechaModificacion = GETDATE()
        WHERE TurnoId = @TurnoId;

    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO

PRINT 'Stored Procedure SP_CancelarTurno creado.';
GO

-- ===============================================
-- SCRIPT COMPLETADO
-- ===============================================

PRINT '';
PRINT '========================================';
PRINT 'SCRIPT COMPLETADO EXITOSAMENTE';
PRINT '========================================';
PRINT 'Tabla creada: Turnos';
PRINT 'Stored Procedures creados: 8';
PRINT '  - SP_ListarTurnos';
PRINT '  - SP_ObtenerTurnoPorId';
PRINT '  - SP_ObtenerTurnosPorPaciente';
PRINT '  - SP_ObtenerAgendaProfesional';
PRINT '  - SP_VerificarDisponibilidad';
PRINT '  - SP_CrearTurno';
PRINT '  - SP_ActualizarTurno';
PRINT '  - SP_CancelarTurno';
PRINT '========================================';
GO
