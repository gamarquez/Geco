using Contracts;
using Data;
using Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Business
{
    /// <summary>
    /// Servicio de negocio para Disponibilidades de Agenda
    /// </summary>
    public class DisponibilidadAgendaService : IDisponibilidadAgendaService
    {
        private readonly DisponibilidadAgendaData _disponibilidadAgendaData;
        private readonly ProfesionalData _profesionalData;

        public DisponibilidadAgendaService(
            DisponibilidadAgendaData disponibilidadAgendaData,
            ProfesionalData profesionalData)
        {
            _disponibilidadAgendaData = disponibilidadAgendaData;
            _profesionalData = profesionalData;
        }

        public List<DisponibilidadAgendaDto> Listar(DisponibilidadAgendaFiltroDto filtro, out int totalRegistros)
        {
            return _disponibilidadAgendaData.Listar(filtro, out totalRegistros);
        }

        public DisponibilidadAgendaDto ObtenerPorId(int disponibilidadAgendaId)
        {
            return _disponibilidadAgendaData.ObtenerPorId(disponibilidadAgendaId);
        }

        public List<DisponibilidadAgendaDto> ObtenerPorProfesional(int profesionalId, bool soloVigentes = true)
        {
            return _disponibilidadAgendaData.ObtenerPorProfesional(profesionalId, soloVigentes);
        }

        public (bool exitoso, string mensaje, int disponibilidadAgendaId) Crear(CrearDisponibilidadAgendaDto dto)
        {
            try
            {
                // Validar profesional
                if (dto.ProfesionalId <= 0)
                    return (false, "Debe seleccionar un profesional", 0);

                var profesional = _profesionalData.ObtenerPorId(dto.ProfesionalId);
                if (profesional == null)
                    return (false, "El profesional seleccionado no existe", 0);

                if (!profesional.Activo)
                    return (false, "El profesional seleccionado está inactivo", 0);

                // Validar día de la semana
                if (dto.DiaSemana < 1 || dto.DiaSemana > 7)
                    return (false, "El día de la semana debe estar entre 1 (Lunes) y 7 (Domingo)", 0);

                // Validar horarios
                if (dto.HoraInicio >= dto.HoraFin)
                    return (false, "La hora de inicio debe ser anterior a la hora de fin", 0);

                // Validar que el horario sea razonable (por ejemplo, entre 6:00 y 22:00)
                var horaMinima = new TimeSpan(6, 0, 0);
                var horaMaxima = new TimeSpan(22, 0, 0);

                if (dto.HoraInicio < horaMinima || dto.HoraInicio > horaMaxima)
                    return (false, "La hora de inicio debe estar entre las 06:00 y las 22:00", 0);

                if (dto.HoraFin < horaMinima || dto.HoraFin > horaMaxima)
                    return (false, "La hora de fin debe estar entre las 06:00 y las 22:00", 0);

                // Validar intervalo
                if (dto.IntervaloMinutos <= 0)
                    return (false, "El intervalo debe ser mayor a 0 minutos", 0);

                if (dto.IntervaloMinutos > 240)
                    return (false, "El intervalo no puede ser mayor a 240 minutos (4 horas)", 0);

                // Validar que el intervalo sea divisor razonable
                var intervalosValidos = new[] { 5, 10, 15, 20, 30, 45, 60, 90, 120, 180, 240 };
                if (!intervalosValidos.Contains(dto.IntervaloMinutos))
                    return (false, $"El intervalo debe ser uno de los siguientes valores: {string.Join(", ", intervalosValidos)} minutos", 0);

                // Validar vigencia
                if (dto.FechaVigenciaDesde < DateTime.Today.AddDays(-7))
                    return (false, "La fecha de vigencia desde no puede ser anterior a hace 7 días", 0);

                if (dto.FechaVigenciaHasta.HasValue && dto.FechaVigenciaHasta.Value < dto.FechaVigenciaDesde)
                    return (false, "La fecha de vigencia hasta debe ser posterior o igual a la fecha desde", 0);

                // Validar superposición de horarios para el mismo profesional y día
                var disponibilidadesExistentes = _disponibilidadAgendaData.ObtenerPorProfesional(dto.ProfesionalId, false)
                    .Where(d => d.DiaSemana == dto.DiaSemana && d.Activo).ToList();

                foreach (var existente in disponibilidadesExistentes)
                {
                    // Verificar si hay superposición de vigencia
                    bool haySuperposicionVigencia = false;

                    if (!dto.FechaVigenciaHasta.HasValue && !existente.FechaVigenciaHasta.HasValue)
                    {
                        // Ambos son indefinidos, siempre hay superposición
                        haySuperposicionVigencia = true;
                    }
                    else if (!dto.FechaVigenciaHasta.HasValue)
                    {
                        // El nuevo es indefinido, verificar si se solapa con el existente
                        haySuperposicionVigencia = dto.FechaVigenciaDesde <= existente.FechaVigenciaHasta.Value;
                    }
                    else if (!existente.FechaVigenciaHasta.HasValue)
                    {
                        // El existente es indefinido
                        haySuperposicionVigencia = dto.FechaVigenciaHasta.Value >= existente.FechaVigenciaDesde;
                    }
                    else
                    {
                        // Ambos tienen fecha de fin
                        haySuperposicionVigencia = dto.FechaVigenciaDesde <= existente.FechaVigenciaHasta.Value &&
                                                   dto.FechaVigenciaHasta.Value >= existente.FechaVigenciaDesde;
                    }

                    if (haySuperposicionVigencia)
                    {
                        // Verificar superposición de horarios
                        if (dto.HoraInicio < existente.HoraFin && dto.HoraFin > existente.HoraInicio)
                        {
                            return (false, $"Ya existe una disponibilidad para este profesional el día {existente.DiaSemanaTexto} " +
                                          $"en el horario {existente.HorarioFormateado} que se superpone con el horario ingresado " +
                                          $"durante el período de vigencia especificado", 0);
                        }
                    }
                }

                // Crear la disponibilidad
                int disponibilidadAgendaId = _disponibilidadAgendaData.Crear(dto);

                if (disponibilidadAgendaId > 0)
                {
                    return (true, "Disponibilidad de agenda creada exitosamente", disponibilidadAgendaId);
                }
                else
                {
                    return (false, "Error al crear la disponibilidad de agenda", 0);
                }
            }
            catch (Exception ex)
            {
                return (false, $"Error al crear la disponibilidad: {ex.Message}", 0);
            }
        }

        public (bool exitoso, string mensaje) Actualizar(ActualizarDisponibilidadAgendaDto dto)
        {
            try
            {
                // Validar que la disponibilidad exista
                var disponibilidadExistente = _disponibilidadAgendaData.ObtenerPorId(dto.DisponibilidadAgendaId);
                if (disponibilidadExistente == null)
                    return (false, "La disponibilidad no existe");

                // Realizar las mismas validaciones que en Crear
                if (dto.ProfesionalId <= 0)
                    return (false, "Debe seleccionar un profesional");

                var profesional = _profesionalData.ObtenerPorId(dto.ProfesionalId);
                if (profesional == null)
                    return (false, "El profesional seleccionado no existe");

                if (!profesional.Activo)
                    return (false, "El profesional seleccionado está inactivo");

                if (dto.DiaSemana < 1 || dto.DiaSemana > 7)
                    return (false, "El día de la semana debe estar entre 1 (Lunes) y 7 (Domingo)");

                if (dto.HoraInicio >= dto.HoraFin)
                    return (false, "La hora de inicio debe ser anterior a la hora de fin");

                var horaMinima = new TimeSpan(6, 0, 0);
                var horaMaxima = new TimeSpan(22, 0, 0);

                if (dto.HoraInicio < horaMinima || dto.HoraInicio > horaMaxima)
                    return (false, "La hora de inicio debe estar entre las 06:00 y las 22:00");

                if (dto.HoraFin < horaMinima || dto.HoraFin > horaMaxima)
                    return (false, "La hora de fin debe estar entre las 06:00 y las 22:00");

                if (dto.IntervaloMinutos <= 0)
                    return (false, "El intervalo debe ser mayor a 0 minutos");

                if (dto.IntervaloMinutos > 240)
                    return (false, "El intervalo no puede ser mayor a 240 minutos (4 horas)");

                var intervalosValidos = new[] { 5, 10, 15, 20, 30, 45, 60, 90, 120, 180, 240 };
                if (!intervalosValidos.Contains(dto.IntervaloMinutos))
                    return (false, $"El intervalo debe ser uno de los siguientes valores: {string.Join(", ", intervalosValidos)} minutos");

                if (dto.FechaVigenciaDesde < DateTime.Today.AddDays(-7))
                    return (false, "La fecha de vigencia desde no puede ser anterior a hace 7 días");

                if (dto.FechaVigenciaHasta.HasValue && dto.FechaVigenciaHasta.Value < dto.FechaVigenciaDesde)
                    return (false, "La fecha de vigencia hasta debe ser posterior o igual a la fecha desde");

                // Actualizar
                bool actualizado = _disponibilidadAgendaData.Actualizar(dto);

                if (actualizado)
                {
                    return (true, "Disponibilidad de agenda actualizada exitosamente");
                }
                else
                {
                    return (false, "Error al actualizar la disponibilidad de agenda");
                }
            }
            catch (Exception ex)
            {
                return (false, $"Error al actualizar la disponibilidad: {ex.Message}");
            }
        }

        public (bool exitoso, string mensaje) Eliminar(int disponibilidadAgendaId)
        {
            try
            {
                var disponibilidad = _disponibilidadAgendaData.ObtenerPorId(disponibilidadAgendaId);
                if (disponibilidad == null)
                    return (false, "La disponibilidad no existe");

                if (!disponibilidad.Activo)
                    return (false, "La disponibilidad ya está eliminada");

                bool eliminada = _disponibilidadAgendaData.Eliminar(disponibilidadAgendaId);

                if (eliminada)
                {
                    return (true, "Disponibilidad de agenda eliminada exitosamente");
                }
                else
                {
                    return (false, "Error al eliminar la disponibilidad de agenda");
                }
            }
            catch (Exception ex)
            {
                return (false, $"Error al eliminar la disponibilidad: {ex.Message}");
            }
        }

        public bool VerificarDisponibilidad(int profesionalId, DateTime fechaTurno, TimeSpan horaTurno, int duracionMinutos)
        {
            return _disponibilidadAgendaData.VerificarDisponibilidad(profesionalId, fechaTurno, horaTurno, duracionMinutos);
        }

        public List<TimeSpan> ObtenerHorariosLibres(int profesionalId, DateTime fechaTurno)
        {
            var horariosLibres = new List<TimeSpan>();

            var (disponibilidades, turnosOcupados) = _disponibilidadAgendaData.ObtenerHorariosDisponibles(profesionalId, fechaTurno);

            // Para cada disponibilidad, generar los slots
            foreach (var (HoraInicio, HoraFin, Intervalo) in disponibilidades)
            {
                var horaActual = HoraInicio;

                while (horaActual.Add(TimeSpan.FromMinutes(Intervalo)) <= HoraFin)
                {
                    // Verificar si este slot no está ocupado
                    bool estaOcupado = turnosOcupados.Any(turno =>
                        horaActual >= turno.HoraInicio && horaActual < turno.HoraFin);

                    if (!estaOcupado)
                    {
                        horariosLibres.Add(horaActual);
                    }

                    horaActual = horaActual.Add(TimeSpan.FromMinutes(Intervalo));
                }
            }

            return horariosLibres.OrderBy(h => h).ToList();
        }
    }
}
