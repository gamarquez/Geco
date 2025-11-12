using System;

namespace Entities
{
    /// <summary>
    /// DTO para actualizar una disponibilidad de agenda
    /// </summary>
    public class ActualizarDisponibilidadAgendaDto
    {
        public int DisponibilidadAgendaId { get; set; }
        public int ProfesionalId { get; set; }

        // Día de la semana (1=Lunes, 2=Martes, ..., 7=Domingo)
        public int DiaSemana { get; set; }

        public TimeSpan HoraInicio { get; set; }
        public TimeSpan HoraFin { get; set; }

        // Intervalo en minutos (15, 30, 60, etc.)
        public int IntervaloMinutos { get; set; }

        public DateTime FechaVigenciaDesde { get; set; }
        public DateTime? FechaVigenciaHasta { get; set; }
    }
}
