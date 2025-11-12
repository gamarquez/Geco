-- ===============================================
-- SCRIPT DE BASE DE DATOS - HISTORIAS CLÍNICAS
-- Sistema GECO - Gestión de Consultorios Médicos
-- ===============================================

USE GECO;
GO

-- ===============================================
-- TABLA: HistoriasClinicas
-- ===============================================

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'HistoriasClinicas')
BEGIN
    CREATE TABLE HistoriasClinicas (
        HistoriaClinicaId INT PRIMARY KEY IDENTITY(1,1),
        PacienteId INT NOT NULL,
        ProfesionalId INT NOT NULL,
        FechaConsulta DATETIME NOT NULL DEFAULT GETDATE(),
        MotivoConsulta NVARCHAR(500) NOT NULL,
        Anamnesis NVARCHAR(MAX) NULL,
        ExamenFisico NVARCHAR(MAX) NULL,
        Diagnostico NVARCHAR(1000) NOT NULL,
        Tratamiento NVARCHAR(MAX) NULL,
        Observaciones NVARCHAR(MAX) NULL,

        -- Signos vitales
        Peso DECIMAL(5,2) NULL,
        Altura DECIMAL(5,2) NULL,
        PresionArterial DECIMAL(5,2) NULL,
        Temperatura DECIMAL(4,2) NULL,
        FrecuenciaCardiaca DECIMAL(5,2) NULL,

        -- Auditoría
        Activo BIT NOT NULL DEFAULT 1,
        FechaAlta DATETIME NOT NULL DEFAULT GETDATE(),
        FechaModificacion DATETIME NULL,

        -- Claves foráneas
        CONSTRAINT FK_HistoriasClinicas_Pacientes FOREIGN KEY (PacienteId)
            REFERENCES Pacientes(PacienteId),
        CONSTRAINT FK_HistoriasClinicas_Profesionales FOREIGN KEY (ProfesionalId)
            REFERENCES Profesionales(ProfesionalId)
    );

    -- Índices para mejorar el rendimiento
    CREATE INDEX IX_HistoriasClinicas_PacienteId ON HistoriasClinicas(PacienteId);
    CREATE INDEX IX_HistoriasClinicas_ProfesionalId ON HistoriasClinicas(ProfesionalId);
    CREATE INDEX IX_HistoriasClinicas_FechaConsulta ON HistoriasClinicas(FechaConsulta);
    CREATE INDEX IX_HistoriasClinicas_Activo ON HistoriasClinicas(Activo);

    PRINT 'Tabla HistoriasClinicas creada exitosamente.';
END
ELSE
BEGIN
    PRINT 'La tabla HistoriasClinicas ya existe.';
END
GO

-- ===============================================
-- SP: SP_ListarHistoriasClinicas
-- Descripción: Lista historias clínicas con filtros y paginación
-- ===============================================

IF OBJECT_ID('SP_ListarHistoriasClinicas', 'P') IS NOT NULL
    DROP PROCEDURE SP_ListarHistoriasClinicas;
GO

