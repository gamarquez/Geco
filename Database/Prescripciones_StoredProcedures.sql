-- ===============================================
-- SCRIPT DE BASE DE DATOS - PRESCRIPCIONES MÉDICAS
-- Sistema GECO - Gestión de Consultorios Médicos
-- ===============================================

USE GECO;
GO

-- ===============================================
-- TABLA: Prescripciones
-- ===============================================

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Prescripciones')
BEGIN
    CREATE TABLE Prescripciones (
        PrescripcionId INT PRIMARY KEY IDENTITY(1,1),
        PacienteId INT NOT NULL,
        ProfesionalId INT NOT NULL,
        TurnoId INT NULL,
        HistoriaClinicaId INT NULL,
        FechaPrescripcion DATETIME NOT NULL DEFAULT GETDATE(),
        Diagnostico NVARCHAR(1000) NOT NULL,
        Indicaciones NVARCHAR(MAX) NULL,
        Vigente BIT NOT NULL DEFAULT 1,
        FechaVencimiento DATETIME NULL,

        -- Auditoría
        Activo BIT NOT NULL DEFAULT 1,
        FechaAlta DATETIME NOT NULL DEFAULT GETDATE(),
        FechaModificacion DATETIME NULL,

        -- Claves foráneas
        CONSTRAINT FK_Prescripciones_Pacientes FOREIGN KEY (PacienteId)
            REFERENCES Pacientes(PacienteId),
        CONSTRAINT FK_Prescripciones_Profesionales FOREIGN KEY (ProfesionalId)
            REFERENCES Profesionales(ProfesionalId),
        CONSTRAINT FK_Prescripciones_Turnos FOREIGN KEY (TurnoId)
            REFERENCES Turnos(TurnoId),
        CONSTRAINT FK_Prescripciones_HistoriasClinicas FOREIGN KEY (HistoriaClinicaId)
            REFERENCES HistoriasClinicas(HistoriaClinicaId)
    );

    -- Índices
    CREATE INDEX IX_Prescripciones_PacienteId ON Prescripciones(PacienteId);
    CREATE INDEX IX_Prescripciones_ProfesionalId ON Prescripciones(ProfesionalId);
    CREATE INDEX IX_Prescripciones_FechaPrescripcion ON Prescripciones(FechaPrescripcion);
    CREATE INDEX IX_Prescripciones_Vigente ON Prescripciones(Vigente);

    PRINT 'Tabla Prescripciones creada exitosamente.';
END
ELSE
BEGIN
    PRINT 'La tabla Prescripciones ya existe.';
END
GO

-- ===============================================
-- TABLA: ItemsPrescripcion
-- ===============================================

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ItemsPrescripcion')
BEGIN
    CREATE TABLE ItemsPrescripcion (
        ItemPrescripcionId INT PRIMARY KEY IDENTITY(1,1),
        PrescripcionId INT NOT NULL,
        Medicamento NVARCHAR(200) NOT NULL,
        PrincipioActivo NVARCHAR(200) NULL,
        Presentacion NVARCHAR(100) NULL,
        Dosis NVARCHAR(100) NOT NULL,
        Frecuencia NVARCHAR(100) NOT NULL,
        Duracion NVARCHAR(100) NOT NULL,
        ViaAdministracion NVARCHAR(50) NULL,
        IndicacionesEspeciales NVARCHAR(500) NULL,
        Orden INT NOT NULL DEFAULT 1,

        -- Clave foránea
        CONSTRAINT FK_ItemsPrescripcion_Prescripciones FOREIGN KEY (PrescripcionId)
            REFERENCES Prescripciones(PrescripcionId) ON DELETE CASCADE
    );

    -- Índice
    CREATE INDEX IX_ItemsPrescripcion_PrescripcionId ON ItemsPrescripcion(PrescripcionId);

    PRINT 'Tabla ItemsPrescripcion creada exitosamente.';
