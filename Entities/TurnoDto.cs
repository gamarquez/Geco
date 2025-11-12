using System;

namespace Entities
{
    /// <summary>
    /// DTO completo de Turno con información relacionada
    /// </summary>
    public class TurnoDto
    {
        public int TurnoId { get; set; }
        public int PacienteId { get; set; }
        public int ProfesionalId { get; set; }
        public DateTime FechaTurno { get; set; }
        public TimeSpan HoraInicio { get; set; }
        public TimeSpan HoraFin { get; set; }
        public int DuracionMinutos { get; set; }
        public string MotivoConsulta { get; set; }
        public string Estado { get; set; } // Pendiente, Confirmado, EnCurso, Completado, Cancelado, Ausente
        public string MotivoCancelacion { get; set; }
        public string Observaciones { get; set; }
        public bool Activo { get; set; }
        public DateTime FechaAlta { get; set; }
        public DateTime? FechaModificacion { get; set; }

        // Datos del paciente
        public string PacienteNombre { get; set; }
        public string PacienteApellido { get; set; }
        public string PacienteDocumento { get; set; }
        public string PacienteTelefono { get; set; }

        // Datos del profesional
        public string ProfesionalNombre { get; set; }
        public string ProfesionalApellido { get; set; }
        public string ProfesionalMatricula { get; set; }

        // Propiedades calculadas
        public string PacienteNombreCompleto => $"{PacienteApellido}, {PacienteNombre}";
        public string ProfesionalNombreCompleto => $"{ProfesionalApellido}, {ProfesionalNombre}";
        public DateTime FechaHoraInicio => FechaTurno.Date.Add(HoraInicio);
        public DateTime FechaHoraFin => FechaTurno.Date.Add(HoraFin);
        public string HorarioFormateado => $"{HoraInicio:hh\\:mm} - {HoraFin:hh\\:mm}";
        public string EstadoBadgeClass => Estado switch
        {
            "Pendiente" => "bg-warning",
            "Confirmado" => "bg-info",
            "EnCurso" => "bg-primary",
            "Completado" => "bg-success",
            "Cancelado" => "bg-danger",
            "Ausente" => "bg-secondary",
            _ => "bg-secondary"
        };
    }
}