CREATE PROCEDURE SP_ListarHistoriasClinicas
    @PacienteId INT = NULL,
    @ProfesionalId INT = NULL,
    @FechaDesde DATETIME = NULL,
    @FechaHasta DATETIME = NULL,
    @Diagnostico NVARCHAR(1000) = NULL,
    @PageNumber INT = 1,
    @PageSize INT = 20,
    @SoloActivas BIT = 1
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;

    -- Resultado paginado
    SELECT
        hc.HistoriaClinicaId,
        hc.PacienteId,
        hc.ProfesionalId,
        hc.FechaConsulta,
        hc.MotivoConsulta,
        hc.Anamnesis,
        hc.ExamenFisico,
        hc.Diagnostico,
        hc.Tratamiento,
        hc.Observaciones,
        hc.Peso,
        hc.Altura,
        hc.PresionArterial,
        hc.Temperatura,
        hc.FrecuenciaCardiaca,
        hc.Activo,
        hc.FechaAlta,

        -- Datos del paciente
        p.Nombre AS PacienteNombre,
        p.Apellido AS PacienteApellido,
        p.NumeroDocumento AS PacienteDocumento,

        -- Datos del profesional
        pr.Nombre AS ProfesionalNombre,
        pr.Apellido AS ProfesionalApellido,
        pr.Matricula AS ProfesionalMatricula

    FROM HistoriasClinicas hc
    INNER JOIN Pacientes p ON hc.PacienteId = p.PacienteId
    INNER JOIN Profesionales pr ON hc.ProfesionalId = pr.ProfesionalId
    WHERE
        (@PacienteId IS NULL OR hc.PacienteId = @PacienteId)
        AND (@ProfesionalId IS NULL OR hc.ProfesionalId = @ProfesionalId)
        AND (@FechaDesde IS NULL OR hc.FechaConsulta >= @FechaDesde)
        AND (@FechaHasta IS NULL OR hc.FechaConsulta <= @FechaHasta)
        AND (@Diagnostico IS NULL OR hc.Diagnostico LIKE '%' + @Diagnostico + '%')
        AND (@SoloActivas = 0 OR hc.Activo = 1)
    ORDER BY hc.FechaConsulta DESC
    OFFSET @Offset ROWS
    FETCH NEXT @PageSize ROWS ONLY;

    -- Total de registros
    SELECT COUNT(*)
    FROM HistoriasClinicas hc
    WHERE
        (@PacienteId IS NULL OR hc.PacienteId = @PacienteId)
        AND (@ProfesionalId IS NULL OR hc.ProfesionalId = @ProfesionalId)
        AND (@FechaDesde IS NULL OR hc.FechaConsulta >= @FechaDesde)
        AND (@FechaHasta IS NULL OR hc.FechaConsulta <= @FechaHasta)
        AND (@Diagnostico IS NULL OR hc.Diagnostico LIKE '%' + @Diagnostico + '%')
        AND (@SoloActivas = 0 OR hc.Activo = 1);
END
GO

PRINT 'Stored Procedure SP_ListarHistoriasClinicas creado.';
GO

-- ===============================================
-- SP: SP_ObtenerHistoriaClinicaPorId
-- Descripción: Obtiene una historia clínica por ID
-- ===============================================

IF OBJECT_ID('SP_ObtenerHistoriaClinicaPorId', 'P') IS NOT NULL
    DROP PROCEDURE SP_ObtenerHistoriaClinicaPorId;
GO

CREATE PROCEDURE SP_ObtenerHistoriaClinicaPorId
    @HistoriaClinicaId INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        hc.HistoriaClinicaId,
        hc.PacienteId,
        hc.ProfesionalId,
        hc.FechaConsulta,
        hc.MotivoConsulta,
        hc.Anamnesis,
        hc.ExamenFisico,
        hc.Diagnostico,
        hc.Tratamiento,
        hc.Observaciones,
        hc.Peso,
        hc.Altura,
        hc.PresionArterial,
        hc.Temperatura,
        hc.FrecuenciaCardiaca,
        hc.Activo,
        hc.FechaAlta,

        -- Datos del paciente
        p.Nombre AS PacienteNombre,
        p.Apellido AS PacienteApellido,
        p.NumeroDocumento AS PacienteDocumento,

        -- Datos del profesional
        pr.Nombre AS ProfesionalNombre,
        pr.Apellido AS ProfesionalApellido,
        pr.Matricula AS ProfesionalMatricula

    FROM HistoriasClinicas hc
    INNER JOIN Pacientes p ON hc.PacienteId = p.PacienteId
    INNER JOIN Profesionales pr ON hc.ProfesionalId = pr.ProfesionalId
    WHERE hc.HistoriaClinicaId = @HistoriaClinicaId;
END
GO

PRINT 'Stored Procedure SP_ObtenerHistoriaClinicaPorId creado.';
GO

-- ===============================================
-- SP: SP_ObtenerHistorialPaciente
-- Descripción: Obtiene el historial completo de un paciente
-- ===============================================