END
ELSE
BEGIN
    PRINT 'La tabla ItemsPrescripcion ya existe.';
END
GO

-- ===============================================
-- SP: SP_ListarPrescripciones
-- ===============================================

IF OBJECT_ID('SP_ListarPrescripciones', 'P') IS NOT NULL
    DROP PROCEDURE SP_ListarPrescripciones;
GO

CREATE PROCEDURE SP_ListarPrescripciones
    @PacienteId INT = NULL,
    @ProfesionalId INT = NULL,
    @FechaDesde DATE = NULL,
    @FechaHasta DATE = NULL,
    @SoloVigentes BIT = NULL,
    @PageNumber INT = 1,
    @PageSize INT = 20,
    @SoloActivas BIT = 1
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;

    SELECT
        pr.PrescripcionId,
        pr.PacienteId,
        pr.ProfesionalId,
        pr.TurnoId,
        pr.HistoriaClinicaId,
        pr.FechaPrescripcion,
        pr.Diagnostico,
        pr.Indicaciones,
        pr.Vigente,
        pr.FechaVencimiento,
        pr.Activo,
        pr.FechaAlta,
        pr.FechaModificacion,

        -- Datos del paciente
        p.Nombre AS PacienteNombre,
        p.Apellido AS PacienteApellido,
        p.NumeroDocumento AS PacienteDocumento,
        p.FechaNacimiento AS PacienteFechaNacimiento,

        -- Datos del profesional
        pf.Nombre AS ProfesionalNombre,
        pf.Apellido AS ProfesionalApellido,
        pf.Matricula AS ProfesionalMatricula,
        pf.Especialidad AS ProfesionalEspecialidad

    FROM Prescripciones pr
    INNER JOIN Pacientes p ON pr.PacienteId = p.PacienteId
    INNER JOIN Profesionales pf ON pr.ProfesionalId = pf.ProfesionalId
    WHERE
        (@PacienteId IS NULL OR pr.PacienteId = @PacienteId)
        AND (@ProfesionalId IS NULL OR pr.ProfesionalId = @ProfesionalId)
        AND (@FechaDesde IS NULL OR CAST(pr.FechaPrescripcion AS DATE) >= @FechaDesde)
        AND (@FechaHasta IS NULL OR CAST(pr.FechaPrescripcion AS DATE) <= @FechaHasta)
        AND (@SoloVigentes IS NULL OR pr.Vigente = @SoloVigentes)
        AND (@SoloActivas = 0 OR pr.Activo = 1)
    ORDER BY pr.FechaPrescripcion DESC
    OFFSET @Offset ROWS
    FETCH NEXT @PageSize ROWS ONLY;

    -- Total de registros
    SELECT COUNT(*)
    FROM Prescripciones pr
    WHERE
        (@PacienteId IS NULL OR pr.PacienteId = @PacienteId)
        AND (@ProfesionalId IS NULL OR pr.ProfesionalId = @ProfesionalId)
        AND (@FechaDesde IS NULL OR CAST(pr.FechaPrescripcion AS DATE) >= @FechaDesde)
        AND (@FechaHasta IS NULL OR CAST(pr.FechaPrescripcion AS DATE) <= @FechaHasta)
        AND (@SoloVigentes IS NULL OR pr.Vigente = @SoloVigentes)
        AND (@SoloActivas = 0 OR pr.Activo = 1);
END
GO

PRINT 'Stored Procedure SP_ListarPrescripciones creado.';
GO

-- ===============================================
-- SP: SP_ObtenerPrescripcionPorId
-- ===============================================

IF OBJECT_ID('SP_ObtenerPrescripcionPorId', 'P') IS NOT NULL
    DROP PROCEDURE SP_ObtenerPrescripcionPorId;
GO

CREATE PROCEDURE SP_ObtenerPrescripcionPorId
    @PrescripcionId INT
