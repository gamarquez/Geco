using System;

namespace Entities
{
    /// <summary>
    /// DTO para mostrar las disponibilidades de agenda
    /// </summary>
    public class DisponibilidadAgendaDto
    {
        public int DisponibilidadAgendaId { get; set; }
        public int ProfesionalId { get; set; }

        // Día de la semana (1=Lunes, 2=Martes, ..., 7=Domingo)
        public int DiaSemana { get; set; }
        public TimeSpan HoraInicio { get; set; }
        public TimeSpan HoraFin { get; set; }
        public int IntervaloMinutos { get; set; }

        public DateTime FechaVigenciaDesde { get; set; }
        public DateTime? FechaVigenciaHasta { get; set; }

        public bool Activo { get; set; }
        public DateTime FechaAlta { get; set; }
        public DateTime? FechaModificacion { get; set; }

        // Datos del profesional
        public string ProfesionalNombre { get; set; }
        public string ProfesionalApellido { get; set; }
        public string ProfesionalMatricula { get; set; }
        public string ProfesionalEspecialidad { get; set; }

        // Propiedades calculadas
        public string ProfesionalNombreCompleto => $"{ProfesionalApellido}, {ProfesionalNombre}";
        public string DiaSemanaTexto => DiaSemana switch
        {
            1 => "Lunes",
            2 => "Martes",
            3 => "Miércoles",
            4 => "Jueves",
            5 => "Viernes",
            6 => "Sábado",
            7 => "Domingo",
            _ => "Desconocido"
        };
        public string HorarioFormateado => $"{HoraInicio:hh\\:mm} a {HoraFin:hh\\:mm}";
        public string IntervaloFormateado => $"Cada {IntervaloMinutos} min";
        public string VigenciaFormateada
        {
            get
            {
                if (FechaVigenciaHasta.HasValue)
                    return $"{FechaVigenciaDesde:dd/MM/yyyy} al {FechaVigenciaHasta:dd/MM/yyyy}";
                else
                    return $"Desde {FechaVigenciaDesde:dd/MM/yyyy} (indefinido)";
            }
        }
        public bool EstaVigente
        {
            get
            {
                var hoy = DateTime.Today;
                if (!Activo) return false;
                if (hoy < FechaVigenciaDesde) return false;
                if (FechaVigenciaHasta.HasValue && hoy > FechaVigenciaHasta.Value) return false;
                return true;
            }
        }
        public string EstadoBadgeClass => EstaVigente ? "bg-success" : "bg-secondary";
        public string EstadoTexto => EstaVigente ? "Vigente" : "No vigente";
    }
}
