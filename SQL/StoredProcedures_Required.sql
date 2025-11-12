-- =============================================
-- STORED PROCEDURES REQUERIDOS PARA GECO
-- =============================================
-- Ejecutar este script en la base de datos GECO
-- =============================================

USE GECO;
GO

-- =============================================
-- 1. SP_ObtenerPacientePorDocumento
-- Obtiene un paciente por tipo y número de documento
-- =============================================
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
        -- Datos de la obra social
        os.Nombre AS ObraSocialNombre,
        os.CUIT AS ObraSocialCUIT,
        -- Datos del plan
        pl.Nombre AS PlanNombre
    FROM Pacientes p
    LEFT JOIN ObrasSociales os ON p.ObraSocialId = os.ObraSocialId
    LEFT JOIN Planes pl ON p.PlanId = pl.PlanId
    WHERE p.TipoDocumento = @TipoDocumento
      AND p.NumeroDocumento = @NumeroDocumento;
END
GO

-- =============================================
-- 2. SP_CrearDisponibilidadAgenda
-- Crea una nueva disponibilidad de agenda
-- =============================================
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
        ProfesionalId,
        DiaSemana,
        HoraInicio,
        HoraFin,
        IntervaloMinutos,
        FechaVigenciaDesde,
        FechaVigenciaHasta,
        Activo,
        FechaAlta
    )
    VALUES (
        @ProfesionalId,
        @DiaSemana,
        @HoraInicio,
        @HoraFin,
        @IntervaloMinutos,
        @FechaVigenciaDesde,
        @FechaVigenciaHasta,
        1,
        GETDATE()
    );

    SELECT SCOPE_IDENTITY() AS DisponibilidadAgendaId;
END
GO

-- =============================================
-- 3. SP_ActualizarDisponibilidadAgenda
-- Actualiza una disponibilidad existente
-- =============================================
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

-- =============================================
-- 4. SP_EliminarDisponibilidadAgenda
-- Elimina (desactiva) una disponibilidad
-- =============================================
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'SP_EliminarDisponibilidadAgenda')
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

-- =============================================
-- 5. SP_ListarDisponibilidadesAgenda
-- Lista disponibilidades con filtros y paginación
-- =============================================
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

    -- Tabla temporal para almacenar los resultados filtrados
    DECLARE @TotalRegistros INT;

    ;WITH DisponibilidadesFiltradas AS (
        SELECT
            da.DisponibilidadAgendaId,
            da.ProfesionalId,
            da.DiaSemana,
            da.HoraInicio,
            da.HoraFin,
            da.IntervaloMinutos,
            da.FechaVigenciaDesde,
            da.FechaVigenciaHasta,
            da.Activo,
            da.FechaAlta,
            da.FechaModificacion,
            -- Datos del profesional
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
               (@SoloVigentes = 1 AND
                GETDATE() >= da.FechaVigenciaDesde AND
                (da.FechaVigenciaHasta IS NULL OR GETDATE() <= da.FechaVigenciaHasta)))
    )
    SELECT * FROM DisponibilidadesFiltradas
    ORDER BY DiaSemana, HoraInicio
    OFFSET @Offset ROWS
    FETCH NEXT @PageSize ROWS ONLY;

    -- Total de registros
    SELECT @TotalRegistros = COUNT(*)
    FROM DisponibilidadesAgenda da
    WHERE (@SoloActivas = 0 OR da.Activo = 1)
      AND (@ProfesionalId IS NULL OR da.ProfesionalId = @ProfesionalId)
      AND (@DiaSemana IS NULL OR da.DiaSemana = @DiaSemana)
      AND (@SoloVigentes IS NULL OR
           (@SoloVigentes = 1 AND
            GETDATE() >= da.FechaVigenciaDesde AND
            (da.FechaVigenciaHasta IS NULL OR GETDATE() <= da.FechaVigenciaHasta)));

    SELECT @TotalRegistros AS TotalRegistros;
END
GO

-- =============================================
-- 6. SP_ObtenerDisponibilidadPorId
-- Obtiene una disponibilidad por ID
-- =============================================
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'SP_ObtenerDisponibilidadPorId')
    DROP PROCEDURE SP_ObtenerDisponibilidadPorId;
GO

CREATE PROCEDURE SP_ObtenerDisponibilidadPorId
    @DisponibilidadAgendaId INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        da.DisponibilidadAgendaId,
        da.ProfesionalId,
        da.DiaSemana,
        da.HoraInicio,
        da.HoraFin,
        da.IntervaloMinutos,
        da.FechaVigenciaDesde,
        da.FechaVigenciaHasta,
        da.Activo,
        da.FechaAlta,
        da.FechaModificacion,
        -- Datos del profesional
        p.Nombre AS ProfesionalNombre,
        p.Apellido AS ProfesionalApellido,
        p.Matricula AS ProfesionalMatricula,
        p.Especialidad AS ProfesionalEspecialidad
    FROM DisponibilidadesAgenda da
    INNER JOIN Profesionales p ON da.ProfesionalId = p.ProfesionalId
    WHERE da.DisponibilidadAgendaId = @DisponibilidadAgendaId;
END
GO

-- =============================================
-- 7. SP_ObtenerDisponibilidadesPorProfesional
-- Obtiene las disponibilidades de un profesional
-- =============================================
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
        da.DisponibilidadAgendaId,
        da.ProfesionalId,
        da.DiaSemana,
        da.HoraInicio,
        da.HoraFin,
        da.IntervaloMinutos,
        da.FechaVigenciaDesde,
        da.FechaVigenciaHasta,
        da.Activo,
        da.FechaAlta,
        da.FechaModificacion,
        -- Datos del profesional
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

-- =============================================
-- 8. SP_VerificarDisponibilidadParaTurno
-- Verifica si existe disponibilidad para un turno específico
-- =============================================
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

    -- Verificar si existe una disponibilidad que cubra este horario
    IF EXISTS (
        SELECT 1
        FROM DisponibilidadesAgenda da
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

-- =============================================
-- 9. SP_ObtenerHorariosDisponibles
-- Obtiene los horarios disponibles para un profesional en una fecha
-- y los turnos ya ocupados
-- =============================================
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

    -- Primera consulta: Disponibilidades del día
    SELECT
        da.HoraInicio,
        da.HoraFin,
        da.IntervaloMinutos
    FROM DisponibilidadesAgenda da
    WHERE da.ProfesionalId = @ProfesionalId
      AND da.DiaSemana = @DiaSemana
      AND da.Activo = 1
      AND @FechaTurno >= da.FechaVigenciaDesde
      AND (da.FechaVigenciaHasta IS NULL OR @FechaTurno <= da.FechaVigenciaHasta)
    ORDER BY da.HoraInicio;

    -- Segunda consulta: Turnos ocupados del día
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

PRINT 'Stored Procedures creados exitosamente.';
GO