AS
BEGIN
    SET NOCOUNT ON;

    -- Datos de la prescripción
    SELECT
        pr.PrescripcionId,
        pr.PacienteId,
        pr.ProfesionalId,
        pr.TurnoId,
        pr.HistoriaClinicaId,
        pr.FechaPrescripcion,
        pr.Diagnostico,
        pr.Indicaciones,
        pr.Vigente,
        pr.FechaVencimiento,
        pr.Activo,
        pr.FechaAlta,
        pr.FechaModificacion,

        -- Datos del paciente
        p.Nombre AS PacienteNombre,
        p.Apellido AS PacienteApellido,
        p.NumeroDocumento AS PacienteDocumento,
        p.FechaNacimiento AS PacienteFechaNacimiento,

        -- Datos del profesional
        pf.Nombre AS ProfesionalNombre,
        pf.Apellido AS ProfesionalApellido,
        pf.Matricula AS ProfesionalMatricula,
        pf.Especialidad AS ProfesionalEspecialidad

    FROM Prescripciones pr
    INNER JOIN Pacientes p ON pr.PacienteId = p.PacienteId
    INNER JOIN Profesionales pf ON pr.ProfesionalId = pf.ProfesionalId
    WHERE pr.PrescripcionId = @PrescripcionId;

    -- Items de la prescripción
    SELECT
        ItemPrescripcionId,
        PrescripcionId,
        Medicamento,
        PrincipioActivo,
        Presentacion,
        Dosis,
        Frecuencia,
        Duracion,
        ViaAdministracion,
        IndicacionesEspeciales,
        Orden
    FROM ItemsPrescripcion
    WHERE PrescripcionId = @PrescripcionId
    ORDER BY Orden;
END
GO

PRINT 'Stored Procedure SP_ObtenerPrescripcionPorId creado.';
GO

-- ===============================================
-- SP: SP_ObtenerPrescripcionesPorPaciente
-- ===============================================

IF OBJECT_ID('SP_ObtenerPrescripcionesPorPaciente', 'P') IS NOT NULL
    DROP PROCEDURE SP_ObtenerPrescripcionesPorPaciente;
GO

CREATE PROCEDURE SP_ObtenerPrescripcionesPorPaciente
    @PacienteId INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        pr.PrescripcionId,
        pr.PacienteId,
        pr.ProfesionalId,
        pr.TurnoId,
        pr.HistoriaClinicaId,
        pr.FechaPrescripcion,
        pr.Diagnostico,
        pr.Indicaciones,
        pr.Vigente,
        pr.FechaVencimiento,
        pr.Activo,
        pr.FechaAlta,
        pr.FechaModificacion,

        -- Datos del paciente
        p.Nombre AS PacienteNombre,
        p.Apellido AS PacienteApellido,
        p.NumeroDocumento AS PacienteDocumento,
        p.FechaNacimiento AS PacienteFechaNacimiento,

        -- Datos del profesional
        pf.Nombre AS ProfesionalNombre,
        pf.Apellido AS ProfesionalApellido,
        pf.Matricula AS ProfesionalMatricula,
        pf.Especialidad AS ProfesionalEspecialidad

    FROM Prescripciones pr
    INNER JOIN Pacientes p ON pr.PacienteId = p.PacienteId
    INNER JOIN Profesionales pf ON pr.ProfesionalId = pf.ProfesionalId
    WHERE pr.PacienteId = @PacienteId
        AND pr.Activo = 1
    ORDER BY pr.FechaPrescripcion DESC;
END
GO

PRINT 'Stored Procedure SP_ObtenerPrescripcionesPorPaciente creado.';
GO

-- ===============================================
-- SP: SP_CrearPrescripcion
-- ===============================================

IF OBJECT_ID('SP_CrearPrescripcion', 'P') IS NOT NULL
    DROP PROCEDURE SP_CrearPrescripcion;
GO

