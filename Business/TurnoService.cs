using Contracts;
using Data;
using Entities;
using System;
using System.Collections.Generic;

namespace Business
{
    /// <summary>
    /// Servicio de negocio para Turnos
    /// </summary>
    public class TurnoService : ITurnoService
    {
        private readonly TurnoData _turnoData;
        private readonly PacienteData _pacienteData;
        private readonly ProfesionalData _profesionalData;
        private readonly DisponibilidadAgendaData _disponibilidadAgendaData;

        public TurnoService(
            TurnoData turnoData,
            PacienteData pacienteData,
            ProfesionalData profesionalData,
            DisponibilidadAgendaData disponibilidadAgendaData)
        {
            _turnoData = turnoData;
            _pacienteData = pacienteData;
            _profesionalData = profesionalData;
            _disponibilidadAgendaData = disponibilidadAgendaData;
        }

        public List<TurnoDto> Listar(TurnoFiltroDto filtro, out int totalRegistros)
        {
            return _turnoData.Listar(filtro, out totalRegistros);
        }

        public TurnoDto ObtenerPorId(int turnoId)
        {
            return _turnoData.ObtenerPorId(turnoId);
        }

        public List<TurnoDto> ObtenerTurnosPorPaciente(int pacienteId)
        {
            return _turnoData.ObtenerTurnosPorPaciente(pacienteId);
        }

        public List<TurnoDto> ObtenerAgendaProfesional(int profesionalId, DateTime fecha)
        {
            return _turnoData.ObtenerAgendaProfesional(profesionalId, fecha);
        }

        public (bool exitoso, string mensaje, int turnoId) Crear(CrearTurnoDto dto)
        {
            try
            {
                // Validar datos obligatorios
                if (dto.PacienteId <= 0)
                    return (false, "Debe seleccionar un paciente", 0);

                if (dto.ProfesionalId <= 0)
                    return (false, "Debe seleccionar un profesional", 0);

                if (string.IsNullOrWhiteSpace(dto.MotivoConsulta))
                    return (false, "El motivo de consulta es obligatorio", 0);

                if (dto.DuracionMinutos <= 0 || dto.DuracionMinutos > 480)
                    return (false, "La duración debe estar entre 1 y 480 minutos (8 horas)", 0);

                // Validar fecha futura o actual
                if (dto.FechaTurno.Date < DateTime.Today)
                    return (false, "La fecha del turno no puede ser anterior a hoy", 0);

                // Validar horario lógico (entre 6:00 y 22:00)
                if (dto.HoraInicio < new TimeSpan(6, 0, 0) || dto.HoraInicio > new TimeSpan(22, 0, 0))
                    return (false, "El horario debe estar entre las 6:00 y las 22:00", 0);

                // Validar que el paciente existe y está activo
                var paciente = _pacienteData.ObtenerPorId(dto.PacienteId);
                if (paciente == null)
                    return (false, "El paciente seleccionado no existe", 0);

                if (!paciente.Activo)
                    return (false, "El paciente seleccionado está inactivo", 0);

                // Validar que el profesional existe y está activo
                var profesional = _profesionalData.ObtenerPorId(dto.ProfesionalId);
                if (profesional == null)
                    return (false, "El profesional seleccionado no existe", 0);

                if (!profesional.Activo)
                    return (false, "El profesional seleccionado está inactivo", 0);

                // Verificar que el horario esté dentro de las disponibilidades de agenda del profesional
                bool existeDisponibilidad = _disponibilidadAgendaData.VerificarDisponibilidad(
                    dto.ProfesionalId,
                    dto.FechaTurno,
                    dto.HoraInicio,
                    dto.DuracionMinutos
                );

                if (!existeDisponibilidad)
                {
                    return (false, "El horario seleccionado está fuera del horario de atención del profesional. " +
                                   "Consulte las disponibilidades de agenda configuradas.", 0);
                }

                // Verificar disponibilidad horaria (que no haya otro turno)
                bool hayDisponibilidad = _turnoData.VerificarDisponibilidad(
                    dto.ProfesionalId,
                    dto.FechaTurno,
                    dto.HoraInicio,
                    dto.DuracionMinutos,
                    null
                );

                if (!hayDisponibilidad)
                    return (false, "El horario seleccionado no está disponible. Ya existe un turno en ese horario.", 0);

                // Validar estado
                string[] estadosValidos = { "Pendiente", "Confirmado" };
                if (!Array.Exists(estadosValidos, e => e == dto.Estado))
                    dto.Estado = "Pendiente"; // Por defecto

                // Crear el turno
                int turnoId = _turnoData.Crear(dto);

                if (turnoId > 0)
                {
                    return (true, "Turno creado exitosamente", turnoId);
                }
                else
                {
                    return (false, "Error al crear el turno", 0);
                }
            }
            catch (Exception ex)
            {
                return (false, $"Error al crear el turno: {ex.Message}", 0);
            }
        }

