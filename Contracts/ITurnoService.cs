using Entities;
using System;
using System.Collections.Generic;

namespace Contracts
{
    /// <summary>
    /// Interfaz para el servicio de turnos
    /// </summary>
    public interface ITurnoService
    {
        /// <summary>
        /// Lista turnos aplicando filtros
        /// </summary>
        List<TurnoDto> Listar(TurnoFiltroDto filtro, out int totalRegistros);

        /// <summary>
        /// Obtiene un turno por su ID
        /// </summary>
        TurnoDto ObtenerPorId(int turnoId);

        /// <summary>
        /// Obtiene los turnos de un paciente
        /// </summary>
        List<TurnoDto> ObtenerTurnosPorPaciente(int pacienteId);

        /// <summary>
        /// Obtiene la agenda de un profesional para una fecha
        /// </summary>
        List<TurnoDto> ObtenerAgendaProfesional(int profesionalId, DateTime fecha);

        /// <summary>
        /// Crea un nuevo turno
        /// </summary>
        (bool exitoso, string mensaje, int turnoId) Crear(CrearTurnoDto dto);

        /// <summary>
        /// Actualiza un turno existente
        /// </summary>
        (bool exitoso, string mensaje) Actualizar(ActualizarTurnoDto dto);

        /// <summary>
        /// Cancela un turno
        /// </summary>
        (bool exitoso, string mensaje) Cancelar(int turnoId, string motivoCancelacion);

        /// <summary>
        /// Cambia el estado de un turno
        /// </summary>
        (bool exitoso, string mensaje) CambiarEstado(int turnoId, string nuevoEstado);
    }
}