IF OBJECT_ID('SP_ObtenerHistorialPaciente', 'P') IS NOT NULL
    DROP PROCEDURE SP_ObtenerHistorialPaciente;
GO

CREATE PROCEDURE SP_ObtenerHistorialPaciente
    @PacienteId INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        hc.HistoriaClinicaId,
        hc.PacienteId,
        hc.ProfesionalId,
        hc.FechaConsulta,
        hc.MotivoConsulta,
        hc.Anamnesis,
        hc.ExamenFisico,
        hc.Diagnostico,
        hc.Tratamiento,
        hc.Observaciones,
        hc.Peso,
        hc.Altura,
        hc.PresionArterial,
        hc.Temperatura,
        hc.FrecuenciaCardiaca,
        hc.Activo,
        hc.FechaAlta,

        -- Datos del paciente
        p.Nombre AS PacienteNombre,
        p.Apellido AS PacienteApellido,
        p.NumeroDocumento AS PacienteDocumento,

        -- Datos del profesional
        pr.Nombre AS ProfesionalNombre,
        pr.Apellido AS ProfesionalApellido,
        pr.Matricula AS ProfesionalMatricula

    FROM HistoriasClinicas hc
    INNER JOIN Pacientes p ON hc.PacienteId = p.PacienteId
    INNER JOIN Profesionales pr ON hc.ProfesionalId = pr.ProfesionalId
    WHERE hc.PacienteId = @PacienteId
        AND hc.Activo = 1
    ORDER BY hc.FechaConsulta DESC;
END
GO

PRINT 'Stored Procedure SP_ObtenerHistorialPaciente creado.';
GO

-- ===============================================
-- SP: SP_CrearHistoriaClinica
-- Descripción: Crea una nueva historia clínica
-- ===============================================

IF OBJECT_ID('SP_CrearHistoriaClinica', 'P') IS NOT NULL
    DROP PROCEDURE SP_CrearHistoriaClinica;
GO

CREATE PROCEDURE SP_CrearHistoriaClinica
    @PacienteId INT,
    @ProfesionalId INT,
    @FechaConsulta DATETIME,
    @MotivoConsulta NVARCHAR(500),
    @Anamnesis NVARCHAR(MAX) = NULL,
    @ExamenFisico NVARCHAR(MAX) = NULL,
    @Diagnostico NVARCHAR(1000),
    @Tratamiento NVARCHAR(MAX) = NULL,
    @Observaciones NVARCHAR(MAX) = NULL,
    @Peso DECIMAL(5,2) = NULL,
    @Altura DECIMAL(5,2) = NULL,
    @PresionArterial DECIMAL(5,2) = NULL,
    @Temperatura DECIMAL(4,2) = NULL,
    @FrecuenciaCardiaca DECIMAL(5,2) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
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

        -- Insertar la historia clínica
        INSERT INTO HistoriasClinicas (
            PacienteId, ProfesionalId, FechaConsulta, MotivoConsulta,
            Anamnesis, ExamenFisico, Diagnostico, Tratamiento, Observaciones,
            Peso, Altura, PresionArterial, Temperatura, FrecuenciaCardiaca,
            Activo, FechaAlta
        )
        VALUES (
            @PacienteId, @ProfesionalId, @FechaConsulta, @MotivoConsulta,
            @Anamnesis, @ExamenFisico, @Diagnostico, @Tratamiento, @Observaciones,
            @Peso, @Altura, @PresionArterial, @Temperatura, @FrecuenciaCardiaca,
            1, GETDATE()
        );

        -- Retornar el ID generado
        SELECT SCOPE_IDENTITY() AS HistoriaClinicaId;
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO

PRINT 'Stored Procedure SP_CrearHistoriaClinica creado.';
GO

-- ===============================================
-- SP: SP_ActualizarHistoriaClinica
-- Descripción: Actualiza una historia clínica existente
-- ===============================================

IF OBJECT_ID('SP_ActualizarHistoriaClinica', 'P') IS NOT NULL
    DROP PROCEDURE SP_ActualizarHistoriaClinica;