        public (bool exitoso, string mensaje) Actualizar(ActualizarTurnoDto dto)
        {
            try
            {
                // Validar que el turno existe
                var turnoExistente = _turnoData.ObtenerPorId(dto.TurnoId);
                if (turnoExistente == null)
                    return (false, "El turno no existe");

                // Validar datos obligatorios
                if (dto.PacienteId <= 0)
                    return (false, "Debe seleccionar un paciente");

                if (dto.ProfesionalId <= 0)
                    return (false, "Debe seleccionar un profesional");

                if (string.IsNullOrWhiteSpace(dto.MotivoConsulta))
                    return (false, "El motivo de consulta es obligatorio");

                if (dto.DuracionMinutos <= 0 || dto.DuracionMinutos > 480)
                    return (false, "La duración debe estar entre 1 y 480 minutos (8 horas)");

                // Validar horario lógico
                if (dto.HoraInicio < new TimeSpan(6, 0, 0) || dto.HoraInicio > new TimeSpan(22, 0, 0))
                    return (false, "El horario debe estar entre las 6:00 y las 22:00");

                // Validar que el paciente existe y está activo
                var paciente = _pacienteData.ObtenerPorId(dto.PacienteId);
                if (paciente == null || !paciente.Activo)
                    return (false, "El paciente seleccionado no existe o está inactivo");

                // Validar que el profesional existe y está activo
                var profesional = _profesionalData.ObtenerPorId(dto.ProfesionalId);
                if (profesional == null || !profesional.Activo)
                    return (false, "El profesional seleccionado no existe o está inactivo");

                // Verificar disponibilidad horaria (excluyendo el turno actual)
                bool hayDisponibilidad = _turnoData.VerificarDisponibilidad(
                    dto.ProfesionalId,
                    dto.FechaTurno,
                    dto.HoraInicio,
                    dto.DuracionMinutos,
                    dto.TurnoId
                );

                if (!hayDisponibilidad)
                    return (false, "El horario seleccionado no está disponible");

                // Validar estado
                string[] estadosValidos = { "Pendiente", "Confirmado", "EnCurso", "Completado", "Cancelado", "Ausente" };
                if (!Array.Exists(estadosValidos, e => e == dto.Estado))
                    return (false, "Estado inválido");

                // Si se cancela, validar motivo
                if (dto.Estado == "Cancelado" && string.IsNullOrWhiteSpace(dto.MotivoCancelacion))
                    return (false, "Debe especificar el motivo de cancelación");

                // Actualizar el turno
                bool actualizado = _turnoData.Actualizar(dto);

                if (actualizado)
                {
                    return (true, "Turno actualizado exitosamente");
                }
                else
                {
                    return (false, "Error al actualizar el turno");
                }
            }
            catch (Exception ex)
            {
                return (false, $"Error al actualizar el turno: {ex.Message}");
            }
        }

        public (bool exitoso, string mensaje) Cancelar(int turnoId, string motivoCancelacion)
        {
            try
            {
                // Validar que el turno existe
                var turno = _turnoData.ObtenerPorId(turnoId);
                if (turno == null)
                    return (false, "El turno no existe");

                // Validar que no esté ya cancelado
                if (turno.Estado == "Cancelado")
                    return (false, "El turno ya está cancelado");

                // Validar que no esté completado
                if (turno.Estado == "Completado")
                    return (false, "No se puede cancelar un turno completado");

                // Validar motivo de cancelación
                if (string.IsNullOrWhiteSpace(motivoCancelacion))
                    return (false, "Debe especificar el motivo de cancelación");

                // Cancelar el turno
                bool cancelado = _turnoData.Cancelar(turnoId, motivoCancelacion);

                if (cancelado)
                {
                    return (true, "Turno cancelado exitosamente");
                }
                else
                {
                    return (false, "Error al cancelar el turno");
                }
            }
            catch (Exception ex)
            {
                return (false, $"Error al cancelar el turno: {ex.Message}");
            }
        }

        public (bool exitoso, string mensaje) CambiarEstado(int turnoId, string nuevoEstado)
        {
            try
            {
                // Validar que el turno existe
                var turno = _turnoData.ObtenerPorId(turnoId);
                if (turno == null)
                    return (false, "El turno no existe");

                // Validar estado
                string[] estadosValidos = { "Pendiente", "Confirmado", "EnCurso", "Completado", "Cancelado", "Ausente" };
                if (!Array.Exists(estadosValidos, e => e == nuevoEstado))
                    return (false, "Estado inválido");

                // Validar transiciones lógicas
                if (turno.Estado == "Completado" && nuevoEstado != "Completado")
                    return (false, "No se puede cambiar el estado de un turno completado");

                if (turno.Estado == "Cancelado" && nuevoEstado != "Cancelado")
                    return (false, "No se puede cambiar el estado de un turno cancelado");

                // Crear DTO de actualización con solo el cambio de estado
                var dto = new ActualizarTurnoDto
                {
                    TurnoId = turno.TurnoId,
                    PacienteId = turno.PacienteId,
                    ProfesionalId = turno.ProfesionalId,
                    FechaTurno = turno.FechaTurno,
                    HoraInicio = turno.HoraInicio,
                    DuracionMinutos = turno.DuracionMinutos,
                    MotivoConsulta = turno.MotivoConsulta,
                    Estado = nuevoEstado,
                    MotivoCancelacion = turno.MotivoCancelacion,
                    Observaciones = turno.Observaciones,
                    Activo = turno.Activo
                };

                bool actualizado = _turnoData.Actualizar(dto);

                if (actualizado)
                {
                    return (true, $"Estado cambiado a '{nuevoEstado}' exitosamente");
                }
                else
                {
                    return (false, "Error al cambiar el estado del turno");
                }
            }
            catch (Exception ex)
            {
                return (false, $"Error al cambiar el estado: {ex.Message}");
            }
        }
    }
}
