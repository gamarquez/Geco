using System;

namespace Entities
{
    /// <summary>
    /// DTO para creación de turnos
    /// </summary>
    public class CrearTurnoDto
    {
        public int PacienteId { get; set; }
        public int ProfesionalId { get; set; }
        public DateTime FechaTurno { get; set; }
        public TimeSpan HoraInicio { get; set; }
        public int DuracionMinutos { get; set; } = 30; // Duración por defecto
        public string MotivoConsulta { get; set; }
        public string Observaciones { get; set; }
        public string Estado { get; set; } = "Pendiente";
    }
}