GO

CREATE PROCEDURE SP_ActualizarHistoriaClinica
    @HistoriaClinicaId INT,
    @PacienteId INT,
    @ProfesionalId INT,
    @FechaConsulta DATETIME,
    @MotivoConsulta NVARCHAR(500),
    @Anamnesis NVARCHAR(MAX) = NULL,
    @ExamenFisico NVARCHAR(MAX) = NULL,
    @Diagnostico NVARCHAR(1000),
    @Tratamiento NVARCHAR(MAX) = NULL,
    @Observaciones NVARCHAR(MAX) = NULL,
    @Peso DECIMAL(5,2) = NULL,
    @Altura DECIMAL(5,2) = NULL,
    @PresionArterial DECIMAL(5,2) = NULL,
    @Temperatura DECIMAL(4,2) = NULL,
    @FrecuenciaCardiaca DECIMAL(5,2) = NULL,
    @Activo BIT
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        -- Validar que la historia clínica existe
        IF NOT EXISTS (SELECT 1 FROM HistoriasClinicas WHERE HistoriaClinicaId = @HistoriaClinicaId)
        BEGIN
            RAISERROR('La historia clínica no existe.', 16, 1);
            RETURN;
        END

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

        -- Actualizar la historia clínica
        UPDATE HistoriasClinicas
        SET
            PacienteId = @PacienteId,
            ProfesionalId = @ProfesionalId,
            FechaConsulta = @FechaConsulta,
            MotivoConsulta = @MotivoConsulta,
            Anamnesis = @Anamnesis,
            ExamenFisico = @ExamenFisico,
            Diagnostico = @Diagnostico,
            Tratamiento = @Tratamiento,
            Observaciones = @Observaciones,
            Peso = @Peso,
            Altura = @Altura,
            PresionArterial = @PresionArterial,
            Temperatura = @Temperatura,
            FrecuenciaCardiaca = @FrecuenciaCardiaca,
            Activo = @Activo,
            FechaModificacion = GETDATE()
        WHERE HistoriaClinicaId = @HistoriaClinicaId;

    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO

PRINT 'Stored Procedure SP_ActualizarHistoriaClinica creado.';
GO

-- ===============================================
-- SP: SP_EliminarHistoriaClinica
-- Descripción: Elimina (desactiva) una historia clínica
-- ===============================================

IF OBJECT_ID('SP_EliminarHistoriaClinica', 'P') IS NOT NULL
    DROP PROCEDURE SP_EliminarHistoriaClinica;
GO

CREATE PROCEDURE SP_EliminarHistoriaClinica
    @HistoriaClinicaId INT
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        -- Validar que la historia clínica existe
        IF NOT EXISTS (SELECT 1 FROM HistoriasClinicas WHERE HistoriaClinicaId = @HistoriaClinicaId)
        BEGIN
            RAISERROR('La historia clínica no existe.', 16, 1);
            RETURN;
        END

        -- Realizar eliminación lógica
        UPDATE HistoriasClinicas
        SET
            Activo = 0,
            FechaModificacion = GETDATE()
        WHERE HistoriaClinicaId = @HistoriaClinicaId;

    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO

PRINT 'Stored Procedure SP_EliminarHistoriaClinica creado.';
GO

-- ===============================================
-- SCRIPT COMPLETADO
-- ===============================================

PRINT '';
PRINT '========================================';
PRINT 'SCRIPT COMPLETADO EXITOSAMENTE';
PRINT '========================================';
PRINT 'Tabla creada: HistoriasClinicas';
PRINT 'Stored Procedures creados: 6';
PRINT '  - SP_ListarHistoriasClinicas';
PRINT '  - SP_ObtenerHistoriaClinicaPorId';
PRINT '  - SP_ObtenerHistorialPaciente';
PRINT '  - SP_CrearHistoriaClinica';
PRINT '  - SP_ActualizarHistoriaClinica';
PRINT '  - SP_EliminarHistoriaClinica';
PRINT '========================================';
GO
