-- =============================================
-- TABLA: DisponibilidadesAgenda
-- Almacena las disponibilidades horarias de los profesionales
-- =============================================

USE GECO;
GO

-- Verificar si la tabla existe y eliminarla si es necesario
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DisponibilidadesAgenda]') AND type in (N'U'))
BEGIN
    PRINT 'La tabla DisponibilidadesAgenda ya existe. Eliminando...';
    DROP TABLE [dbo].[DisponibilidadesAgenda];
END
GO

-- Crear la tabla DisponibilidadesAgenda
CREATE TABLE [dbo].[DisponibilidadesAgenda] (
    [DisponibilidadAgendaId] INT IDENTITY(1,1) NOT NULL,
    [ProfesionalId] INT NOT NULL,
    [DiaSemana] INT NOT NULL, -- 1=Lunes, 2=Martes, ..., 7=Domingo
    [HoraInicio] TIME NOT NULL,
    [HoraFin] TIME NOT NULL,
    [IntervaloMinutos] INT NOT NULL, -- Duración de cada turno en minutos (ej: 15, 30, 60)
    [FechaVigenciaDesde] DATE NOT NULL,
    [FechaVigenciaHasta] DATE NULL, -- NULL = vigencia indefinida
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
GO

-- Crear índices para mejorar el rendimiento
CREATE NONCLUSTERED INDEX [IX_DisponibilidadesAgenda_ProfesionalId]
    ON [dbo].[DisponibilidadesAgenda] ([ProfesionalId]);
GO

CREATE NONCLUSTERED INDEX [IX_DisponibilidadesAgenda_DiaSemana]
    ON [dbo].[DisponibilidadesAgenda] ([DiaSemana]);
GO

CREATE NONCLUSTERED INDEX [IX_DisponibilidadesAgenda_Vigencia]
    ON [dbo].[DisponibilidadesAgenda] ([FechaVigenciaDesde], [FechaVigenciaHasta]);
GO

PRINT 'Tabla DisponibilidadesAgenda creada exitosamente.';
GO

-- Insertar datos de ejemplo (opcional)
-- Descomentar para agregar datos de prueba
/*
INSERT INTO DisponibilidadesAgenda (ProfesionalId, DiaSemana, HoraInicio, HoraFin, IntervaloMinutos, FechaVigenciaDesde)
VALUES
    (1, 1, '08:00', '12:00', 30, '2025-01-01'), -- Lunes 8:00-12:00, turnos de 30 min
    (1, 1, '14:00', '18:00', 30, '2025-01-01'), -- Lunes 14:00-18:00, turnos de 30 min
    (1, 3, '08:00', '12:00', 30, '2025-01-01'), -- Miércoles 8:00-12:00, turnos de 30 min
    (1, 3, '14:00', '18:00', 30, '2025-01-01'), -- Miércoles 14:00-18:00, turnos de 30 min
    (1, 5, '08:00', '12:00', 30, '2025-01-01'); -- Viernes 8:00-12:00, turnos de 30 min

PRINT 'Datos de ejemplo insertados.';
*/
GO