CREATE PROCEDURE SP_CrearPrescripcion
    @PacienteId INT,
    @ProfesionalId INT,
    @TurnoId INT = NULL,
    @HistoriaClinicaId INT = NULL,
    @FechaPrescripcion DATETIME,
    @Diagnostico NVARCHAR(1000),
    @Indicaciones NVARCHAR(MAX) = NULL,
    @FechaVencimiento DATETIME = NULL
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        DECLARE @Vigente BIT = 1;

        -- Determinar si está vigente
        IF @FechaVencimiento IS NOT NULL AND @FechaVencimiento < GETDATE()
            SET @Vigente = 0;

        INSERT INTO Prescripciones (
            PacienteId, ProfesionalId, TurnoId, HistoriaClinicaId,
            FechaPrescripcion, Diagnostico, Indicaciones,
            Vigente, FechaVencimiento, Activo, FechaAlta
        )
        VALUES (
            @PacienteId, @ProfesionalId, @TurnoId, @HistoriaClinicaId,
            @FechaPrescripcion, @Diagnostico, @Indicaciones,
            @Vigente, @FechaVencimiento, 1, GETDATE()
        );

        SELECT SCOPE_IDENTITY() AS PrescripcionId;
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO

PRINT 'Stored Procedure SP_CrearPrescripcion creado.';
GO

-- ===============================================
-- SP: SP_CrearItemPrescripcion
-- ===============================================

IF OBJECT_ID('SP_CrearItemPrescripcion', 'P') IS NOT NULL
    DROP PROCEDURE SP_CrearItemPrescripcion;
GO

CREATE PROCEDURE SP_CrearItemPrescripcion
    @PrescripcionId INT,
    @Medicamento NVARCHAR(200),
    @PrincipioActivo NVARCHAR(200) = NULL,
    @Presentacion NVARCHAR(100) = NULL,
    @Dosis NVARCHAR(100),
    @Frecuencia NVARCHAR(100),
    @Duracion NVARCHAR(100),
    @ViaAdministracion NVARCHAR(50) = NULL,
    @IndicacionesEspeciales NVARCHAR(500) = NULL,
    @Orden INT
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO ItemsPrescripcion (
        PrescripcionId, Medicamento, PrincipioActivo, Presentacion,
        Dosis, Frecuencia, Duracion, ViaAdministracion,
        IndicacionesEspeciales, Orden
    )
    VALUES (
        @PrescripcionId, @Medicamento, @PrincipioActivo, @Presentacion,
        @Dosis, @Frecuencia, @Duracion, @ViaAdministracion,
        @IndicacionesEspeciales, @Orden
    );
END
GO

PRINT 'Stored Procedure SP_CrearItemPrescripcion creado.';
GO

-- ===============================================
-- SP: SP_AnularPrescripcion
-- ===============================================

IF OBJECT_ID('SP_AnularPrescripcion', 'P') IS NOT NULL
    DROP PROCEDURE SP_AnularPrescripcion;
GO

CREATE PROCEDURE SP_AnularPrescripcion
    @PrescripcionId INT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE Prescripciones
    SET
        Activo = 0,
        Vigente = 0,
        FechaModificacion = GETDATE()
    WHERE PrescripcionId = @PrescripcionId;
END
GO

PRINT 'Stored Procedure SP_AnularPrescripcion creado.';
GO

-- ===============================================
-- SCRIPT COMPLETADO
-- ===============================================

PRINT '';
PRINT '========================================';
PRINT 'SCRIPT COMPLETADO EXITOSAMENTE';
PRINT '========================================';
PRINT 'Tablas creadas: Prescripciones, ItemsPrescripcion';
PRINT 'Stored Procedures creados: 6';
PRINT '  - SP_ListarPrescripciones';
PRINT '  - SP_ObtenerPrescripcionPorId';
PRINT '  - SP_ObtenerPrescripcionesPorPaciente';
PRINT '  - SP_CrearPrescripcion';
PRINT '  - SP_CrearItemPrescripcion';
PRINT '  - SP_AnularPrescripcion';
PRINT '========================================';
GO
